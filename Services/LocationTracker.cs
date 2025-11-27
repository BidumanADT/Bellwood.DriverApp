using System.Collections.Concurrent;
using System.Net.Http.Json;
using Bellwood.DriverApp.Helpers;
using Bellwood.DriverApp.Models;

namespace Bellwood.DriverApp.Services;

/// <summary>
/// Implementation of location tracking using MAUI Geolocation and periodic timers
/// </summary>
public class LocationTracker : ILocationTracker
{
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeTrackers = new();
    
    public event EventHandler<string>? LocationUpdateFailed;

    public LocationTracker(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(AppSettings.AdminApiBaseUrl);
    }

    public async Task<bool> StartTrackingAsync(string rideId)
    {
        // Check if already tracking this ride
        if (_activeTrackers.ContainsKey(rideId))
        {
            return true; // Already tracking
        }

        // Request location permissions
        var status = await CheckAndRequestLocationPermission();
        if (status != PermissionStatus.Granted)
        {
            LocationUpdateFailed?.Invoke(this, "Location permission denied");
            return false;
        }

        // Create cancellation token for this ride's tracking
        var cts = new CancellationTokenSource();
        
        if (!_activeTrackers.TryAdd(rideId, cts))
        {
            return false; // Race condition, already added
        }

        // Start background tracking loop
        _ = Task.Run(async () => await TrackLocationLoopAsync(rideId, cts.Token), cts.Token);

        return true;
    }

    public async Task StopTrackingAsync(string rideId)
    {
        if (_activeTrackers.TryRemove(rideId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
        
        await Task.CompletedTask;
    }

    public async Task StopAllTrackingAsync()
    {
        foreach (var kvp in _activeTrackers)
        {
            kvp.Value.Cancel();
            kvp.Value.Dispose();
        }
        
        _activeTrackers.Clear();
        await Task.CompletedTask;
    }

    public bool IsTracking(string rideId)
    {
        return _activeTrackers.ContainsKey(rideId);
    }

    private async Task TrackLocationLoopAsync(string rideId, CancellationToken cancellationToken)
    {
        var interval = TimeSpan.FromSeconds(LocationConfig.UpdateIntervalSeconds);
        using var timer = new PeriodicTimer(interval);

        try
        {
            // Send first update immediately
            await SendLocationUpdateAsync(rideId, cancellationToken);

            // Then send updates at configured interval
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await SendLocationUpdateAsync(rideId, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation, exit gracefully
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Location tracking error for ride {rideId}: {ex.Message}");
            LocationUpdateFailed?.Invoke(this, ex.Message);
        }
    }

    private async Task SendLocationUpdateAsync(string rideId, CancellationToken cancellationToken)
    {
        try
        {
            // Get current location
            var location = await Geolocation.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Best,
                Timeout = TimeSpan.FromSeconds(10)
            }, cancellationToken);

            if (location == null)
            {
                Console.WriteLine("Failed to get location");
                return;
            }

            // Send to server
            var update = new LocationUpdate
            {
                RideId = rideId,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Timestamp = DateTime.UtcNow
            };

            var response = await _httpClient.PostAsJsonAsync("/driver/location/update", update, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var statusCode = response.StatusCode;
                Console.WriteLine($"Location update failed: {statusCode}");
                
                // If server says ride isn't active, stop tracking
                if (statusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    await StopTrackingAsync(rideId);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during cancellation
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending location update: {ex.Message}");
        }
    }

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

        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        return status;
    }
}
