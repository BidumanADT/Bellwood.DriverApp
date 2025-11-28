using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace Bellwood.DriverApp.Handlers;

/// <summary>
/// HTTP message handler that automatically injects JWT Bearer token into requests
/// and handles 401 Unauthorized responses by triggering re-authentication
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
        // Resolve IAuthService from service provider (avoid circular dependency)
        var authService = _serviceProvider.GetRequiredService<Services.IAuthService>();

        // Get token and inject into Authorization header
        var token = await authService.GetAccessTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Send request
        var response = await base.SendAsync(request, cancellationToken);

        // Handle 401 Unauthorized - token expired or invalid
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // Clear token and signal that re-authentication is needed
            await authService.SignOutAsync();
            
            // Future Phase 3: Attempt refresh token flow here
            // For now, the app will detect IsAuthenticated=false and redirect to login
        }

        return response;
    }
}
