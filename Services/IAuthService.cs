using Bellwood.DriverApp.Models;

namespace Bellwood.DriverApp.Services;

/// <summary>
/// Service for authentication operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a driver and stores the JWT token
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> LoginAsync(string username, string password);

    /// <summary>
    /// Signs out the current driver and clears stored tokens
    /// </summary>
    Task SignOutAsync();

    /// <summary>
    /// Gets the current JWT access token
    /// </summary>
    Task<string?> GetAccessTokenAsync();

    /// <summary>
    /// Checks if a valid token exists
    /// </summary>
    Task<bool> IsAuthenticatedAsync();
}
