using CommunityToolkit.Mvvm.ComponentModel;

namespace Bellwood.DriverApp.ViewModels;

/// <summary>
/// Base class for all ViewModels providing common MVVM functionality
/// </summary>
public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private string? _errorMessage;

    protected void ClearError()
    {
        this.ErrorMessage = null;
    }

    protected void SetError(string message)
    {
        this.ErrorMessage = message;
    }
}
