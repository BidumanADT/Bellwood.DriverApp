# Timezone Header Implementation - Driver App

## Overview

The Driver App now automatically includes the device's timezone in all API requests to support worldwide operations. This ensures rides are displayed in the driver's local timezone regardless of their location.

## Implementation Details

### 1. New Handler: `TimezoneHttpHandler.cs`

Created a new HTTP message handler that automatically adds the `X-Timezone-Id` header to all outgoing requests.

**Key Features:**
- Detects device timezone using `TimeZoneInfo.Local.Id`
- Logs timezone information on startup for debugging
- Automatically adds header to all requests
- Works across Android, iOS, and Windows platforms

**Code:**
```csharp
public class TimezoneHttpHandler : DelegatingHandler
{
    private readonly string _timezoneId;

    public TimezoneHttpHandler()
    {
        _timezoneId = TimeZoneInfo.Local.Id;
        
        // Logs timezone detection for debugging
        Console.WriteLine($"?? Device Timezone ID: {_timezoneId}");
        Console.WriteLine($"? Current UTC Offset: {offset.TotalHours:+0.0;-0.0} hours");
        Console.WriteLine($"?? Current Local Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Add timezone header to all requests
        if (!request.Headers.Contains("X-Timezone-Id"))
        {
            request.Headers.Add("X-Timezone-Id", _timezoneId);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
```

### 2. Updated `MauiProgram.cs`

Registered the `TimezoneHttpHandler` and added it to both HTTP clients:

**Changes:**
1. Registered `TimezoneHttpHandler` as a transient service
2. Added to "auth" HTTP client (for login endpoint)
3. Added to "driver-admin" HTTP client (for all driver API calls)
4. Positioned **before** `AuthHttpHandler` in the pipeline

**Handler Chain Order:**
```
Request ? TimezoneHttpHandler ? AuthHttpHandler ? Server
         (adds X-Timezone-Id)  (adds Bearer token)
```

**Code:**
```csharp
// Register handlers
builder.Services.AddTransient<AuthHttpHandler>();
builder.Services.AddTransient<TimezoneHttpHandler>();

// Auth Server client
builder.Services.AddHttpClient("auth", c => { /* config */ })
    .AddHttpMessageHandler<TimezoneHttpHandler>()
    // ...

// AdminAPI client
builder.Services.AddHttpClient("driver-admin", c => { /* config */ })
    .AddHttpMessageHandler<TimezoneHttpHandler>()  // First: timezone
    .AddHttpMessageHandler<AuthHttpHandler>()      // Then: auth
    // ...
```

## Platform-Specific Behavior

### Android
- Returns IANA timezone IDs (e.g., `America/New_York`)
- Works directly with backend ?

### iOS
- Returns IANA timezone IDs (e.g., `America/New_York`)
- Works directly with backend ?

### Windows
- Returns Windows timezone IDs (e.g., `Eastern Standard Time`)
- Backend automatically converts to IANA format ?

## Testing

### What to Look For

When you launch the app, you should see timezone detection logs in the output:

```
?? Device Timezone ID: America/New_York
? Current UTC Offset: -5.0 hours
?? Current Local Time: 2025-01-15 15:30:45
```

### Debug Mode

In DEBUG builds, every API request logs the timezone header:

```
?? Request: GET /driver/rides/today
?? Timezone Header: America/New_York
```

### Test Scenarios

#### Scenario 1: Driver in Different Timezones

Test the app with device timezone set to different regions:

| Device Timezone | Expected Header | Expected Behavior |
|-----------------|-----------------|-------------------|
| America/New_York | `America/New_York` | Rides in EST/EDT |
| America/Chicago | `America/Chicago` | Rides in CST/CDT |
| America/Los_Angeles | `America/Los_Angeles` | Rides in PST/PDT |
| Europe/London | `Europe/London` | Rides in GMT/BST |
| Asia/Tokyo | `Asia/Tokyo` | Rides in JST |

#### Scenario 2: Daylight Saving Time

The timezone header automatically reflects DST changes:
- Winter: `America/New_York` reports UTC-5
- Summer: `America/New_York` reports UTC-4

No code changes needed - handled automatically by `TimeZoneInfo.Local`.

#### Scenario 3: API Endpoints

All endpoints now receive the timezone header:

```http
# Login
POST https://localhost:5001/api/auth/login
X-Timezone-Id: America/New_York

# Get Today's Rides
GET https://localhost:5206/driver/rides/today
Authorization: Bearer <token>
X-Timezone-Id: America/New_York

# Update Ride Status
PUT https://localhost:5206/driver/rides/{id}/status
Authorization: Bearer <token>
X-Timezone-Id: America/New_York

# Location Update
POST https://localhost:5206/driver/location/update
Authorization: Bearer <token>
X-Timezone-Id: America/New_York
```

## Backend Integration

### Server-Side Timezone Detection

The backend logs timezone detection:

```
?? Driver driver-001 timezone: America/New_York, current time: 2025-12-14 20:04
```

If the header is **missing**, backend logs:

```
?? Warning: Could not load Central timezone, using server local time
```

### Backward Compatibility

If the `X-Timezone-Id` header is not sent, the backend defaults to **Central Time (America/Chicago)** for backward compatibility. However, this will cause incorrect ride filtering for drivers outside the Central timezone.

**Therefore, this update is required for correct worldwide operation.**

## Benefits

? **Automatic timezone detection** - No manual configuration required  
? **Worldwide support** - Works for drivers in any timezone  
? **DST handling** - Automatically adjusts for daylight saving time  
? **Cross-platform** - Works on Android, iOS, and Windows  
? **Transparent** - No changes needed to existing service code  
? **Debuggable** - Comprehensive logging in debug builds  

## Troubleshooting

### Issue: Rides not appearing at expected times

**Check:**
1. Review app startup logs for timezone detection:
   ```
   ?? Device Timezone ID: America/New_York
   ```

2. Verify the device's timezone settings are correct:
   - Android: Settings ? System ? Date & time ? Time zone
   - iOS: Settings ? General ? Date & Time ? Time Zone

3. Check backend logs for timezone detection:
   ```
   ?? Driver driver-001 timezone: America/New_York
   ```

### Issue: Backend shows warning about timezone

If you see:
```
?? Warning: Could not load Central timezone, using server local time
```

**Solution:**
- Verify the app is sending the header (check debug logs)
- Rebuild and redeploy the app
- Clear app data and restart

### Issue: Wrong timezone being sent

**Check device timezone:**
```csharp
// In debug mode, check:
Console.WriteLine($"Device Timezone: {TimeZoneInfo.Local.Id}");
Console.WriteLine($"Device UTC Offset: {TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow)}");
Console.WriteLine($"Device Local Time: {DateTime.Now}");
```

If the device timezone is incorrect, the user needs to update their device settings.

## Migration Notes

### Before This Change
- Driver app did not send timezone information
- Backend defaulted to Central Time for all drivers
- Drivers outside Central timezone saw incorrect ride times

### After This Change
- Driver app automatically sends device timezone
- Backend returns rides in driver's local timezone
- Works correctly worldwide

### Deployment Steps

1. ? Deploy updated Driver App with `TimezoneHttpHandler`
2. ? Verify timezone detection logs
3. ? Test with drivers in different timezones
4. ? Monitor backend logs for timezone detection
5. Future: Remove Central Time fallback (after all drivers updated)

## Files Modified

1. **Created:** `Handlers/TimezoneHttpHandler.cs`
   - New HTTP message handler for timezone header

2. **Modified:** `MauiProgram.cs`
   - Registered `TimezoneHttpHandler`
   - Added to both HTTP clients ("auth" and "driver-admin")

## Performance Impact

**Minimal:**
- Timezone is detected once on handler creation (app startup)
- Header addition adds <1ms per request
- No additional network calls

## Security Considerations

**Timezone header is not sensitive data:**
- Does not reveal precise location (only timezone)
- IANA timezone IDs are public information
- No privacy concerns

## Future Enhancements

### Possible Future Features:

1. **Manual Timezone Override**
   ```csharp
   // Allow driver to manually set timezone (e.g., if traveling)
   public void SetTimezone(string timezoneId)
   {
       _httpClient.DefaultRequestHeaders.Remove("X-Timezone-Id");
       _httpClient.DefaultRequestHeaders.Add("X-Timezone-Id", timezoneId);
   }
   ```

2. **Timezone Change Detection**
   ```csharp
   // Detect when device timezone changes and update header
   public void OnTimezoneChanged()
   {
       var newTimezone = TimeZoneInfo.Local.Id;
       // Update header...
   }
   ```

3. **Settings Page**
   - Display current timezone
   - Allow manual override
   - Show UTC offset

## Summary

The Driver App now automatically sends the device's timezone with every API request, enabling worldwide operations with correct local time display. The implementation is transparent, cross-platform, and requires no additional configuration.

**Key Header:**
```
X-Timezone-Id: America/New_York
```

**Result:**
- ? Rides appear in driver's local timezone
- ? Works worldwide
- ? Automatic DST handling
- ? No manual configuration needed
