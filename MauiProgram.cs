using Microsoft.Extensions.Logging;
using Bellwood.DriverApp.Services;
using Bellwood.DriverApp.ViewModels;
using Bellwood.DriverApp.Views;
using Bellwood.DriverApp.Handlers;

namespace Bellwood.DriverApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register HTTP Clients with platform-specific configuration
            RegisterHttpClients(builder.Services);

            // Register Services
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<ILocationTracker, LocationTracker>();

            // Register ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<RideDetailViewModel>();

            // Register Views
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<RideDetailPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void RegisterHttpClients(IServiceCollection services)
        {
            // Create HttpClientHandler for development (accepts self-signed certs)
#if DEBUG
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
#else
            var handler = new HttpClientHandler();
#endif

            // Register AuthService HttpClient (no auth handler needed for login endpoint)
            services.AddHttpClient<IAuthService, AuthService>()
                .ConfigurePrimaryHttpMessageHandler(() => handler);

            // Register HttpClient with AuthHttpHandler for authenticated endpoints
            services.AddTransient<AuthHttpHandler>();

            services.AddHttpClient<IRideService, RideService>()
                .ConfigurePrimaryHttpMessageHandler(() => handler)
                .AddHttpMessageHandler<AuthHttpHandler>();

            services.AddHttpClient<ILocationTracker, LocationTracker>()
                .ConfigurePrimaryHttpMessageHandler(() => handler)
                .AddHttpMessageHandler<AuthHttpHandler>();
        }
    }
}
