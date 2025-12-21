using System.Net.Http.Json;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using Bellwood.DriverApp.Models;

namespace Bellwood.DriverApp.Services;

/// <summary>
/// Implementation of authentication service using SecureStorage for token management.
/// Uses the named "auth" HttpClient configured in MauiProgram.
/// Enhanced with token validation and expiration checking.
/// </summary>
public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private const string AccessTokenKey = "bellwood_access_token";
    private const string TokenExpiryKey = "bellwood_token_expiry";

    public AuthService(IHttpClientFactory httpClientFactory)
    {
        // Use the "auth" client – base address and dev-cert override are set in MauiProgram
        _httpClient = httpClientFactory.CreateClient("auth");
    }

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(string username, string password)
    {
        try
        {
            var loginRequest = new LoginRequest { Username = username, Password = password };
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginRequest);

            if (!response.IsSuccessStatusCode)
            {
                return response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized => (false, "Invalid username or password"),
                    System.Net.HttpStatusCode.Forbidden => (false, "Access denied. Driver role required."),
                    _ => (false, $"Login failed: {response.StatusCode}")
                };
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (loginResponse?.AccessToken == null)
            {
                return (false, "Invalid response from server");
            }

            // Store token securely
            await SecureStorage.SetAsync(AccessTokenKey, loginResponse.AccessToken);
            
            // Extract and store token expiration (as Unix timestamp for accuracy)
            var expiry = GetTokenExpiration(loginResponse.AccessToken);
            if (expiry.HasValue)
            {
                // Store as Unix timestamp (seconds since epoch) to avoid DateTime parsing issues
                var unixTimestamp = new DateTimeOffset(expiry.Value).ToUnixTimeSeconds();
                await SecureStorage.SetAsync(TokenExpiryKey, unixTimestamp.ToString());
                
                Console.WriteLine($"?? Token stored, expires: {expiry.Value:yyyy-MM-dd HH:mm:ss} UTC");
                Console.WriteLine($"    Unix timestamp: {unixTimestamp}");
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

    public async Task SignOutAsync()
    {
        SecureStorage.Remove(AccessTokenKey);
        SecureStorage.Remove(TokenExpiryKey);
        Console.WriteLine($"?? User signed out - tokens cleared");
        await Task.CompletedTask;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            var token = await SecureStorage.GetAsync(AccessTokenKey);
            
            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine($"?? [AuthService] No token in SecureStorage");
                return null;
            }
            
            // Check if token is expired
            if (await IsTokenExpiredAsync())
            {
                Console.WriteLine($"? [AuthService] Token expired - clearing from storage");
                await SignOutAsync();
                return null;
            }
            
            return token;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? [AuthService] Error retrieving token: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetAccessTokenAsync();
        var isAuthenticated = !string.IsNullOrWhiteSpace(token);
        
#if DEBUG
        Console.WriteLine($"?? [AuthService] IsAuthenticated: {isAuthenticated}");
#endif
        
        return isAuthenticated;
    }

    /// <summary>
    /// Checks if the current token is expired
    /// </summary>
    private async Task<bool> IsTokenExpiredAsync()
    {
        try
        {
            var expiryStr = await SecureStorage.GetAsync(TokenExpiryKey);
            if (string.IsNullOrWhiteSpace(expiryStr))
            {
                Console.WriteLine($"?? [AuthService] No expiry timestamp in storage");
                // No expiry stored, assume token is valid
                return false;
            }

            if (long.TryParse(expiryStr, out var unixTimestamp))
            {
                // Convert Unix timestamp back to DateTime UTC
                var expiry = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime;
                var now = DateTime.UtcNow;
                
                // Add 1 minute buffer to prevent edge-case failures
                var isExpired = now >= expiry.AddMinutes(-1);
                
#if DEBUG
                Console.WriteLine($"?? [AuthService] Expiry check:");
                Console.WriteLine($"    Token expires: {expiry:yyyy-MM-dd HH:mm:ss} UTC");
                Console.WriteLine($"    Current time:  {now:yyyy-MM-dd HH:mm:ss} UTC");
                Console.WriteLine($"    Time until expiry: {(expiry - now).TotalMinutes:F1} minutes");
                Console.WriteLine($"    Is expired: {isExpired}");
#endif
                
                if (isExpired)
                {
                    Console.WriteLine($"? Token expired at {expiry:yyyy-MM-dd HH:mm:ss} UTC");
                }
                
                return isExpired;
            }

            Console.WriteLine($"?? [AuthService] Failed to parse expiry timestamp: {expiryStr}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? [AuthService] Error checking expiration: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Extracts the expiration time from a JWT token
    /// </summary>
    private DateTime? GetTokenExpiration(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            // JWT ValidTo is always in UTC
            var exp = jwtToken.ValidTo;
            
            Console.WriteLine($"?? [AuthService] Parsed JWT token:");
            Console.WriteLine($"    Issued at: {jwtToken.ValidFrom:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine($"    Expires:   {exp:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine($"    Lifetime:  {(exp - jwtToken.ValidFrom).TotalHours:F1} hours");
            
            return exp;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"?? Failed to parse token expiration: {ex.Message}");
            return null;
        }
    }
}
