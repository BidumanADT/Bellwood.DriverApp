using System.Text.Json.Serialization;

namespace Bellwood.DriverApp.Models;

/// <summary>
/// Enum representing the driver-facing ride status (matches AdminAPI RideStatus enum)
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RideStatus
{
    Scheduled,
    OnRoute,
    Arrived,
    PassengerOnboard,
    Completed,
    Cancelled
}

/// <summary>
/// Minimal ride information for list display
/// Matches AdminAPI DriverRideListItemDto
/// </summary>
public sealed class DriverRideListItemDto
{
    public required string Id { get; set; }
    
    /// <summary>
    /// Legacy pickup time (deprecated - use PickupDateTimeOffset instead)
    /// Kept for backward compatibility during transition period
    /// </summary>
    [Obsolete("Use PickupDateTimeOffset instead for correct timezone handling")]
    public DateTime PickupDateTime { get; set; }
    
    /// <summary>
    /// Pickup time with explicit timezone offset
    /// This is the correct property to use for display
    /// </summary>
    public DateTimeOffset? PickupDateTimeOffset { get; set; }
    
    /// <summary>
    /// Helper property that returns the correct pickup time
    /// Prefers PickupDateTimeOffset if available, falls back to PickupDateTime
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset DisplayPickupTime => 
        PickupDateTimeOffset ?? new DateTimeOffset(PickupDateTime, TimeZoneInfo.Local.GetUtcOffset(PickupDateTime));
    
    public required string PickupLocation { get; set; }
    public required string DropoffLocation { get; set; }
    public required string PassengerName { get; set; }
    public string? PassengerPhone { get; set; }
    public RideStatus Status { get; set; }
}

/// <summary>
/// Full ride details for single ride view
/// Matches AdminAPI DriverRideDetailDto
/// </summary>
public sealed class DriverRideDetailDto
{
    public required string Id { get; set; }
    
    /// <summary>
    /// Legacy pickup time (deprecated - use PickupDateTimeOffset instead)
    /// Kept for backward compatibility during transition period
    /// </summary>
    [Obsolete("Use PickupDateTimeOffset instead for correct timezone handling")]
    public DateTime PickupDateTime { get; set; }
    
    /// <summary>
    /// Pickup time with explicit timezone offset
    /// This is the correct property to use for display
    /// </summary>
    public DateTimeOffset? PickupDateTimeOffset { get; set; }
    
    /// <summary>
    /// Helper property that returns the correct pickup time
    /// Prefers PickupDateTimeOffset if available, falls back to PickupDateTime
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset DisplayPickupTime => 
        PickupDateTimeOffset ?? new DateTimeOffset(PickupDateTime, TimeZoneInfo.Local.GetUtcOffset(PickupDateTime));
    
    public required string PickupLocation { get; set; }
    public string? PickupStyle { get; set; }
    public string? PickupSignText { get; set; }
    public required string DropoffLocation { get; set; }
    public required string PassengerName { get; set; }
    public string? PassengerPhone { get; set; }
    public int PassengerCount { get; set; }
    public int CheckedBags { get; set; }
    public int CarryOnBags { get; set; }
    public string? VehicleClass { get; set; }
    public FlightInfo? OutboundFlight { get; set; }
    public string? AdditionalRequest { get; set; }
    public RideStatus Status { get; set; }
}

/// <summary>
/// Flight information for airport pickups
/// </summary>
public sealed class FlightInfo
{
    public string? FlightNumber { get; set; }
    public string? TailNumber { get; set; }
}

/// <summary>
/// Request to update ride status
/// Matches AdminAPI RideStatusUpdateRequest
/// </summary>
public sealed class RideStatusUpdateRequest
{
    public RideStatus NewStatus { get; set; }
}

/// <summary>
/// Response from status update endpoint
/// </summary>
public sealed class RideStatusUpdateResponse
{
    public required string Message { get; set; }
    public required string RideId { get; set; }
    public RideStatus NewStatus { get; set; }
}

/// <summary>
/// Location update sent to server
/// Matches AdminAPI LocationUpdate
/// </summary>
public sealed class LocationUpdate
{
    public required string RideId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Direction of travel in degrees (0-360, where 0 is North)
    /// Optional - may be null if device doesn't provide heading
    /// </summary>
    public double? Heading { get; set; }
    
    /// <summary>
    /// Current speed in meters per second
    /// Optional - may be null if device doesn't provide speed
    /// </summary>
    public double? Speed { get; set; }
    
    /// <summary>
    /// Accuracy of the location reading in meters
    /// </summary>
    public double? Accuracy { get; set; }
}

/// <summary>
/// Response from location update endpoint
/// </summary>
public sealed class LocationUpdateResponse
{
    public required string Message { get; set; }
}

/// <summary>
/// Login request for AuthServer
/// </summary>
public sealed class LoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

/// <summary>
/// Login response from AuthServer containing JWT
/// </summary>
public sealed class LoginResponse
{
    public required string AccessToken { get; set; }
    public string? RefreshToken { get; set; } // Phase 3
    public int ExpiresIn { get; set; }
}
