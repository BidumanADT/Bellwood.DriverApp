namespace Bellwood.DriverApp.Handlers;

/// <summary>
/// HTTP message handler that automatically injects the device's timezone
/// into all API requests via the X-Timezone-Id header.
/// This enables the backend to return rides in the driver's local timezone.
/// </summary>
public class TimezoneHttpHandler : DelegatingHandler
{
    private readonly string _timezoneId;

    public TimezoneHttpHandler()
    {
        // Get the device's current timezone
        _timezoneId = TimeZoneInfo.Local.Id;
        
        // Log timezone detection for debugging
        var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
        Console.WriteLine($"?? Device Timezone ID: {_timezoneId}");
        Console.WriteLine($"? Current UTC Offset: {offset.TotalHours:+0.0;-0.0} hours");
        Console.WriteLine($"?? Current Local Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
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
        // Log in debug mode to verify header is being sent
        Console.WriteLine($"?? Request: {request.Method} {request.RequestUri?.PathAndQuery}");
        Console.WriteLine($"?? Timezone Header: {_timezoneId}");
#endif

        return await base.SendAsync(request, cancellationToken);
    }
}
