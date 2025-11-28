using System.Net.Http.Json;
using System.Text.Json;
using Bellwood.DriverApp.Models;

namespace Bellwood.DriverApp.Services;

/// <summary>
/// Implementation of authentication service using SecureStorage for token management.
/// Uses the named "auth" HttpClient configured in MauiProgram.
/// </summary>
public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private const string AccessTokenKey = "bellwood_access_token";

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
        await Task.CompletedTask;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            return await SecureStorage.GetAsync(AccessTokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetAccessTokenAsync();
        return !string.IsNullOrWhiteSpace(token);
    }
}
