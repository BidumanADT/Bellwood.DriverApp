namespace Bellwood.DriverApp.Handlers;

/// <summary>
/// HTTP message handler that automatically injects the device's timezone
/// into all API requests via the X-Timezone-Id header.
/// This enables the backend to return rides in the driver's local timezone.
/// IMPORTANT: This handler runs BEFORE AuthHttpHandler in the pipeline.
/// </summary>
public class TimezoneHttpHandler : DelegatingHandler
{
    private readonly string _timezoneId;
    private readonly TimeSpan _utcOffset;

    public TimezoneHttpHandler()
    {
        // Get the device's current timezone
        _timezoneId = TimeZoneInfo.Local.Id;
        _utcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
        
        // Log timezone detection for debugging
        Console.WriteLine("???????????????????????????????????????????????????");
        Console.WriteLine("?? TIMEZONE DETECTION");
        Console.WriteLine($"   Device Timezone ID: {_timezoneId}");
        Console.WriteLine($"   Current UTC Offset: {_utcOffset.TotalHours:+0.0;-0.0} hours");
        Console.WriteLine($"   Current Local Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"   Current UTC Time:   {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine("???????????????????????????????????????????????????");
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Add timezone header to all requests
        // Backend will use this to return rides in the driver's local timezone
        if (!request.Headers.Contains("X-Timezone-Id"))
        {
            request.Headers.Add("X-Timezone-Id", _timezoneId);
        }

#if DEBUG
        // Log request details BEFORE passing to next handler (AuthHttpHandler)
        Console.WriteLine($"?? [TimezoneHttpHandler] Request: {request.Method} {request.RequestUri?.PathAndQuery}");
        Console.WriteLine($"   ?? X-Timezone-Id: {_timezoneId}");
#endif

        // Pass to next handler (AuthHttpHandler will add Authorization header)
        var response = await base.SendAsync(request, cancellationToken);

#if DEBUG
        // Log response status and check if auth header was added by subsequent handlers
        var statusEmoji = response.IsSuccessStatusCode ? "?" : "?";
        var hasAuthHeader = request.Headers.Authorization != null;
        
        Console.WriteLine($"{statusEmoji} Response: {(int)response.StatusCode} {response.StatusCode}");
        Console.WriteLine($"   ?? Authorization: {(hasAuthHeader ? "Present ?" : "Missing ??")}");
        
        if (!hasAuthHeader && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            Console.WriteLine($"   ?? WARNING: 401 response with missing Authorization header!");
            Console.WriteLine($"   This suggests AuthHttpHandler did not add the token.");
        }
        
        Console.WriteLine("?????????????????????????????????????????????????");
#endif

        return response;
    }
}
