using Bellwood.DriverApp.ViewModels;

namespace Bellwood.DriverApp.Views;

public partial class RideDetailPage : ContentPage
{
    public RideDetailPage(RideDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (BindingContext is RideDetailViewModel viewModel)
        {
            viewModel.Cleanup();
        }
    }
}
