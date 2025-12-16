# Timezone Support - Quick Reference

## What Changed?

The Driver App now automatically includes the device's timezone in all API requests.

## Implementation

### New File: `Handlers/TimezoneHttpHandler.cs`
```csharp
public class TimezoneHttpHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(...)
    {
        request.Headers.Add("X-Timezone-Id", TimeZoneInfo.Local.Id);
        return await base.SendAsync(request, cancellationToken);
    }
}
```

### Modified: `MauiProgram.cs`
```csharp
builder.Services.AddTransient<TimezoneHttpHandler>();

builder.Services.AddHttpClient("driver-admin", c => { ... })
    .AddHttpMessageHandler<TimezoneHttpHandler>()  // ? Added
    .AddHttpMessageHandler<AuthHttpHandler>();
```

## What to Test

1. **Launch app** - Look for timezone detection logs:
   ```
   ?? Device Timezone ID: America/New_York
   ? Current UTC Offset: -5.0 hours
   ```

2. **Make API calls** - In DEBUG mode, verify header is sent:
   ```
   ?? Request: GET /driver/rides/today
   ?? Timezone Header: America/New_York
   ```

3. **Check backend logs** - Server should show:
   ```
   ?? Driver driver-001 timezone: America/New_York
   ```

## Expected Results

? Rides appear in driver's local timezone  
? Works on Android, iOS, Windows  
? Automatic DST handling  
? No manual configuration needed  

## Platform-Specific Timezone IDs

| Platform | Example Timezone ID |
|----------|---------------------|
| Android | `America/New_York` (IANA) |
| iOS | `America/New_York` (IANA) |
| Windows | `Eastern Standard Time` (converted by backend) |

## Common Timezone IDs

| Location | Timezone ID |
|----------|-------------|
| New York (EST/EDT) | `America/New_York` |
| Chicago (CST/CDT) | `America/Chicago` |
| Los Angeles (PST/PDT) | `America/Los_Angeles` |
| London (GMT/BST) | `Europe/London` |
| Tokyo (JST) | `Asia/Tokyo` |

## Troubleshooting

**Rides not showing at expected time?**
1. Check app logs for timezone detection
2. Verify device timezone settings
3. Check backend logs for timezone detection warning

**Header not being sent?**
1. Rebuild the app
2. Check handler is registered in `MauiProgram.cs`
3. Verify logs show timezone header in DEBUG mode

## Build Status

? Build successful  
? All tests passing  
? Ready for deployment  
