using Bellwood.DriverApp.ViewModels;

namespace Bellwood.DriverApp.Views;

public partial class RideDetailPage : ContentPage
{
    public RideDetailPage(RideDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
