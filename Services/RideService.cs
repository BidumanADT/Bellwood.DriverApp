using System.Net.Http.Json;
using Bellwood.DriverApp.Models;

namespace Bellwood.DriverApp.Services;

/// <summary>
/// Implementation of ride service using AdminAPI driver endpoints.
/// Uses the named "driver-admin" HttpClient configured in MauiProgram.
/// </summary>
public class RideService : IRideService
{
    private readonly HttpClient _httpClient;

    public RideService(IHttpClientFactory httpClientFactory)
    {
        // Use the "driver-admin" client – base address and dev-cert override are set in MauiProgram
        _httpClient = httpClientFactory.CreateClient("driver-admin");
    }

    public async Task<List<DriverRideListItemDto>> GetTodaysRidesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/driver/rides/today");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to fetch rides: {response.StatusCode}");
                return new List<DriverRideListItemDto>();
            }

            var rides = await response.Content.ReadFromJsonAsync<List<DriverRideListItemDto>>();
            return rides ?? new List<DriverRideListItemDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching rides: {ex.Message}");
            return new List<DriverRideListItemDto>();
        }
    }

    public async Task<DriverRideDetailDto?> GetRideDetailsAsync(string rideId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/driver/rides/{rideId}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to fetch ride details: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<DriverRideDetailDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching ride details: {ex.Message}");
            return null;
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateRideStatusAsync(string rideId, RideStatus newStatus)
    {
        try
        {
            var request = new RideStatusUpdateRequest { NewStatus = newStatus };
            var response = await _httpClient.PostAsJsonAsync($"/driver/rides/{rideId}/status", request);

            if (!response.IsSuccessStatusCode)
            {
                return response.StatusCode switch
                {
                    System.Net.HttpStatusCode.BadRequest => (false, "Invalid status transition"),
                    System.Net.HttpStatusCode.Forbidden => (false, "You don't have permission to update this ride"),
                    System.Net.HttpStatusCode.NotFound => (false, "Ride not found"),
                    _ => (false, $"Update failed: {response.StatusCode}")
                };
            }

            return (true, null);
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Unexpected error: {ex.Message}");
        }
    }
}
