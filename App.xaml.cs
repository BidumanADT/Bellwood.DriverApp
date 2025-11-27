using Bellwood.DriverApp.Services;

namespace Bellwood.DriverApp
{
    public partial class App : Application
    {
        private readonly IAuthService _authService;

        public App(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;

            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            base.OnStart();

            // Check if user is authenticated
            var isAuthenticated = await _authService.IsAuthenticatedAsync();

            if (isAuthenticated)
            {
                // Navigate to home page
                await Shell.Current.GoToAsync("//HomePage");
            }
            else
            {
                // Navigate to login page
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }
    }
}
