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
    public DateTime PickupDateTime { get; set; }
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
    public DateTime PickupDateTime { get; set; }
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
