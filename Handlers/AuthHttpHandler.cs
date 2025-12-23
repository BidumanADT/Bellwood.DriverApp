using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace Bellwood.DriverApp.Handlers;

/// <summary>
/// HTTP message handler that automatically injects JWT Bearer token into requests
/// and handles 401 Unauthorized responses by triggering re-authentication.
/// Enhanced with detailed logging for debugging authorization issues.
/// </summary>
public class AuthHttpHandler : DelegatingHandler
{
    private readonly IServiceProvider _serviceProvider;

    public AuthHttpHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Resolve IAuthService from service provider (avoid circular dependency)
            var authService = _serviceProvider.GetRequiredService<Services.IAuthService>();

            // Get token and inject into Authorization header
            var token = await authService.GetAccessTokenAsync();
            
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
#if DEBUG
                // Log token presence (first/last 4 chars only for security)
                var tokenPreview = token.Length > 8 
                    ? $"{token.Substring(0, 4)}...{token.Substring(token.Length - 4)}" 
                    : "****";
                Console.WriteLine($"?? [AuthHttpHandler] Token added: {tokenPreview}");
#endif
            }
            else
            {
#if DEBUG
                Console.WriteLine($"?? [AuthHttpHandler] WARNING: No token available for {request.RequestUri?.PathAndQuery}");
#endif
            }

            // Send request
            var response = await base.SendAsync(request, cancellationToken);

            // Handle 401 Unauthorized - token expired or invalid
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine($"?? [AuthHttpHandler] 401 Unauthorized received for {request.RequestUri?.PathAndQuery}");
                Console.WriteLine($"    Token was: {(string.IsNullOrWhiteSpace(token) ? "MISSING" : "PRESENT")}");
                
                // Clear token and signal that re-authentication is needed
                await authService.SignOutAsync();
                Console.WriteLine($"    ? Signed out user - token cleared from SecureStorage");
                
                // Notify the UI that re-authentication is needed
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Shell.Current.DisplayAlert(
                        "Session Expired",
                        "Your session has expired. Please log in again.",
                        "OK");
                    
                    // Navigate to login page
                    await Shell.Current.GoToAsync("//LoginPage");
                });
            }

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? [AuthHttpHandler] Exception: {ex.Message}");
            Console.WriteLine($"    Request: {request.RequestUri}");
            Console.WriteLine($"    Stack: {ex.StackTrace}");
            throw;
        }
    }
}
