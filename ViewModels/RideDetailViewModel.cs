using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Bellwood.DriverApp.Models;
using Bellwood.DriverApp.Services;

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
    [NotifyPropertyChangedFor(nameof(CanTransitionToOnRoute), nameof(CanTransitionToArrived), 
                              nameof(CanTransitionToPassengerOnboard), nameof(CanTransitionToCompleted), 
                              nameof(CanCancel))]
    private DriverRideDetailDto? _ride;

    [ObservableProperty]
    private string? _rideId;

    [ObservableProperty]
    private bool _isTracking;

    public RideDetailViewModel(IRideService rideService, ILocationTracker locationTracker)
    {
        _rideService = rideService;
        _locationTracker = locationTracker;
        this.Title = "Ride Details";
    }

    partial void OnRideIdChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _ = LoadRideDetailsAsync();
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

            // Check if location tracking is active
            this.IsTracking = _locationTracker.IsTracking(this.RideId);
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
                // Update local ride status
                this.Ride.Status = newStatus;
                OnPropertyChanged(nameof(Ride));

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
                var started = await _locationTracker.StartTrackingAsync(this.RideId);
                this.IsTracking = started;

                if (!started)
                {
                    await Shell.Current.DisplayAlert(
                        "Location Tracking",
                        "Unable to start location tracking. Please check location permissions.",
                        "OK");
                }
            }
        }
        // Stop tracking when ride is completed or cancelled
        else if (status == RideStatus.Completed || status == RideStatus.Cancelled)
        {
            await _locationTracker.StopTrackingAsync(this.RideId);
            this.IsTracking = false;
        }
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

    [RelayCommand]
    private async Task CallPassengerAsync()
    {
        if (string.IsNullOrEmpty(this.Ride?.PassengerPhone))
        {
            await Shell.Current.DisplayAlert("No Phone", "No phone number available for this passenger", "OK");
            return;
        }

        try
        {
            PhoneDialer.Open(this.Ride.PassengerPhone);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Unable to open phone dialer: {ex.Message}", "OK");
        }
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

    // Show which status buttons should be visible based on current status
    public bool CanTransitionToOnRoute => this.Ride?.Status == RideStatus.Scheduled;
    public bool CanTransitionToArrived => this.Ride?.Status == RideStatus.OnRoute;
    public bool CanTransitionToPassengerOnboard => this.Ride?.Status == RideStatus.Arrived;
    public bool CanTransitionToCompleted => this.Ride?.Status == RideStatus.PassengerOnboard;
    public bool CanCancel => this.Ride?.Status != RideStatus.Completed && this.Ride?.Status != RideStatus.Cancelled;
}
