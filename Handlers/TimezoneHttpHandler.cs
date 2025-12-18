namespace Bellwood.DriverApp.Handlers;

/// <summary>
/// HTTP message handler that automatically injects the device's timezone
/// into all API requests via the X-Timezone-Id header.
/// This enables the backend to return rides in the driver's local timezone.
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
        // Enhanced logging in debug mode to verify header is being sent
        Console.WriteLine($"?? API Request: {request.Method} {request.RequestUri?.PathAndQuery}");
        Console.WriteLine($"   ?? X-Timezone-Id: {_timezoneId}");
        
        // Log auth header presence (not the actual token for security)
        var hasAuthHeader = request.Headers.Contains("Authorization");
        Console.WriteLine($"   ?? Authorization: {(hasAuthHeader ? "Present" : "Missing")}");
#endif

        var response = await base.SendAsync(request, cancellationToken);

#if DEBUG
        // Log response status for debugging
        var statusEmoji = response.IsSuccessStatusCode ? "?" : "?";
        Console.WriteLine($"{statusEmoji} Response: {(int)response.StatusCode} {response.StatusCode}");
        Console.WriteLine("?????????????????????????????????????????????????");
#endif

        return response;
    }
}
