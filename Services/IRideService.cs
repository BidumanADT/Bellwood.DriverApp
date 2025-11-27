using Bellwood.DriverApp.Models;

namespace Bellwood.DriverApp.Services;

/// <summary>
/// Service for ride-related operations
/// </summary>
public interface IRideService
{
    /// <summary>
    /// Gets today's assigned rides for the authenticated driver
    /// </summary>
    Task<List<DriverRideListItemDto>> GetTodaysRidesAsync();

    /// <summary>
    /// Gets detailed information about a specific ride
    /// </summary>
    Task<DriverRideDetailDto?> GetRideDetailsAsync(string rideId);

    /// <summary>
    /// Updates the status of a ride
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> UpdateRideStatusAsync(string rideId, RideStatus newStatus);
}
