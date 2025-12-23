using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Bellwood.DriverApp.Models;
using Bellwood.DriverApp.Services;
using Bellwood.DriverApp.Helpers;

namespace Bellwood.DriverApp.ViewModels;

/// <summary>
/// ViewModel for ride detail page with status updates and navigation
/// </summary>
[QueryProperty(nameof(RideId), "rideId")]
public partial class RideDetailViewModel : BaseViewModel
{
    private readonly IRideService _rideService;
    private readonly ILocationTracker _locationTracker;

    [ObservableProperty]
    private DriverRideDetailDto? _ride;

    [ObservableProperty]
    private string? _rideId;

    [ObservableProperty]
    private bool _isTracking;

    [ObservableProperty]
    private string _trackingStatusMessage = string.Empty;

    [ObservableProperty]
    private bool _hasTrackingError;

    [ObservableProperty]
    private bool _showTrackingIndicator;

    // Computed visibility properties for status buttons
    [ObservableProperty]
    private bool _canTransitionToOnRoute;

    [ObservableProperty]
    private bool _canTransitionToArrived;

    [ObservableProperty]
    private bool _canTransitionToPassengerOnboard;

    [ObservableProperty]
    private bool _canTransitionToCompleted;

    [ObservableProperty]
    private bool _canCancel;

    public RideDetailViewModel(IRideService rideService, ILocationTracker locationTracker)
    {
        _rideService = rideService;
        _locationTracker = locationTracker;
        this.Title = "Ride Details";

        // Subscribe to location tracking events
        _locationTracker.TrackingStatusChanged += OnTrackingStatusChanged;
        _locationTracker.LocationUpdateFailed += OnLocationUpdateFailed;
        _locationTracker.LocationSent += OnLocationSent;
    }

    partial void OnRideIdChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _ = LoadRideDetailsAsync();
        }
    }

    partial void OnRideChanged(DriverRideDetailDto? value)
    {
        UpdateCanTransitionProperties();
        UpdateTrackingIndicator();
    }

    private void UpdateCanTransitionProperties()
    {
        CanTransitionToOnRoute = Ride?.Status == RideStatus.Scheduled;
        CanTransitionToArrived = Ride?.Status == RideStatus.OnRoute;
        CanTransitionToPassengerOnboard = Ride?.Status == RideStatus.Arrived;
        CanTransitionToCompleted = Ride?.Status == RideStatus.PassengerOnboard;
        CanCancel = Ride != null && Ride.Status != RideStatus.Completed && Ride.Status != RideStatus.Cancelled;
    }

    private void UpdateTrackingIndicator()
    {
        if (string.IsNullOrEmpty(RideId) || Ride == null)
        {
            ShowTrackingIndicator = false;
            return;
        }

        // Show tracking indicator only for active ride statuses
        var isActiveRide = Ride.Status == RideStatus.OnRoute || 
                          Ride.Status == RideStatus.Arrived || 
                          Ride.Status == RideStatus.PassengerOnboard;

        ShowTrackingIndicator = isActiveRide;
        IsTracking = _locationTracker.IsTracking(RideId);

        if (isActiveRide && IsTracking)
        {
            TrackingStatusMessage = LocationConfig.TrackingActiveMessage;
            HasTrackingError = false;
        }
        else if (isActiveRide && !IsTracking)
        {
            // Tracking should be active but isn't
            var status = _locationTracker.GetTrackingStatus(RideId);
            if (status == TrackingStatus.PermissionRequired)
            {
                TrackingStatusMessage = LocationConfig.GpsUnavailableMessage;
                HasTrackingError = true;
            }
            else
            {
                TrackingStatusMessage = "Location tracking inactive";
                HasTrackingError = true;
            }
        }
    }

    private async Task LoadRideDetailsAsync()
    {
        if (string.IsNullOrEmpty(this.RideId))
            return;

        try
        {
            this.IsBusy = true;
            ClearError();

            this.Ride = await _rideService.GetRideDetailsAsync(this.RideId);

            if (this.Ride == null)
            {
                SetError("Ride not found");
                return;
            }

            // Update title with passenger name
            this.Title = $"Ride for {this.Ride.PassengerName}";

            // Check if location tracking is active and update status
            this.IsTracking = _locationTracker.IsTracking(this.RideId);
            UpdateTrackingIndicator();

            // If the ride is already in an active state, ensure tracking is started
            if (Ride.Status == RideStatus.OnRoute || 
                Ride.Status == RideStatus.Arrived || 
                Ride.Status == RideStatus.PassengerOnboard)
            {
                await ResumeTrackingIfNeeded();
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load ride details: {ex.Message}");
        }
        finally
        {
            this.IsBusy = false;
        }
    }

    /// <summary>
    /// Resumes tracking if it should be active but isn't (e.g., after app restart)
    /// </summary>
    private async Task ResumeTrackingIfNeeded()
    {
        if (string.IsNullOrEmpty(RideId) || Ride == null)
            return;

        if (!_locationTracker.IsTracking(RideId))
        {
            // Determine destination coordinates based on current status
            var (destLat, destLon) = await GetDestinationCoordinatesAsync();
            
            var started = await _locationTracker.StartTrackingAsync(RideId, destLat, destLon);
            IsTracking = started;
            UpdateTrackingIndicator();

            if (!started)
            {
                Console.WriteLine($"Failed to resume tracking for ride {RideId}");
            }
        }
    }

    /// <summary>
    /// Gets destination coordinates for proximity-based tracking
    /// Returns pickup coords when OnRoute, dropoff coords when PassengerOnboard
    /// </summary>
    private async Task<(double? lat, double? lon)> GetDestinationCoordinatesAsync()
    {
        if (Ride == null)
            return (null, null);

        try
        {
            string targetAddress = Ride.Status switch
            {
                RideStatus.OnRoute or RideStatus.Arrived => Ride.PickupLocation,
                RideStatus.PassengerOnboard => Ride.DropoffLocation,
                _ => Ride.PickupLocation
            };

            // Try to geocode the address to get coordinates
            var locations = await Geocoding.GetLocationsAsync(targetAddress);
            var location = locations?.FirstOrDefault();

            if (location != null)
            {
                return (location.Latitude, location.Longitude);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to geocode destination: {ex.Message}");
        }

        return (null, null);
    }

    [RelayCommand]
    private async Task UpdateStatusAsync(string statusString)
    {
        if (this.Ride == null || this.IsBusy)
            return;

        if (!Enum.TryParse<RideStatus>(statusString, out var newStatus))
            return;

        // Confirm status change
        var confirmed = await Shell.Current.DisplayAlert(
            "Update Status",
            $"Change ride status to {GetStatusDisplayName(newStatus)}?",
            "Yes",
            "No");

        if (!confirmed)
            return;

        try
        {
            this.IsBusy = true;
            ClearError();

            var result = await _rideService.UpdateRideStatusAsync(this.Ride.Id, newStatus);

            if (result.Success)
            {
                // Update local ride status and trigger property changed
                this.Ride.Status = newStatus;
                
                // Manually trigger property updates since we're modifying the object not replacing it
                OnPropertyChanged(nameof(Ride));
                UpdateCanTransitionProperties();

                // Handle location tracking based on new status
                await HandleLocationTracking(newStatus);

                await Shell.Current.DisplayAlert("Success", "Ride status updated", "OK");
            }
            else
            {
                SetError(result.ErrorMessage ?? "Failed to update status");
                await Shell.Current.DisplayAlert("Error", result.ErrorMessage ?? "Failed to update status", "OK");
            }
        }
        catch (Exception ex)
        {
            SetError($"Error updating status: {ex.Message}");
        }
        finally
        {
            this.IsBusy = false;
        }
    }

    private async Task HandleLocationTracking(RideStatus status)
    {
        if (string.IsNullOrEmpty(this.RideId))
            return;

        // Start tracking when ride becomes active
        if (status == RideStatus.OnRoute || status == RideStatus.Arrived || status == RideStatus.PassengerOnboard)
        {
            if (!this.IsTracking)
            {
                var (destLat, destLon) = await GetDestinationCoordinatesAsync();
                var started = await _locationTracker.StartTrackingAsync(this.RideId, destLat, destLon);
                this.IsTracking = started;
                UpdateTrackingIndicator();

                if (!started)
                {
                    await Shell.Current.DisplayAlert(
                        "Location Tracking",
                        "Unable to start location tracking. Please check location permissions.",
                        "OK");
                }
            }
            else if (status == RideStatus.PassengerOnboard)
            {
                // Update destination to dropoff location when passenger is onboard
                var (destLat, destLon) = await GetDestinationCoordinatesAsync();
                if (destLat.HasValue && destLon.HasValue)
                {
                    _locationTracker.UpdateDestination(this.RideId, destLat.Value, destLon.Value);
                }
            }
        }
        // Stop tracking when ride is completed or cancelled
        else if (status == RideStatus.Completed || status == RideStatus.Cancelled)
        {
            await _locationTracker.StopTrackingAsync(this.RideId);
            this.IsTracking = false;
            UpdateTrackingIndicator();
        }
    }

    private void OnTrackingStatusChanged(object? sender, TrackingStatusChangedEventArgs e)
    {
        if (e.RideId != RideId)
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsTracking = e.NewStatus == TrackingStatus.Active;
            HasTrackingError = e.NewStatus == TrackingStatus.Error || e.NewStatus == TrackingStatus.PermissionRequired;
            
            if (!string.IsNullOrEmpty(e.Message))
            {
                TrackingStatusMessage = e.Message;
            }

            UpdateTrackingIndicator();
        });
    }

    private void OnLocationUpdateFailed(object? sender, LocationUpdateFailedEventArgs e)
    {
        if (e.RideId != RideId)
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!e.WillRetry)
            {
                HasTrackingError = true;
                TrackingStatusMessage = $"Location update failed: {e.ErrorMessage}";
            }
        });
    }

    private void OnLocationSent(object? sender, LocationSentEventArgs e)
    {
        if (e.RideId != RideId)
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Reset error state on successful send
            if (HasTrackingError)
            {
                HasTrackingError = false;
                TrackingStatusMessage = LocationConfig.TrackingActiveMessage;
            }
        });
    }

    [RelayCommand]
    private async Task NavigateToPickupAsync()
    {
        if (this.Ride?.PickupLocation == null)
            return;

        await LaunchMapsAsync(this.Ride.PickupLocation);
    }

    [RelayCommand]
    private async Task NavigateToDropoffAsync()
    {
        if (this.Ride?.DropoffLocation == null)
            return;

        await LaunchMapsAsync(this.Ride.DropoffLocation);
    }

    private async Task LaunchMapsAsync(string address)
    {
        try
        {
            // Use platform-specific map URI scheme
            var escapedAddress = Uri.EscapeDataString(address);
            var uri = DeviceInfo.Platform == DevicePlatform.iOS
                ? new Uri($"http://maps.apple.com/?q={escapedAddress}")
                : new Uri($"geo:0,0?q={escapedAddress}");

            await Launcher.OpenAsync(uri);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Unable to open maps: {ex.Message}", "OK");
        }
    }

    private string GetStatusDisplayName(RideStatus status)
    {
        return status switch
        {
            RideStatus.Scheduled => "Scheduled",
            RideStatus.OnRoute => "On Route",
            RideStatus.Arrived => "Arrived",
            RideStatus.PassengerOnboard => "Passenger Onboard",
            RideStatus.Completed => "Completed",
            RideStatus.Cancelled => "Cancelled",
            _ => status.ToString()
        };
    }

    /// <summary>
    /// Cleanup event subscriptions
    /// </summary>
    public void Cleanup()
    {
        _locationTracker.TrackingStatusChanged -= OnTrackingStatusChanged;
        _locationTracker.LocationUpdateFailed -= OnLocationUpdateFailed;
        _locationTracker.LocationSent -= OnLocationSent;
    }
}
