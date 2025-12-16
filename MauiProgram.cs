using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Bellwood.DriverApp.Services;
using Bellwood.DriverApp.ViewModels;
using Bellwood.DriverApp.Views;
using Bellwood.DriverApp.Handlers;

namespace Bellwood.DriverApp;

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

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // -------- Views / ViewModels --------
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<RideDetailPage>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<RideDetailViewModel>();

        // -------- Services --------
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IRideService, RideService>();
        builder.Services.AddSingleton<ILocationTracker, LocationTracker>();

        // HTTP Message Handlers
        builder.Services.AddTransient<AuthHttpHandler>();
        builder.Services.AddTransient<TimezoneHttpHandler>();

        // ===== HTTP CLIENTS =====

        // 1. Auth Server client (login)
        builder.Services.AddHttpClient("auth", c =>
        {
#if ANDROID
            c.BaseAddress = new Uri("https://10.0.2.2:5001");
#elif IOS || MACCATALYST
            c.BaseAddress = new Uri("https://localhost:5001");
#else
            c.BaseAddress = new Uri("https://localhost:5001");
#endif
            c.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddHttpMessageHandler<TimezoneHttpHandler>()  // Add timezone header
#if DEBUG
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            // DEV ONLY: trust local dev certs
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
#else
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler());
#endif

        // 2. AdminAPI client for driver endpoints (protected)
        builder.Services.AddHttpClient("driver-admin", c =>
        {
#if ANDROID
            c.BaseAddress = new Uri("https://10.0.2.2:5206");
#elif IOS || MACCATALYST
            c.BaseAddress = new Uri("https://localhost:5206");
#else
            c.BaseAddress = new Uri("https://localhost:5206");
#endif
            c.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddHttpMessageHandler<TimezoneHttpHandler>()  // Add timezone header first
        .AddHttpMessageHandler<AuthHttpHandler>()       // Then add auth header
#if DEBUG
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            // DEV ONLY: trust local dev certs
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
#else
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler());
#endif

        return builder.Build();
    }
}
