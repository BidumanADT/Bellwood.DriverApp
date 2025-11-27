using Bellwood.DriverApp.Models;

namespace Bellwood.DriverApp.Services;

/// <summary>
/// Service for managing real-time GPS location tracking during active rides
/// </summary>
public interface ILocationTracker
{
    /// <summary>
    /// Starts tracking location for a specific ride
    /// </summary>
    Task<bool> StartTrackingAsync(string rideId);

    /// <summary>
    /// Stops tracking location for a specific ride
    /// </summary>
    Task StopTrackingAsync(string rideId);

    /// <summary>
    /// Stops all active location tracking
    /// </summary>
    Task StopAllTrackingAsync();

    /// <summary>
    /// Checks if tracking is active for a ride
    /// </summary>
    bool IsTracking(string rideId);

    /// <summary>
    /// Event fired when a location update fails
    /// </summary>
    event EventHandler<string>? LocationUpdateFailed;
}
