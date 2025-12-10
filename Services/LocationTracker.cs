using Bellwood.DriverApp.Models;
using Bellwood.DriverApp.Helpers;
using System.Collections.Concurrent;
using System.Net.Http.Json;

namespace Bellwood.DriverApp.Services;

/// <summary>
/// Implementation of location tracking using MAUI Geolocation and periodic timers.
/// Supports dynamic interval adjustment, retry logic, and background tracking.
/// </summary>
public class LocationTracker : ILocationTracker
{
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, TrackingSession> _activeSessions = new();

    public event EventHandler<LocationUpdateFailedEventArgs>? LocationUpdateFailed;
    public event EventHandler<TrackingStatusChangedEventArgs>? TrackingStatusChanged;
    public event EventHandler<LocationSentEventArgs>? LocationSent;

    /// <summary>
    /// Internal class to manage per-ride tracking state
    /// </summary>
    private class TrackingSession
    {
        public required string RideId { get; init; }
        public CancellationTokenSource Cts { get; } = new();
        public TrackingStatus Status { get; set; } = TrackingStatus.Active;
        public double? DestinationLatitude { get; set; }
        public double? DestinationLongitude { get; set; }
        public int CurrentIntervalSeconds { get; set; }
        public int ConsecutiveFailures { get; set; }
        public DateTime? LastSuccessfulUpdate { get; set; }
    }

    public LocationTracker(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("driver-admin");
    }

    public async Task<bool> StartTrackingAsync(string rideId, double? destinationLatitude = null, double? destinationLongitude = null)
    {
        if (_activeSessions.ContainsKey(rideId))
        {
            // Update destination if already tracking
            if (_activeSessions.TryGetValue(rideId, out var existingSession))
            {
                existingSession.DestinationLatitude = destinationLatitude;
                existingSession.DestinationLongitude = destinationLongitude;
            }
            return true;
        }

        var status = await CheckAndRequestLocationPermission();
        if (status != PermissionStatus.Granted)
        {
            RaiseTrackingStatusChanged(rideId, TrackingStatus.Inactive, TrackingStatus.PermissionRequired, 
                LocationConfig.GpsUnavailableMessage);
            RaiseLocationUpdateFailed(rideId, "Location permission denied", willRetry: false);
            return false;
        }

        var session = new TrackingSession
        {
            RideId = rideId,
            DestinationLatitude = destinationLatitude,
            DestinationLongitude = destinationLongitude,
            CurrentIntervalSeconds = LocationConfig.DefaultUpdateIntervalSeconds
        };

        if (!_activeSessions.TryAdd(rideId, session))
        {
            return false; // Race condition
        }

        RaiseTrackingStatusChanged(rideId, TrackingStatus.Inactive, TrackingStatus.Active, 
            LocationConfig.TrackingActiveMessage);

        // Start the tracking loop on a background task
        _ = Task.Run(async () => await TrackLocationLoopAsync(session), session.Cts.Token);

        return true;
    }

    public async Task StopTrackingAsync(string rideId)
    {
        if (_activeSessions.TryRemove(rideId, out var session))
        {
            var oldStatus = session.Status;
            session.Status = TrackingStatus.Inactive;
            
            try
            {
                await session.Cts.CancelAsync();
            }
            catch
            {
                // Ignore cancellation exceptions
            }
            finally
            {
                session.Cts.Dispose();
            }

            RaiseTrackingStatusChanged(rideId, oldStatus, TrackingStatus.Inactive, "Tracking stopped");
        }
    }

    public async Task StopAllTrackingAsync()
    {
        var rideIds = _activeSessions.Keys.ToList();
        foreach (var rideId in rideIds)
        {
            await StopTrackingAsync(rideId);
        }
    }

    public bool IsTracking(string rideId) =>
        _activeSessions.TryGetValue(rideId, out var session) && session.Status == TrackingStatus.Active;

    public TrackingStatus GetTrackingStatus(string rideId)
    {
        if (_activeSessions.TryGetValue(rideId, out var session))
        {
            return session.Status;
        }
        return TrackingStatus.Inactive;
    }

    public void UpdateDestination(string rideId, double latitude, double longitude)
    {
        if (_activeSessions.TryGetValue(rideId, out var session))
        {
            session.DestinationLatitude = latitude;
            session.DestinationLongitude = longitude;
            Console.WriteLine($"Updated destination for ride {rideId}: ({latitude}, {longitude})");
        }
    }

    private async Task TrackLocationLoopAsync(TrackingSession session)
    {
        try
        {
            // Send first update immediately
            await SendLocationUpdateWithRetryAsync(session);

            while (!session.Cts.Token.IsCancellationRequested)
            {
                // Wait for the current interval
                await Task.Delay(TimeSpan.FromSeconds(session.CurrentIntervalSeconds), session.Cts.Token);

                if (session.Cts.Token.IsCancellationRequested)
                    break;

                await SendLocationUpdateWithRetryAsync(session);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation - tracking was stopped
            Console.WriteLine($"Location tracking stopped for ride {session.RideId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Location tracking error for ride {session.RideId}: {ex.Message}");
            session.Status = TrackingStatus.Error;
            RaiseTrackingStatusChanged(session.RideId, TrackingStatus.Active, TrackingStatus.Error, ex.Message);
            RaiseLocationUpdateFailed(session.RideId, ex.Message, willRetry: false);
        }
    }

    private async Task SendLocationUpdateWithRetryAsync(TrackingSession session)
    {
        var retryCount = 0;
        var success = false;

        while (retryCount <= LocationConfig.MaxRetryAttempts && !success && !session.Cts.Token.IsCancellationRequested)
        {
            try
            {
                success = await SendLocationUpdateAsync(session);

                if (success)
                {
                    session.ConsecutiveFailures = 0;
                    session.LastSuccessfulUpdate = DateTime.UtcNow;

                    if (session.Status != TrackingStatus.Active)
                    {
                        var oldStatus = session.Status;
                        session.Status = TrackingStatus.Active;
                        RaiseTrackingStatusChanged(session.RideId, oldStatus, TrackingStatus.Active, 
                            LocationConfig.TrackingActiveMessage);
                    }
                }
                else
                {
                    retryCount++;
                    if (retryCount <= LocationConfig.MaxRetryAttempts)
                    {
                        RaiseLocationUpdateFailed(session.RideId, "Location update failed", 
                            willRetry: true, retryCount: retryCount);
                        await Task.Delay(LocationConfig.RetryDelayMs, session.Cts.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw; // Re-throw cancellation
            }
            catch (Exception ex)
            {
                retryCount++;
                Console.WriteLine($"Location update attempt {retryCount} failed: {ex.Message}");
                
                if (retryCount <= LocationConfig.MaxRetryAttempts)
                {
                    RaiseLocationUpdateFailed(session.RideId, ex.Message, 
                        willRetry: true, retryCount: retryCount);
                    await Task.Delay(LocationConfig.RetryDelayMs, session.Cts.Token);
                }
            }
        }

        if (!success)
        {
            session.ConsecutiveFailures++;
            RaiseLocationUpdateFailed(session.RideId, "Location update failed after retries", 
                willRetry: false, retryCount: retryCount);

            // If we've had multiple consecutive failures, update status
            if (session.ConsecutiveFailures >= 3 && session.Status == TrackingStatus.Active)
            {
                session.Status = TrackingStatus.Error;
                RaiseTrackingStatusChanged(session.RideId, TrackingStatus.Active, TrackingStatus.Error,
                    "Multiple location updates failed. Check network connection.");
            }
        }
    }

    private async Task<bool> SendLocationUpdateAsync(TrackingSession session)
    {
        var location = await Geolocation.GetLocationAsync(new GeolocationRequest
        {
            DesiredAccuracy = GeolocationAccuracy.Best,
            Timeout = TimeSpan.FromSeconds(LocationConfig.LocationTimeoutSeconds)
        }, session.Cts.Token);

        if (location == null)
        {
            Console.WriteLine("Failed to get location from device");
            return false;
        }

        // Calculate dynamic interval based on proximity to destination
        UpdateIntervalBasedOnProximity(session, location.Latitude, location.Longitude);

        var update = new LocationUpdate
        {
            RideId = session.RideId,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Timestamp = DateTime.UtcNow,
            Heading = location.Course,  // Course is the heading in degrees
            Speed = location.Speed,      // Speed in meters/second
            Accuracy = location.Accuracy
        };

        var response = await _httpClient.PostAsJsonAsync("/driver/location/update", update, session.Cts.Token);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Location sent for ride {session.RideId}: ({location.Latitude}, {location.Longitude})");
            
            LocationSent?.Invoke(this, new LocationSentEventArgs
            {
                RideId = session.RideId,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Timestamp = update.Timestamp,
                CurrentIntervalSeconds = session.CurrentIntervalSeconds
            });
            
            return true;
        }

        var statusCode = response.StatusCode;
        Console.WriteLine($"Location update failed with status: {statusCode}");

        // If we get a 400 Bad Request, the ride may be invalid - stop tracking
        if (statusCode == System.Net.HttpStatusCode.BadRequest)
        {
            Console.WriteLine($"Stopping tracking for ride {session.RideId} due to bad request");
            _ = StopTrackingAsync(session.RideId);
        }
        // If we get 401 Unauthorized, token may have expired
        else if (statusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            session.Status = TrackingStatus.Error;
            RaiseTrackingStatusChanged(session.RideId, TrackingStatus.Active, TrackingStatus.Error,
                "Authentication failed. Please log in again.");
        }

        return false;
    }

    private void UpdateIntervalBasedOnProximity(TrackingSession session, double currentLat, double currentLon)
    {
        if (!session.DestinationLatitude.HasValue || !session.DestinationLongitude.HasValue)
        {
            session.CurrentIntervalSeconds = LocationConfig.DefaultUpdateIntervalSeconds;
            return;
        }

        var distance = CalculateDistanceMeters(
            currentLat, currentLon,
            session.DestinationLatitude.Value, session.DestinationLongitude.Value);

        var previousInterval = session.CurrentIntervalSeconds;

        // Use faster updates when close to destination
        session.CurrentIntervalSeconds = distance <= LocationConfig.ProximityDistanceMeters
            ? LocationConfig.ProximityUpdateIntervalSeconds
            : LocationConfig.DefaultUpdateIntervalSeconds;

        if (previousInterval != session.CurrentIntervalSeconds)
        {
            Console.WriteLine($"Update interval changed to {session.CurrentIntervalSeconds}s " +
                             $"(distance: {distance:F0}m)");
        }
    }

    /// <summary>
    /// Calculates the distance between two coordinates using the Haversine formula
    /// </summary>
    private static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double EarthRadiusMeters = 6371000;

        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMeters * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;

    private async Task<PermissionStatus> CheckAndRequestLocationPermission()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Granted)
            return status;

        if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
        {
            // On iOS, if permission is denied, we can't request again
            return status;
        }

        // Show rationale if available
        if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Shell.Current.DisplayAlert(
                    "Location Permission Required",
                    LocationConfig.PermissionRationale,
                    "OK");
            });
        }

        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        
        // Also request background location for continuous tracking
        if (status == PermissionStatus.Granted)
        {
            var bgStatus = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
            if (bgStatus != PermissionStatus.Granted)
            {
                // On Android, we can request "always" permission for background tracking
                // This helps when the app is minimized during a ride
                bgStatus = await Permissions.RequestAsync<Permissions.LocationAlways>();
                Console.WriteLine($"Background location permission: {bgStatus}");
            }
        }
        
        return status;
    }

    private void RaiseLocationUpdateFailed(string rideId, string message, bool willRetry, int retryCount = 0)
    {
        LocationUpdateFailed?.Invoke(this, new LocationUpdateFailedEventArgs
        {
            RideId = rideId,
            ErrorMessage = message,
            WillRetry = willRetry,
            RetryCount = retryCount
        });
    }

    private void RaiseTrackingStatusChanged(string rideId, TrackingStatus oldStatus, TrackingStatus newStatus, string? message = null)
    {
        TrackingStatusChanged?.Invoke(this, new TrackingStatusChangedEventArgs
        {
            RideId = rideId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Message = message
        });
    }
}
