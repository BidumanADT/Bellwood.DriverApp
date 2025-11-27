using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Bellwood.DriverApp.Services;

namespace Bellwood.DriverApp.ViewModels;

/// <summary>
/// ViewModel for the login page
/// </summary>
public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        this.Title = "Bellwood Driver Login";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (this.IsBusy)
            return;

        ClearError();

        // Validation
        if (string.IsNullOrWhiteSpace(this.Username))
        {
            SetError("Please enter your username");
            return;
        }

        if (string.IsNullOrWhiteSpace(this.Password))
        {
            SetError("Please enter your password");
            return;
        }

        try
        {
            this.IsBusy = true;

            var result = await _authService.LoginAsync(this.Username, this.Password);

            if (result.Success)
            {
                // Navigate to home page
                await Shell.Current.GoToAsync("//HomePage");
            }
            else
            {
                SetError(result.ErrorMessage ?? "Login failed");
            }
        }
        catch (Exception ex)
        {
            SetError($"An error occurred: {ex.Message}");
        }
        finally
        {
            this.IsBusy = false;
        }
    }
}
