# Bellwood Driver App – Developer README

Bellwood Driver App Design (Minimalist MAUI)
1 Overview

The Bellwood Driver App is a new cross‑platform mobile application, built with .NET MAUI, that allows hired drivers to quickly review their assignments, update ride status and share real‑time location while a ride is active. It is intentionally minimal, reflecting the less‑is‑more design philosophy: drivers already juggle several apps, so this one should focus only on the tasks that matter. The app must integrate seamlessly with the existing Bellwood AuthServer and AdminAPI using JWT‐based authentication and reuse the same signing key and validation rules. All communications must occur over HTTPS, and the app must respect user privacy by tracking location only when a ride is active.

2 Architecture
2.1 Platform and Layers

Cross‑platform: built with .NET MAUI, targeting iOS and Android with a single codebase. Use the MVVM pattern and CommunityToolkit.Mvvm or Prism for state management and navigation.

API clients: the app integrates with the existing AuthServer and AdminAPI (and later RidesAPI) through strongly typed HttpClient services. Development uses the Android emulator loopback 10.0.2.2:<port>; production uses the deployed HTTPS endpoints.

Shared DTOs: share data‑transfer classes with the AdminAPI via a shared project to ensure consistent models.

Dependency injection: register services such as AuthService, RideService, LocationTracker, TokenValidator and INavigationService for testability.

2.2 Authentication and Token Flow

Login endpoint: drivers authenticate via POST /api/auth/login on the AuthServer. The request contains username and password; the response returns an access token (JWT) and, once Phase 3 is implemented, a refresh token. The JWT contains claims sub (subject), uid (user identifier) and role=driver.

Token storage: persist the JWT in SecureStorage on the device. For Phase 1 a refresh token is not used; on expiry the app forces logout. Phase 3 may implement refresh‑token exchange.

Authorization header: a custom AuthHttpHandler injects Authorization: Bearer <token> on every call to the AdminAPI/RidesAPI. This logic is separate from UI components, following the lessons learned from the Passenger app.

Token validation: the AdminAPI must trust the same signing key and uses ValidateIssuer=false, ValidateAudience=false and ClockSkew=0 just like other Bellwood apps. The role=driver claim enables the AdminAPI to filter endpoints for drivers.

2.3 Server Endpoints

On the server the following endpoints are needed (protected with RequireAuthorization() and limited to role driver):

Endpoint	Purpose
GET /driver/rides/today	Returns today’s assignments for the authenticated driver (filtered by uid).
GET /driver/rides/{id}	Returns details of a specific ride.
POST /driver/rides/{id}/status	Updates the ride status (On route, Arrived, Passenger onboard, Completed).
POST /driver/location/update	Receives periodic latitude/longitude updates while a ride is active.

The AdminAPI should enforce that drivers only see their own rides and cannot access quote or administrative data.

3 Features
3.1 Login and Session Handling

Login screen (/driver/login): collects username and password and sends them to the AuthServer. Successful login stores the JWT and navigates to the Home screen. Failed login displays an error.

Session management: a TokenValidator monitors expiration and returns 401 triggers to AuthService. On invalid tokens, the app clears storage and returns to the login screen. Phase 3 will allow silent refresh using refresh tokens.

3.2 Home Screen (/driver/home)

Displays a list of today’s rides pulled from GET /driver/rides/today. Each item shows pickup time, passenger name, pickup and drop‑off locations, and current status.

Allows pull to refresh and uses caching to display the previous list offline. A “Refresh” button triggers a reload.

Tapping a ride navigates to the ride detail page.

3.3 Ride Details (/driver/ride/{id})

Shows full ride information from GET /driver/rides/{id}, including passenger contact details, special notes and estimated fare.

Includes a simple map widget or link to the native maps app to display the best route. Integration with mapping services like Google Maps or Apple Maps provides real‑time navigation. Map integration and real‑time location tracking are key features for logistics apps
nix-united.com
.

Contains buttons to update status: On Route, Arrived, Passenger Onboard and Completed. Each button sends POST /driver/rides/{id}/status with the new state and updates the UI.

3.4 Real‑Time Location Tracking

When a ride enters the active state (status = On Route, Arrived or Passenger Onboard), the app starts a background LocationTracker that requests GPS updates. Real‑time tracking allows apps to monitor vehicle positions precisely
nix-united.com
.

Every 10–20 seconds (configurable) the app posts the current latitude/longitude and ride ID to POST /driver/location/update. Tracking stops automatically when status is changed to Completed or Cancelled.

Users must grant location permissions at runtime. The app should clearly indicate when location sharing is active and stop tracking when rides are inactive. Limiting tracking to active rides protects privacy and conserves battery.

Optionally, the AdminAPI can return a shareable link or embed code so that passengers can view the driver’s location in their Passenger app. Location sharing improves coordination and builds trust
nix-united.com
.

3.5 Notifications (optional for Phase 1)

Future versions can integrate push notifications to alert drivers of new assignments, schedule changes or cancellations. For minimal release this is optional.

3.6 Sign Out & Error Handling

A “Sign out” menu item clears tokens, stops any ongoing location tracking and navigates to the login screen.

Display clear error messages for network issues, invalid credentials or server errors. Save user progress and maintain offline cache where possible.

4 User Experience & Design
4.1 Minimalist Design Principles

Less is more: minimalism relies on clean layouts, a restrained color scheme and ample whitespace
design-studio.medium.com
. Avoid extraneous graphics and decorative images; focus on function.

Clear typography and colors: choose a single typeface (e.g., system font) with variations in size and weight for headings and labels. Use only 2–3 primary colors for interface elements; additional colors are reserved for accent or notifications
design-studio.medium.com
design-studio.medium.com
.

Whitespace and grouping: space elements generously to reduce clutter and guide the eye. Group related items together and create natural boundaries using negative space
design-studio.medium.com
.

Universal icons: select simple, universally recognizable icons (e.g., phone, map, status indicators). Use solid icons for active states and outlined icons for inactive states
design-studio.medium.com
.

Simplicity over decoration: remove any element that does not directly support the user’s primary tasks. Avoid oversimplifying to the point of confusion
design-studio.medium.com
.

4.2 Focus on Core Tasks

Conduct user research to understand drivers’ needs and list the primary tasks they must accomplish; design those functionalities first
design-studio.medium.com
.

Critically evaluate each proposed feature and ask whether it is essential for the app’s purpose; unnecessary features should be removed or simplified
design-studio.medium.com
.

4.3 Navigation & Flow

Implement a predictable navigation pattern such as a bottom tab bar with a small number of items (e.g., Home and Settings). People prefer familiar navigation methods; label navigation elements with short, descriptive text and limit options to avoid overwhelming users
design-studio.medium.com
.

Follow UX principles: allow users to accomplish tasks without interruption; break larger tasks into manageable steps; minimize cognitive load
uxpin.com
. Never hide important information; emphasize high‑value elements with larger fonts or contrasting colors
uxpin.com
.

Users expect navigation to be predictable; adhere to common patterns and the three‑click rule so that any part of the app is reachable within a few taps
uxpin.com
.

4.4 Accessibility & Platform Guidelines

Respect platform UI guidelines (Apple Human Interface Guidelines and Android Material Design). Use appropriate font sizes and color contrast for readability. Provide dark‑mode support. Ensure touch targets meet recommended sizes for finger interaction.

Provide offline support: cache assignments and ride details and gracefully handle lost connectivity, resuming location updates when connectivity returns.

5 Security & Privacy Considerations

HTTPS everywhere: enforce TLS for all network calls. During development the Android emulator uses DangerousAcceptAnyServerCertificateValidator to accept self‑signed certificates; production must use trusted certificates.

Token storage: store JWTs in SecureStorage; never embed secrets in the app bundle. Clear tokens when logging out or on expiration.

Role‑based access: tokens include role=driver claim, and the server uses this to authorize driver endpoints. Drivers cannot call staff‑only endpoints (quotes, bookings for other drivers, etc.).

Least privilege: the app requests location permissions only when starting a ride. Stop tracking when the ride is finished to protect user privacy.

Backend data filtering: AdminAPI filters results based on the token’s uid claim; drivers only receive their own rides. All write operations (status changes, location updates) are validated against the ride’s ownership.

Key management: the shared JWT signing key must be moved to a secure secret store (e.g., Azure Key Vault) before production, as recommended in the knowledge pack.

6 Implementation Details and Best Practices

Reusable Services: implement AuthService, RideService and LocationTracker as singleton services injected via dependency injection. Use HttpClientFactory to create typed clients with the authentication handler attached.

Token Validation: create an ITokenValidator service to check expiry and manage refresh logic. Attach it to a DelegatingHandler that catches 401 responses and triggers re‑authentication.

Geolocation: use Microsoft.Maui.Essentials.Geolocation for foreground location and BackgroundService or platform‑specific background tasks for continuous updates while the ride is active. Balance update frequency with battery consumption.

Maps & Navigation: embed a simple map control (MAUI Map or community library) to display pickup and drop‑off points. Provide a button to launch the native maps app with coordinates for navigation.

Caching & Offline: store today’s rides and ride details in a lightweight local database (e.g., SQLite or Preferences) to support offline viewing. Sync with the server when connectivity returns.

Testing: implement unit tests for services and view models and UI tests for critical flows (login, listing rides, updating status). Use dependency injection and mock services to isolate tests.

7 Server‑Side Enhancements and Dependencies

AdminAPI: implement the driver endpoints listed above with proper authorization and filtering. Add a DriverRideService to retrieve assignments based on the driver’s user ID and enforce status transitions.

Location broadcasting: the server should use the received location data to update the ride’s current position and optionally push updates via SignalR/WebSockets to the Admin Portal and Passenger app for real‑time tracking.

Ride assignment: ensure there is an internal dispatcher or automated logic to assign rides to drivers and update them in real time. A push notification system could be added later.

CORS and versioning: configure CORS policies to allow the mobile app to access the API. Version the endpoints so that future changes do not break existing clients.

8 Future Enhancements

Refresh tokens: implement refresh token flow in both AuthServer and mobile clients to avoid forcing drivers to re‑authenticate frequently.

Push notifications: integrate with Apple/Google push notification services to notify drivers of new rides, cancellations or changes.

Offline job completion: allow drivers to update statuses offline and sync when connectivity is restored.

Integration with RidesAPI and LimoAnywhere: unify ride management across systems and provide richer features like route optimization, price calculation and analytics.

Accessibility improvements: add voice guidance and haptic feedback for hands‑free operation.

Telemetry & analytics: collect anonymized usage data (with user consent) to improve app performance and user experience.

9 Conclusion

The Bellwood Driver App should be a focused, reliable companion for drivers. It leverages the existing Bellwood authentication architecture and AdminAPI to deliver core functionality—viewing assigned rides, updating ride statuses and sharing real‑time location—while adhering to strict privacy and security requirements. A minimalist design with clear navigation, restrained colors and purposeful layout helps drivers accomplish tasks quickly and intuitively, aligning with UX research that emphasizes reducing cognitive load and avoiding clutter
uxpin.com
. By starting with this minimal feature set and solid architecture, the app can evolve incrementally with features like refresh tokens, push notifications and deeper integrations without overwhelming drivers.