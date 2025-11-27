using Bellwood.DriverApp.ViewModels;

namespace Bellwood.DriverApp.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
