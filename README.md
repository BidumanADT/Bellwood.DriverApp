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
   - Account must have `role=driver` and `uid=driver-001`

---

## 📋 Features Implemented

### ✅ Phase 1 Complete
- JWT-based authentication with SecureStorage
- View today's assigned rides
- Detailed ride information display
- FSM-validated ride status updates
- Real-time GPS location tracking (30s interval)
- Call passenger directly from app
- Navigate to pickup/dropoff via native maps
- Pull-to-refresh ride list
- Automatic token injection via AuthHttpHandler
- 401 handling with auto sign-out
- Comprehensive error handling

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

See [TESTING-GUIDE.md](TESTING-GUIDE.md) for comprehensive test scenarios.

### Quick Test Flow
1. Start AuthServer (port 5001) and AdminAPI (port 5206)
2. Seed test data: `POST /bookings/seed`
3. Login with driver credentials
4. Test ride list, detail view, status updates, location tracking

---

## 📚 Documentation

- [DEV-README.md](DEV-README.md) - High-level design and architecture
- [DRIVER_API_SUMMARY.md](DRIVER_API_SUMMARY.md) - API specifications
- [TESTING-GUIDE.md](TESTING-GUIDE.md) - Test scenarios and checklist

---

## 🛠️ Configuration

Android emulator uses `10.0.2.2` to access host `localhost`:
- AuthServer: `https://10.0.2.2:5001`
- AdminAPI: `https://10.0.2.2:5206`

Production URLs configured in `Helpers/AppSettings.cs`

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

## 📞 Support

For questions or issues:
- Review [TESTING-GUIDE.md](TESTING-GUIDE.md)
- Check AdminAPI Swagger docs (`/swagger` in dev mode)
- Contact the mobile development team

---

**Built with ❤️ for Bellwood Drivers**
