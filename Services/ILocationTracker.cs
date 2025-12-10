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
    /// <param name="rideId">The ride identifier</param>
    /// <param name="destinationLatitude">Optional destination latitude for proximity-based interval adjustment</param>
    /// <param name="destinationLongitude">Optional destination longitude for proximity-based interval adjustment</param>
    Task<bool> StartTrackingAsync(string rideId, double? destinationLatitude = null, double? destinationLongitude = null);

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
    /// Gets the current tracking status for a ride
    /// </summary>
    TrackingStatus GetTrackingStatus(string rideId);

    /// <summary>
    /// Updates the destination coordinates for proximity-based tracking
    /// Call this when transitioning from OnRoute to PassengerOnboard to update target
    /// </summary>
    void UpdateDestination(string rideId, double latitude, double longitude);

    /// <summary>
    /// Event fired when a location update fails
    /// </summary>
    event EventHandler<LocationUpdateFailedEventArgs>? LocationUpdateFailed;

    /// <summary>
    /// Event fired when tracking status changes (started, stopped, error)
    /// </summary>
    event EventHandler<TrackingStatusChangedEventArgs>? TrackingStatusChanged;

    /// <summary>
    /// Event fired when location is successfully sent to server
    /// </summary>
    event EventHandler<LocationSentEventArgs>? LocationSent;
}

/// <summary>
/// Represents the current tracking status
/// </summary>
public enum TrackingStatus
{
    /// <summary>Not tracking</summary>
    Inactive,
    /// <summary>Tracking is active and working normally</summary>
    Active,
    /// <summary>Tracking is active but experiencing errors</summary>
    Error,
    /// <summary>Tracking requires permission</summary>
    PermissionRequired
}

/// <summary>
/// Event args for location update failures
/// </summary>
public class LocationUpdateFailedEventArgs : EventArgs
{
    public required string RideId { get; init; }
    public required string ErrorMessage { get; init; }
    public bool WillRetry { get; init; }
    public int RetryCount { get; init; }
}

/// <summary>
/// Event args for tracking status changes
/// </summary>
public class TrackingStatusChangedEventArgs : EventArgs
{
    public required string RideId { get; init; }
    public TrackingStatus OldStatus { get; init; }
    public TrackingStatus NewStatus { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// Event args for successful location sends
/// </summary>
public class LocationSentEventArgs : EventArgs
{
    public required string RideId { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public DateTime Timestamp { get; init; }
    public int CurrentIntervalSeconds { get; init; }
}
