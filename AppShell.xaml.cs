using Bellwood.DriverApp.Views;

namespace Bellwood.DriverApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for navigation
            Routing.RegisterRoute("RideDetailPage", typeof(RideDetailPage));
        }
    }
}
