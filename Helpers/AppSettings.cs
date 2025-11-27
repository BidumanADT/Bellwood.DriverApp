namespace Bellwood.DriverApp.Helpers;

/// <summary>
/// Configuration settings for API endpoints and app behavior
/// </summary>
public static class AppSettings
{
    /// <summary>
    /// Determines if we're in development mode (using Android emulator loopback)
    /// </summary>
#if DEBUG
    public static bool IsDevelopment => true;
#else
    public static bool IsDevelopment => false;
#endif

    /// <summary>
    /// Base URL for AuthServer
    /// Development: AuthServer runs on https://localhost:5001
    /// Android emulator must use 10.0.2.2 to access host machine's localhost
    /// </summary>
    public static string AuthServerBaseUrl => IsDevelopment
        ? "https://10.0.2.2:5001"
        : "https://auth.bellwoodglobal.com";

    /// <summary>
    /// Base URL for AdminAPI (driver endpoints)
    /// Development: AdminAPI runs on https://localhost:5206
    /// Android emulator must use 10.0.2.2 to access host machine's localhost
    /// </summary>
    public static string AdminApiBaseUrl => IsDevelopment
        ? "https://10.0.2.2:5206"
        : "https://adminapi.bellwoodglobal.com";

    /// <summary>
    /// Base URL for RidesAPI (future integration)
    /// </summary>
    public static string RidesApiBaseUrl => IsDevelopment
        ? "https://10.0.2.2:5005"
        : "https://ridesapi.bellwoodglobal.com";

    /// <summary>
    /// Login endpoint on AuthServer
    /// </summary>
    public static string LoginEndpoint => $"{AuthServerBaseUrl}/api/auth/login";
}

/// <summary>
/// Configuration for location tracking behavior
/// </summary>
public static class LocationConfig
{
    /// <summary>
    /// Interval between location updates (in seconds)
    /// Default: 30 seconds to balance accuracy and battery consumption
    /// Server enforces minimum 15 seconds between updates
    /// </summary>
    public static int UpdateIntervalSeconds => 30;

    /// <summary>
    /// Desired accuracy for GPS readings (in meters)
    /// </summary>
    public static double DesiredAccuracyMeters => 10.0;

    /// <summary>
    /// Location permission rationale message
    /// </summary>
    public static string PermissionRationale =>
        "Bellwood needs your location to share your position with passengers while a ride is active.";
}
