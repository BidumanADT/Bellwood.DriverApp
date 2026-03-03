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
- Display dispatcher-attached passenger contact details and location notes when available

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
- **Role-Based Authorization:** `driver` role required for all API operations; JWT must carry `userId`, `uid` (or `sub`) and `role` claims matching the AuthServer's persistent SQLite identity store
- **HTTPS Enforcement:** All API calls over secure connections
- **Background Location:** Support for continuous tracking when app is minimized
- **AuthServer Compatibility:** Authentication is fully database-agnostic — the app sends credentials and stores the returned JWT. Switching the AuthServer from an in-memory to a persistent SQLite store requires no driver-app code changes.

### Ride Management
- **Today's Rides:** View all assigned rides for the next 24 hours
- **Timezone Support:** Automatic timezone detection and header injection (`X-Timezone-Id`)
- **Pickup Time Display:** DateTimeOffset-based display with smart fallback for backward compatibility
- **Pull-to-Refresh:** Manual refresh of ride list
- **Ride Details:** Comprehensive ride information including:
  - Passenger name and contact information
  - Passenger count and luggage details (checked bags, carry-on)
  - Pickup and dropoff locations with map navigation
  - Dispatcher-supplied saved location labels (e.g. "ORD Terminal 1 – Door 4B") — displayed automatically when present
  - Pickup style and sign text for airport pickups
  - Vehicle class requirements
  - Flight information (flight number, tail number)
  - Special requests and additional instructions
  - Dispatcher notes — displayed automatically when present
  - Saved passenger phone and email — displayed automatically when present

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
├── Models/
│   └── ApiModels.cs                 # DTOs matching AdminAPI contracts;
│                                    # includes nullable future fields for
│                                    # saved passenger/location context
├── Services/
│   ├── IAuthService.cs
│   ├── AuthService.cs               # JWT auth with Unix-timestamp expiry validation
│   ├── IRideService.cs
│   ├── RideService.cs               # Ride management
│   ├── ILocationTracker.cs
│   └── LocationTracker.cs           # GPS tracking with retry logic
├── ViewModels/
│   ├── BaseViewModel.cs
│   ├── LoginViewModel.cs
│   ├── HomeViewModel.cs
│   └── RideDetailViewModel.cs       # Tracking status integration
├── Views/
│   ├── LoginPage.xaml
│   ├── HomePage.xaml
│   └── RideDetailPage.xaml          # Displays saved passenger / location context
│                                    # when AdminAPI populates optional fields
├── Handlers/
│   ├── AuthHttpHandler.cs           # JWT injection & 401 handling
│   └── TimezoneHttpHandler.cs       # X-Timezone-Id header injection
├── Helpers/
│   ├── AppSettings.cs               # LocationConfig constants
│   └── Converters.cs                # Value converters (IsNotNullConverter etc.)
├── Platforms/
│   ├── Android/
│   │   ├── AndroidManifest.xml      # Background location permissions
│   │   └── MainActivity.cs
│   ├── iOS/
│   │   └── Info.plist               # Location usage strings
│   └── Windows/
├── Resources/
│   ├── Images/
│   ├── Fonts/
│   └── Styles/
├── Docs/                            # Comprehensive documentation
│   ├── DRIVER-TRACKING-IMPLEMENTATION.md
│   ├── AUTHORIZATION-FIX.md
│   ├── HOTFIX-TOKEN-EXPIRATION.md
│   ├── PHASE1-IMPLEMENTATION.md
│   ├── TIMEZONE-HEADER-IMPLEMENTATION.md
│   └── [additional docs...]
├── App.xaml
├── AppShell.xaml
└── MauiProgram.cs                   # DI registration & HttpClient configuration
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

1. **AuthServer** – JWT authentication (port 5001); must issue tokens containing `userId` (Identity GUID), `uid`, `sub`, and a flat `role` claim equal to `driver`
2. **AdminAPI** – Driver endpoints (port 5206)

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
- **Note:** Account must have `role=driver` and a `uid` / `userId` claim matching the driver's assigned rides

## Key Features Deep Dive

### Authentication Compatibility with Persistent AuthServer

The driver app is **fully compatible** with an AuthServer backed by a persistent SQLite database. The app's authentication flow is stateless with respect to the server's storage mechanism:

1. `POST /api/auth/login` → AuthServer validates credentials against its store (in-memory or SQLite) and returns a JWT
2. The JWT is stored in platform-encrypted SecureStorage
3. Every subsequent request attaches the token via `AuthHttpHandler`; no session or server-side state is involved

**Required JWT claims (verified at login, used by AdminAPI):**

| Claim | Value | Notes |
|-------|-------|-------|
| `userId` | Identity GUID | Preferred; set by AuthServer from Identity store |
| `uid` | Identity GUID | Fallback if `userId` absent |
| `sub` | Identity GUID | Fallback if both above absent |
| `role` | `driver` | Flat string; required for all driver endpoints |

No driver-app code changes are needed when the AuthServer transitions from in-memory to SQLite.

---

### Dispatcher-Attached Passenger & Location Context (Forward-Compatible)

`DriverRideDetailDto` contains a set of **nullable optional fields** that are silently ignored today but will be displayed automatically once the AdminAPI begins populating them:

| Field | Source | Displayed As |
|-------|--------|--------------|
| `SavedPassengerPhone` | Dispatcher's saved passenger record | 📞 in Passenger Information card |
| `SavedPassengerEmail` | Dispatcher's saved passenger record | ✉️ in Passenger Information card |
| `SavedPickupLocationLabel` | Dispatcher's saved location record | Bold label above pickup address |
| `SavedDropoffLocationLabel` | Dispatcher's saved location record | Bold label above dropoff address |
| `DispatcherNotes` | Dispatcher free-text annotation | 🗂️ blue card in Ride Details section |

All five fields are hidden via `IsNotNullConverter` when absent, so the existing UI is completely unchanged for bookings that do not carry this data.

**No further UI or model changes are required** when the AdminAPI starts returning these fields.

---

### Real-Time Location Tracking

**How It Works:**
1. Driver starts trip (status → OnRoute)
2. `LocationTracker` automatically begins GPS updates
3. Location sent to AdminAPI every 30 seconds (or 15s when near destination)
4. AdminAPI broadcasts location to PassengerApp via in-memory cache
5. Tracking stops automatically when ride is completed or cancelled

**Technical Details:**
- **Haversine Formula:** Calculates distance to destination for proximity detection
- **Retry Logic:** Up to 2 retry attempts with 1-second delay on failure
- **Race Condition Prevention:** 2-second delay before first update allows status change to propagate
- **Background Tracking:** Android foreground service keeps updates flowing when app minimised
- **Error Handling:** Graceful recovery from 400 Bad Request (status not yet active), 401 Unauthorized (re-login), 429 Too Many Requests

**Configuration (`Helpers/AppSettings.cs`):**
```csharp
DefaultUpdateIntervalSeconds    = 30   // Standard update cadence
ProximityUpdateIntervalSeconds  = 15   // When within 500m of destination
ProximityDistanceMeters         = 500  // Proximity threshold
MaxRetryAttempts                = 2    // Retries per failed send
LocationTimeoutSeconds          = 10   // GPS request timeout
```

---

### Timezone Support

**Automatic Detection:**
- Device timezone detected on app start via `TimeZoneInfo.Local.Id`
- `X-Timezone-Id` header sent with every API request via `TimezoneHttpHandler`
- AdminAPI returns `PickupDateTimeOffset` with the correct UTC offset for the driver's locale
- `DisplayPickupTime` helper property on both DTOs prefers `PickupDateTimeOffset`; falls back to `PickupDateTime` with local offset applied

**Platform-Specific:**
- **Android/iOS:** IANA format (`America/Chicago`)
- **Windows:** Windows format (`Central Standard Time`) — auto-converted by the backend

---

### Token Management

**Security Features:**
- Tokens stored in platform-specific encrypted storage (Keychain on iOS, KeyStore on Android)
- Unix timestamp expiration validation — avoids `DateTime.Kind` parsing pitfalls across timezones
- 1-minute expiration buffer prevents race conditions at boundary
- Automatic token cleanup on expiry; user notified with auto-navigation to login
- Token preview (first 4 / last 4 chars) logged in DEBUG builds only

**Example Debug Output:**
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
   - Tap ride → View details → Verify all fields display
   - Update status → Verify FSM transitions
   - Start trip → Verify GPS indicator turns green
   - Navigate to pickup → Verify native maps launch
   - Complete ride → Verify tracking stops

### Status Workflow Testing

```
✅ Confirmed → Start Trip → OnRoute
   - GPS tracking starts automatically
   - Green indicator: "Sharing location with passenger…"

✅ OnRoute → Mark Arrived → Arrived
   - Tracking continues with proximity-based intervals

✅ Arrived → Passenger Onboard → PassengerOnboard
   - Tracking continues; destination updates to dropoff

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
- ✅ Token lasts full duration (server-configured lifetime)

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

**Future / Dispatcher Context:**
- ✅ Booking with no saved passenger/location data → extra fields hidden, no regressions
- ✅ Booking with `SavedPassengerPhone` populated → phone row visible in Passenger card
- ✅ Booking with `SavedPickupLocationLabel` populated → label shown above pickup address
- ✅ Booking with `DispatcherNotes` populated → blue card visible in Ride Details section

## Platform Notes

### Android
- **Min SDK:** API 21 (Android 5.0)
- **Target SDK:** API 35
- **Permissions Required:**
  - `ACCESS_FINE_LOCATION`
  - `ACCESS_COARSE_LOCATION`
  - `ACCESS_BACKGROUND_LOCATION`
  - `FOREGROUND_SERVICE`
  - `FOREGROUND_SERVICE_LOCATION`
  - `INTERNET`
  - `ACCESS_NETWORK_STATE`

### iOS (Planned)
- **Min:** iOS 11.0 · **Target:** iOS 18.0
- **Required `Info.plist` entries:** `NSLocationWhenInUseUsageDescription`, `NSLocationAlwaysAndWhenInUseUsageDescription`, `UIBackgroundModes` → `location`, `fetch`

### Windows (Planned)
- **Min Build:** 17763 · Location via Windows Location API

## Deployment

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
- [ ] Token expiration tested (full server-configured lifetime)
- [ ] GPS tracking tested end-to-end
- [ ] Timezone header confirmed on all requests
- [ ] Status transitions validated
- [ ] Dispatcher context fields verified (hidden when null, visible when populated)
- [ ] Build successful on all target platforms
- [ ] Documentation updated

## Troubleshooting

| Symptom | Steps |
|---------|-------|
| Times show 6 hours off | Verify API returns `pickupDateTimeOffset`; check XAML uses `DisplayPickupTime` |
| Authorization header missing | Verify login succeeded; check `AuthHttpHandler` registered; review console logs |
| GPS tracking not starting | Check location permissions; verify status is OnRoute/Arrived/PassengerOnboard |
| Token expires immediately after login | Check device clock; review Unix timestamp in logs |
| Session expired loop | Rebuild app after `HOTFIX-TOKEN-EXPIRATION.md` changes; clear SecureStorage |
| Dispatcher fields not showing | Confirm AdminAPI is returning the optional fields; check `IsNotNullConverter` is registered |

### Debug Logging (DEBUG builds)

```
═══════════════════════════════════════════════════
📍 TIMEZONE DETECTION
   Device Timezone ID: America/Chicago
   Current UTC Offset: -6.0 hours
═══════════════════════════════════════════════════
🌐 [TimezoneHttpHandler] Request: GET /driver/rides/today
   📍 X-Timezone-Id: America/Chicago
🔐 [AuthHttpHandler] Token added: eyJh...xyz
✅ Response: 200 OK
   🔐 Authorization: Present ✅
```

## Branches

- **main** – Stable production code
- **wip/pre-alpha-gap-fix-persistence** – Active: persistence compatibility + dispatcher context
- **feature/driver-tracking** – Real-time tracking implementation (merged)
- **develop** – Integration branch

## Security & Standards

- JWT authentication with automatic Bearer-token injection
- SecureStorage (Keychain / KeyStore) for encrypted token persistence
- HTTPS for all API traffic; dev builds allow local self-signed certificates
- `driver` role enforcement on all protected endpoints
- Proactive Unix-timestamp token expiration validation
- GPS data shared only during active trips
- C# naming conventions, async/await I/O, DI-first, MVVM, nullable reference types enabled

## What's Next

### Phase 2 (Near-term)
- **Token Refresh Flow:** Silent re-authentication using refresh tokens
- **Push Notifications:** Real-time ride assignment alerts
- **Offline Queue:** Buffer status updates when offline; sync on reconnect
- **Trip History:** View completed rides and earnings summary
- **iOS Support:** Full implementation with TestFlight distribution
- **Dark Mode:** Low-light theme support
- **Dispatcher Context (UI polish):** Tap-to-call on `SavedPassengerPhone`; tap-to-map on saved location labels

### Phase 3 (Advanced)
- **SignalR Integration:** Real-time bidirectional events
- **Turn-by-Turn Navigation:** In-app routing
- **Earnings Dashboard:** Financial tracking
- **Rating System:** Post-trip passenger feedback
- **Fleet Management:** Multi-vehicle support

---

**Built with ❤️ for Bellwood Drivers**

*© 2025 Bellwood Global, Inc. All rights reserved.*
