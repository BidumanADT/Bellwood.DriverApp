# ?? Quick Reference Card

## Essential Commands

### Build & Run
```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run on Android
dotnet build -t:Run -f net8.0-android

# Clean
dotnet clean
```

### Testing with Backend
```bash
# Start AuthServer (from AuthServer directory)
dotnet run --urls="https://localhost:5001;http://localhost:5000"

# Start AdminAPI (from AdminAPI directory)
dotnet run --urls="https://localhost:5206;http://localhost:5205"

# Seed test data
curl -X POST https://localhost:5206/bookings/seed \
  -H "Authorization: Bearer {admin-jwt}"
```

---

## Test Credentials

**Driver Account:**
- Username: `[your-driver-username]`
- Password: `[your-driver-password]`
- Expected UID: `driver-001`
- Required Role: `driver`

---

## API Endpoints Used

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/auth/login` | POST | Login & get JWT |
| `/driver/rides/today` | GET | List rides (next 24h) |
| `/driver/rides/{id}` | GET | Ride details |
| `/driver/rides/{id}/status` | POST | Update status |
| `/driver/location/update` | POST | Send GPS coords |

---

## Configuration

### Development URLs (Android Emulator)
```csharp
AuthServer: https://10.0.2.2:5001
AdminAPI:   https://10.0.2.2:5206
```

### Location Tracking
```csharp
Update Interval: 30 seconds
Server Min:      15 seconds
Accuracy:        10 meters
```

---

## Key Classes

### Services
- `IAuthService` / `AuthService` - Login, token storage
- `IRideService` / `RideService` - Ride operations
- `ILocationTracker` / `LocationTracker` - GPS tracking

### ViewModels
- `LoginViewModel` - Authentication
- `HomeViewModel` - Ride list
- `RideDetailViewModel` - Ride details & status updates

### Views
- `LoginPage` - Login UI
- `HomePage` - Ride list UI
- `RideDetailPage` - Ride details UI

---

## Status Workflow

```
Scheduled ???????????
    ?               ?
    ?               ?
OnRoute ?????????????
    ?               ?
    ?               ?
Arrived ???????????????? Cancelled
    ?               ?
    ?               ?
PassengerOnboard ????
    ?               ?
    ?               ?
Completed ???????????
```

---

## Common Tasks

### Add New Page
1. Create `Views/MyPage.xaml` and `.xaml.cs`
2. Create `ViewModels/MyViewModel.cs`
3. Register in `MauiProgram.cs`:
   ```csharp
   builder.Services.AddTransient<MyViewModel>();
   builder.Services.AddTransient<MyPage>();
   ```
4. Add route in `AppShell.xaml.cs`:
   ```csharp
   Routing.RegisterRoute("MyPage", typeof(MyPage));
   ```

### Add API Call
1. Add DTO to `Models/ApiModels.cs`
2. Add method to service interface:
   ```csharp
   Task<MyDto> GetDataAsync();
   ```
3. Implement in service:
   ```csharp
   public async Task<MyDto> GetDataAsync()
   {
       var response = await _httpClient.GetAsync("/my/endpoint");
       return await response.Content.ReadFromJsonAsync<MyDto>();
   }
   ```

### Update Location Interval
In `Helpers/AppSettings.cs`:
```csharp
public static int UpdateIntervalSeconds => 30; // Change this
```

---

## Troubleshooting Quick Fixes

### Can't connect to server
```bash
# Check if services are running
curl https://localhost:5001
curl https://localhost:5206

# Test from emulator browser
http://10.0.2.2:5206/swagger
```

### Location not working
1. Check permissions in Android Settings
2. Enable GPS in emulator Extended Controls ? Location
3. Verify ride status is OnRoute/Arrived/PassengerOnboard

### Invalid credentials
- Verify driver account exists
- Check JWT claims include `role=driver` and `uid=driver-001`
- Use jwt.io to inspect token

### Build errors
```bash
dotnet clean
dotnet restore
dotnet build
```

---

## Debug Tips

### View Console Logs
In Visual Studio: View ? Output ? Show output from: Debug

### Inspect Token
After login, add breakpoint in `AuthService.LoginAsync`:
```csharp
var token = loginResponse.AccessToken;
// Copy token and paste into jwt.io
```

### Monitor API Calls
Use Fiddler or Charles Proxy to intercept HTTP traffic

### Check Stored Token
In `App.xaml.cs OnStart`:
```csharp
var token = await SecureStorage.GetAsync("bellwood_access_token");
Console.WriteLine($"Token: {token}");
```

---

## Useful Links

- [MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [JWT.io](https://jwt.io) - Token decoder
- [Android Emulator Network](https://developer.android.com/studio/run/emulator-networking)

---

## Emergency Contacts

- **Mobile Team Lead**: [contact]
- **Backend API Team**: [contact]
- **DevOps**: [contact]
- **On-Call**: [contact]

---

## Version Info

- **App Version**: 1.0.0
- **.NET Version**: 8.0
- **MAUI Version**: 8.0
- **Min Android**: API 21 (Android 5.0)
- **Target Android**: API 34 (Android 14)

---

**Last Updated**: November 28, 2024
**Status**: Phase 1 Complete ?
