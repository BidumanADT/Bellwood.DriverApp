# Bellwood Driver App

![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-8.0-512BD4?style=flat-square&logo=.net)
![Platform](https://img.shields.io/badge/platform-Android%20%7C%20iOS%20%7C%20Windows-lightgrey?style=flat-square)
![License](https://img.shields.io/badge/license-Proprietary-red?style=flat-square)

A professional cross-platform mobile application for Bellwood chauffeur drivers to manage ride assignments, update statuses in real-time, and share GPS location with passengers during active trips.

## Overview

Bellwood Driver App is a .NET MAUI application that enables drivers to:
- Authenticate securely with JWT tokens and role-based authorization
- View today's assigned rides with timezone-aware pickup times
- Access detailed ride information including passenger details and flight info
- Update ride status through validated state transitions (FSM)
- Share real-time GPS location with passengers during active trips
- Navigate to pickup and dropoff locations using native map applications
- Track location updates with dynamic intervals based on proximity to destination
- Receive immediate feedback on session expiry and authentication issues

## Architecture

Built with .NET 8 MAUI using MVVM architecture:

- **Framework:** .NET 8.0
- **Target Platforms:**
  - Android (API 21+)
  - iOS (Planned)
  - Windows (Planned)
- **Application ID:** com.bellwoodglobal.driver
- **Version:** 1.0

### Key Technologies
- .NET 8 & MAUI
- CommunityToolkit.Mvvm (MVVM framework)
- Microsoft.Extensions.Http (HttpClient factory pattern)
- System.IdentityModel.Tokens.Jwt (JWT token validation)
- SecureStorage (encrypted token storage)
- Geolocation API (GPS tracking with background support)

## Current Capabilities

### Authentication & Security
- **JWT-based Authentication:** Secure login with encrypted token storage in SecureStorage
- **Token Expiration Validation:** Proactive token expiry checking with Unix timestamps
- **Session Management:** Automatic session expiry detection with user notifications
- **Role-Based Authorization:** Driver role required for all API operations
- **HTTPS Enforcement:** All API calls over secure connections
- **Background Location:** Support for continuous tracking when app is minimized

### Ride Management
- **Today's Rides:** View all assigned rides for the next 24 hours
- **Timezone Support:** Automatic timezone detection and header injection (`X-Timezone-Id`)
- **Pickup Time Display:** DateTimeOffset-based display with smart fallback for backward compatibility
- **Pull-to-Refresh:** Manual refresh of ride list
- **Ride Details:** Comprehensive ride information including:
  - Passenger name and contact information
  - Passenger count and luggage details (checked bags, carry-on)
  - Pickup and dropoff locations with map navigation
  - Pickup style and sign text for airport pickups
  - Vehicle class requirements
  - Flight information (flight number, tail number)
  - Special requests and additional instructions

### Status Management
- **FSM-Validated Transitions:** Finite state machine ensures valid status changes
- **Status Workflow:**
  ```
  Scheduled → OnRoute → Arrived → PassengerOnboard → Completed
  ```
- **Visual Status Indicators:** Color-coded status badges
- **Error Handling:** Detailed error messages for invalid transitions
- **Cancel Option:** Available at appropriate stages

### Real-Time Location Tracking
- **Automatic Activation:** Starts when trip begins (OnRoute status)
- **Dynamic Interval Adjustment:**
  - 30-second interval when >500m from destination
  - 15-second interval when within 500m (proximity mode)
- **Enhanced Location Data:**
  - GPS coordinates (latitude, longitude)
  - Heading (direction of travel in degrees)
  - Speed (meters/second)
  - Accuracy (meters)
  - Timestamp (UTC)
- **Haversine Distance Calculation:** Accurate proximity detection
- **Retry Logic:** Automatic retry on failed location sends (2 attempts, 1s delay)
- **Status Tracking:** Real-time visual indicator with detailed status messages
- **Background Support:** Continues when app is minimized (Android foreground service)
- **Automatic Cleanup:** Stops tracking on completion or cancellation
- **Error Recovery:** Graceful handling of 401, 400, 429 status codes

### Navigation
- **Native Map Integration:** Launch platform-specific map apps
  - iOS: Apple Maps
  - Android: Google Maps
- **Direct Links:** Navigate to pickup or dropoff locations
- **Geocoding Support:** Address-to-coordinates conversion for proximity tracking

### User Experience
- **Comprehensive Logging:** Enhanced debug logging for troubleshooting
- **Timezone Awareness:** All times display in driver's local timezone
- **Session Expiry Alerts:** Immediate user notification with auto-navigation to login
- **Visual Feedback:** GPS tracking indicator with color-coded status
- **Offline Detection:** Network error handling with retry mechanisms

## Project Structure

```
Bellwood.DriverApp/
├── Models/                          # Data Models & DTOs
│   └── ApiModels.cs                 # API contracts matching AdminAPI
├── Services/                        # Business Services
│   ├── IAuthService.cs
│   ├── AuthService.cs               # JWT auth with expiry validation
│   ├── IRideService.cs
│   ├── RideService.cs               # Ride management
│   ├── ILocationTracker.cs
│   └── LocationTracker.cs           # GPS tracking with retry logic
├── ViewModels/                      # MVVM ViewModels
│   ├── BaseViewModel.cs
│   ├── LoginViewModel.cs
│   ├── HomeViewModel.cs
│   └── RideDetailViewModel.cs       # Tracking status integration
├── Views/                           # XAML Pages
│   ├── LoginPage.xaml
│   ├── HomePage.xaml
│   └── RideDetailPage.xaml          # Enhanced tracking indicator
├── Handlers/                        # HTTP Message Handlers
│   ├── AuthHttpHandler.cs           # JWT injection & 401 handling
│   └── TimezoneHttpHandler.cs       # Timezone header injection
├── Helpers/                         # Utilities
│   ├── AppSettings.cs               # LocationConfig constants
│   └── Converters.cs                # Value converters
├── Platforms/                       # Platform-Specific Code
│   ├── Android/
│   │   ├── AndroidManifest.xml      # Background location permissions
│   │   └── MainActivity.cs
│   ├── iOS/
│   │   └── Info.plist               # Location usage strings
│   └── Windows/
├── Resources/                       # App Resources
│   ├── Images/
│   ├── Fonts/
│   └── Styles/
├── Docs/                            # Comprehensive Documentation
│   ├── DRIVER-TRACKING-IMPLEMENTATION.md
│   ├── AUTHORIZATION-FIX.md
│   ├── HOTFIX-TOKEN-EXPIRATION.md
│   ├── PHASE1-IMPLEMENTATION.md
│   ├── TIMEZONE-HEADER-IMPLEMENTATION.md
│   └── [additional docs...]
├── App.xaml
├── AppShell.xaml
└── MauiProgram.cs                   # Dependency injection & configuration
```

## Documentation

### Implementation Guides
- `Docs/DRIVER-TRACKING-IMPLEMENTATION.md` – Complete real-time tracking implementation
- `Docs/PHASE1-IMPLEMENTATION.md` – Timezone and pickup time fixes
- `Docs/TIMEZONE-HEADER-IMPLEMENTATION.md` – Worldwide timezone support
- `Docs/AUTHORIZATION-FIX.md` – Authorization header debugging and fixes
- `Docs/HOTFIX-TOKEN-EXPIRATION.md` – Token expiration validation fix

### Testing & Reference
- `Docs/TESTING-GUIDE.md` – Comprehensive testing scenarios
- `Docs/QUICK-REFERENCE.md` – Quick code snippets and examples
- `Docs/PHASE1-DEPLOYMENT-CHECKLIST.md` – Pre-deployment verification
- `Docs/BUGFIX-CTS-DISPOSAL.md` – CancellationTokenSource disposal fix

### API & Architecture
- `Docs/DRIVER_API_SUMMARY.md` – AdminAPI endpoint specifications
- `Docs/DEV-README.md` – Architecture and design decisions
- `Docs/ALIGNMENT-VERIFICATION.md` – API contract verification

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (17.8+) with MAUI workload
  - OR [Visual Studio Code](https://code.visualstudio.com/) with C# Dev Kit
- Platform-specific requirements:
  - **Android:** Android SDK API 21-35
  - **iOS:** macOS with Xcode 15+ (planned)
  - **Windows:** Windows 10 Build 17763+ (planned)

### Backend Services
The app requires the following services to be running:

1. **AuthServer** - JWT authentication (port 5001)
2. **AdminAPI** - Driver endpoints (port 5206)

## Getting Started

### 1. Clone the Repository

```sh
git clone https://github.com/BidumanADT/Bellwood.DriverApp.git
cd Bellwood.DriverApp
```

### 2. Restore Dependencies

```sh
dotnet restore
```

### 3. Configure API Endpoints

API endpoints are configured in `MauiProgram.cs`:

**Android Development:**
```csharp
// Android Emulator uses 10.0.2.2 to access host localhost
AuthServer: https://10.0.2.2:5001
AdminAPI:   https://10.0.2.2:5206
```

**iOS/Windows Development:**
```csharp
AuthServer: https://localhost:5001
AdminAPI:   https://localhost:5206
```

### 4. Build

```sh
# All targets
dotnet build

# Android specific
dotnet build -f net8.0-android

# iOS specific (requires macOS)
dotnet build -f net8.0-ios

# Windows specific
dotnet build -f net8.0-windows10.0.19041.0
```

### 5. Run

**Visual Studio:**
1. Open `Bellwood.DriverApp.sln`
2. Select target platform (Android Emulator recommended for testing)
3. Press F5 to build and deploy

**CLI:**
```sh
dotnet build -t:Run -f net8.0-android
```

### 6. Login

Use test driver credentials:
- Username: `charlie` (or your assigned driver username)
- Password: Your test driver password
- **Note:** Account must have `role=driver` and `uid` matching assigned rides

## Key Features Deep Dive

### Real-Time Location Tracking

**How It Works:**
1. Driver starts trip (status → OnRoute)
2. LocationTracker automatically begins GPS updates
3. Location sent to AdminAPI every 30 seconds (or 15s when near destination)
4. AdminAPI broadcasts location to PassengerApp via in-memory cache
5. Tracking stops automatically when ride is completed or cancelled

**Technical Details:**
- **Haversine Formula:** Calculates distance to destination for proximity detection
- **Retry Logic:** Up to 2 retry attempts with 1-second delay on failure
- **Race Condition Prevention:** 2-second delay before first update allows status change to propagate
- **Background Tracking:** Android foreground service keeps updates flowing when app minimized
- **Error Handling:** Graceful recovery from 400 Bad Request (status not yet active), 401 Unauthorized (re-login), 429 Too Many Requests

**Configuration (AppSettings.cs):**
```csharp
DefaultUpdateIntervalSeconds = 30      // Standard interval
ProximityUpdateIntervalSeconds = 15    // When within 500m
ProximityDistanceMeters = 500          // Proximity threshold
MaxRetryAttempts = 2                   // Failed update retries
LocationTimeoutSeconds = 10            // GPS request timeout
```

### Timezone Support

**Automatic Detection:**
- Device timezone detected on app start
- `X-Timezone-Id` header sent with every API request
- AdminAPI returns times in driver's local timezone
- Supports worldwide operations with automatic DST handling

**Platform-Specific:**
- **Android/iOS:** IANA format (`America/Chicago`)
- **Windows:** Windows format (`Central Standard Time`) - auto-converted by backend

### Token Management

**Security Features:**
- Tokens stored in platform-specific encrypted storage (Keychain on iOS, KeyStore on Android)
- Unix timestamp-based expiration validation (prevents timezone parsing issues)
- 1-minute expiration buffer prevents race conditions
- Automatic token cleanup on expiry
- User notification with auto-navigation to login on session expiry

**Debug Logging:**
```
🔐 Token stored, expires: 2025-12-21 01:43:12 UTC
    Unix timestamp: 1735177392
🕐 Expiry check:
    Token expires: 2025-12-21 01:43:12 UTC
    Current time:  2025-12-20 19:45:00 UTC
    Time until expiry: 358.2 minutes
    Is expired: False ✅
```

## Testing

### Quick Test Flow
1. Start AuthServer (port 5001) and AdminAPI (port 5206)
2. Seed test data:
   ```
   POST /bookings/seed
   POST /dev/seed-affiliates
   ```
3. Assign a driver to a booking via AdminPortal
4. Login with driver credentials
5. Test workflow:
   - View today's rides → Verify timezone-correct pickup times
   - Tap ride → View details → Verify all data displays
   - Update status → Verify FSM transitions
   - Start trip → Verify GPS tracking indicator turns green
   - Navigate to pickup → Verify native maps launch
   - Complete ride → Verify tracking stops

### Status Workflow Testing

```
✅ Confirmed → Start Trip → OnRoute
   - GPS tracking starts automatically
   - Green indicator: "Sharing location with passenger…"

✅ OnRoute → Mark Arrived → Arrived
   - Tracking continues with proximity-based intervals
   - Destination updates to dropoff location

✅ Arrived → Passenger Onboard → PassengerOnboard
   - Tracking continues to dropoff

✅ PassengerOnboard → Complete Ride → Completed
   - GPS tracking stops automatically
   - Ride removed from today's list
```

### Testing Scenarios

**Normal Operation:**
- ✅ Pickup times display correctly (no 6-hour shift)
- ✅ Authorization header present in all requests
- ✅ Location updates succeed every 30 seconds
- ✅ Status transitions follow FSM rules
- ✅ Token lasts full 7 hours

**Error Handling:**
- ✅ Session expiry shows alert and navigates to login
- ✅ Invalid status transitions show error messages
- ✅ Network errors trigger retry logic
- ✅ GPS permission denied shows helpful message
- ✅ 401 errors clear token and redirect to login

**Edge Cases:**
- ✅ App backgrounding maintains location tracking
- ✅ Cross-timezone accuracy (test with different device timezones)
- ✅ Rapid status changes don't cause race conditions
- ✅ Token expiration buffer prevents edge-case failures

## Platform Notes

### Android
- **Min SDK:** API 21 (Android 5.0)
- **Target SDK:** API 35
- **Permissions Required:**
  - `ACCESS_FINE_LOCATION` - GPS tracking
  - `ACCESS_COARSE_LOCATION` - Network-based location
  - `ACCESS_BACKGROUND_LOCATION` - Tracking when app minimized
  - `FOREGROUND_SERVICE` - Keep tracking alive
  - `FOREGROUND_SERVICE_LOCATION` - Location-specific foreground service
  - `INTERNET` - API communication
  - `ACCESS_NETWORK_STATE` - Network connectivity detection

### iOS (Planned)
- **Min:** iOS 11.0
- **Target:** iOS 18.0
- **Required Info.plist entries:**
  - `NSLocationWhenInUseUsageDescription`
  - `NSLocationAlwaysAndWhenInUseUsageDescription`
  - `UIBackgroundModes` with `location` and `fetch`

### Windows (Planned)
- **Min Build:** 17763
- **Location Services:** Windows Location API

## Deployment

### Build for Release

```sh
# Android
dotnet publish -f net8.0-android -c Release

# iOS (requires macOS)
dotnet publish -f net8.0-ios -c Release

# Windows
dotnet publish -f net8.0-windows10.0.19041.0 -c Release
```

### Pre-Deployment Checklist
- [ ] All tests passing
- [ ] Authorization headers verified in API logs
- [ ] Token expiration tested (7-hour lifetime)
- [ ] GPS tracking tested end-to-end
- [ ] Timezone header confirmed on all requests
- [ ] Status transitions validated
- [ ] Build successful on all target platforms
- [ ] Documentation updated

## Troubleshooting

### Common Issues

**Issue:** Times show 6 hours off
- **Solution:** Verify API returns `pickupDateTimeOffset` field, check XAML uses `DisplayPickupTime` binding

**Issue:** Authorization header missing
- **Solution:** Verify user logged in, check `AuthHttpHandler` registered in `MauiProgram.cs`, review console logs

**Issue:** GPS tracking not starting
- **Solution:** Check location permissions granted, verify ride status is OnRoute/Arrived/PassengerOnboard, review LocationTracker logs

**Issue:** Token expires immediately after login
- **Solution:** Verify system time accurate, check Unix timestamp in logs, ensure not comparing local time with UTC

**Issue:** Session expires too quickly
- **Solution:** Check server JWT configuration, verify device clock accuracy, adjust expiration buffer in `AuthService.cs`

### Debug Logging

Enhanced console logging available in DEBUG builds:

```
🌐 [TimezoneHttpHandler] Request: GET /driver/rides/today
   📍 X-Timezone-Id: America/Chicago
🔐 [AuthHttpHandler] Token added: eyJh...xyz
✅ Response: 200 OK
   🔐 Authorization: Present ✅
```

## Branches

- **main** - Stable production code
- **feature/driver-tracking** - Real-time tracking implementation (merged)
- **develop** - Integration branch for features

## Security & Standards

- **JWT Authentication:** Token-based with automatic header injection
- **Encrypted Storage:** SecureStorage for tokens (platform-specific encryption)
- **HTTPS Only:** All API calls over secure connections (dev builds allow local certs)
- **Role-Based Authorization:** Driver role required for all operations
- **Proactive Expiration:** Token validation before each request
- **Session Management:** Automatic cleanup and user notification
- **Location Privacy:** GPS data only shared during active trips
- **Code Standards:**
  - C# naming conventions
  - Async/await for I/O operations
  - Dependency injection pattern
  - MVVM separation of concerns
  - Nullable reference types enabled

## Performance Optimizations

- **Dynamic GPS Intervals:** Reduces battery drain by adjusting update frequency
- **Retry Logic:** Prevents unnecessary re-authentication on transient failures
- **Haversine Calculation:** Efficient distance calculation without external APIs
- **Background Tracking:** Android foreground service prevents process termination
- **Token Caching:** SecureStorage reduces authentication overhead

## Support

For issues or questions:
- Check AdminAPI Swagger docs (`/swagger` in dev mode)
- Review comprehensive documentation in `Docs/` folder
- Use GitHub issue tracker
- Contact mobile development team

## What's Next

### Phase 2 (Future Enhancements)
- **Token Refresh Flow:** Automatic silent re-authentication
- **Push Notifications:** Real-time ride assignment alerts
- **Offline Queue:** Store status updates when offline, sync when online
- **Trip History:** View completed rides and earnings
- **iOS Support:** Full iOS implementation with TestFlight distribution
- **Dark Mode:** Theme support for low-light conditions
- **Multilingual Support:** Internationalization for global drivers
- **Performance Metrics:** Tracking analytics and reporting
- **Driver Preferences:** Customizable app settings

### Phase 3 (Advanced Features)
- **SignalR Integration:** Real-time bidirectional communication
- **Advanced Navigation:** Turn-by-turn directions
- **Voice Commands:** Hands-free operation while driving
- **Earnings Dashboard:** Financial tracking and reporting
- **Rating System:** Passenger feedback integration
- **Fleet Management:** Multi-vehicle support

---

**Built with ❤️ for Bellwood Drivers**

*© 2025 Bellwood Global, Inc. All rights reserved.*
