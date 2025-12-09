# Bellwood Driver App

A minimalist .NET MAUI mobile application for Bellwood chauffeur drivers to manage their ride assignments, update statuses, and share real-time location with passengers.

---

## 🚀 Quick Start

### Prerequisites
- Visual Studio 2022 (17.8+) with .NET MAUI workload
- .NET 8 SDK
- Android SDK (for Android development)

### Required Services
The app requires the following backend services to be running:

1. **AuthServer** - JWT authentication (port 5001)
2. **AdminAPI** - Driver endpoints (port 5206)

### Running the App

1. **Start backend services** (AuthServer and AdminAPI)

2. **Run the app in Visual Studio**
   - Select Android Emulator as target
   - Press F5 to build and deploy

3. **Login with test credentials**
   - Username: Your test driver username
   - Password: Your test driver password
   - Account must have `role=driver` and `uid` matching assigned rides

---

## 📋 Phase 1 Features

### ✅ Complete
- **Authentication**: JWT-based login with SecureStorage
- **Ride List**: View today's assigned rides (next 24 hours)
- **Pull-to-Refresh**: Manual refresh of ride list
- **Ride Details**: Full ride information display
  - Passenger name and count
  - Pickup/dropoff locations with navigation buttons
  - Pickup style and sign text
  - Vehicle class
  - Luggage count (checked and carry-on)
  - Flight information (if applicable)
  - Special requests highlighted
- **Status Updates**: FSM-validated ride status transitions
  - Scheduled → On Route → Arrived → Passenger Onboard → Completed
  - Cancel option available at each step
- **Navigation**: Launch native maps app for pickup/dropoff locations
- **Location Tracking**: Real-time GPS updates (30s interval)
  - Automatic start when trip begins
  - Automatic stop on completion/cancellation
  - Visual indicator when tracking active
- **Sign Out**: Secure logout with confirmation

---

## 🏗️ Architecture

Built with .NET 8 MAUI using MVVM pattern:

- **Models/** - DTOs matching AdminAPI contracts
- **Services/** - AuthService, RideService, LocationTracker
- **ViewModels/** - CommunityToolkit.Mvvm-powered view models
- **Views/** - LoginPage, HomePage, RideDetailPage
- **Handlers/** - AuthHttpHandler for JWT injection
- **Helpers/** - AppSettings, value converters

### Key Technologies
- .NET 8 & MAUI
- CommunityToolkit.Mvvm
- Microsoft.Extensions.Http
- SecureStorage for encrypted token storage
- Geolocation API for GPS tracking

---

## 🔐 Security

- JWT tokens stored in encrypted SecureStorage
- HTTPS enforced for all API calls
- Self-signed cert handling in development
- Location permissions requested at runtime
- Role-based authorization (`driver` role required)

---

## 📍 Location Tracking

- Automatic start when trip begins (OnRoute status)
- 30-second update interval (configurable)
- Server-side rate limiting (15s minimum)
- Automatic stop on completion/cancellation
- Permission handling with user-friendly messages

---

## 🧪 Testing

### Quick Test Flow
1. Start AuthServer (port 5001) and AdminAPI (port 5206)
2. Seed test data: `POST /bookings/seed` and `POST /dev/seed-affiliates`
3. Assign a driver to a booking via AdminPortal
4. Login with driver credentials
5. Test: Tap ride → View details → Navigate → Update status → Location tracking

### Status Workflow
```
Scheduled → Start Trip → On Route
On Route → Mark Arrived → Arrived
Arrived → Passenger Onboard → Passenger Onboard
Passenger Onboard → Complete Ride → Completed
```

---

## 🛠️ Configuration

Android emulator uses `10.0.2.2` to access host `localhost`:
- AuthServer: `https://10.0.2.2:5001`
- AdminAPI: `https://10.0.2.2:5206`

Production URLs configured in `MauiProgram.cs`

---

## 📱 Supported Platforms

- ✅ **Android** - API 21+ (Android 5.0)
- 🔄 **iOS** - Planned for Phase 2
- 🔄 **Windows** - Planned for Phase 2

---

## 🎯 What's Next (Phase 2)

- Refresh token implementation
- Push notifications for new rides
- Offline status update queue
- Trip history and earnings
- iOS support
- Dark mode

---

## 📚 Documentation

- [DEV-README.md](DEV-README.md) - High-level design and architecture
- [DRIVER_API_SUMMARY.md](DRIVER_API_SUMMARY.md) - API specifications

---

## 📞 Support

For questions or issues:
- Check AdminAPI Swagger docs (`/swagger` in dev mode)
- Contact the mobile development team

---

**Built with ❤️ for Bellwood Drivers**
