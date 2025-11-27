using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Bellwood.DriverApp.Models;
using Bellwood.DriverApp.Services;

namespace Bellwood.DriverApp.ViewModels;

/// <summary>
/// ViewModel for the home page showing today's rides
/// </summary>
public partial class HomeViewModel : BaseViewModel
{
    private readonly IRideService _rideService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private ObservableCollection<DriverRideListItemDto> _rides = new();

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _hasNoRides;

    public HomeViewModel(IRideService rideService, IAuthService authService)
    {
        _rideService = rideService;
        _authService = authService;
        this.Title = "Today's Rides";
    }

    public async Task InitializeAsync()
    {
        await LoadRidesAsync();
    }

    [RelayCommand]
    private async Task LoadRidesAsync()
    {
        if (this.IsBusy)
            return;

        try
        {
            this.IsBusy = true;
            ClearError();

            var rides = await _rideService.GetTodaysRidesAsync();
            
            this.Rides.Clear();
            foreach (var ride in rides)
            {
                this.Rides.Add(ride);
            }

            this.HasNoRides = this.Rides.Count == 0;
        }
        catch (Exception ex)
        {
            SetError($"Failed to load rides: {ex.Message}");
        }
        finally
        {
            this.IsBusy = false;
            this.IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        this.IsRefreshing = true;
        await LoadRidesAsync();
    }

    [RelayCommand]
    private async Task SelectRideAsync(DriverRideListItemDto ride)
    {
        if (ride == null)
            return;

        await Shell.Current.GoToAsync($"RideDetailPage?rideId={ride.Id}");
    }

    [RelayCommand]
    private async Task SignOutAsync()
    {
        var confirmed = await Shell.Current.DisplayAlert(
            "Sign Out",
            "Are you sure you want to sign out?",
            "Yes",
            "No");

        if (!confirmed)
            return;

        await _authService.SignOutAsync();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
