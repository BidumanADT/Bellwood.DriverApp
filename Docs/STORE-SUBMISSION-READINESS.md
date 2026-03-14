# Store Submission Readiness Report — Bellwood Driver App

**Document Type**: Living Document — Planning & Operations  
**Last Updated**: January 2026  
**Status**: ?? Active — Action Required  
**Applies To**: Bellwood Driver App (Android / iOS)  
**Audience**: Development, Mobile, DevOps, Product

---

## ?? Executive Summary

This report captures the full readiness assessment of the Bellwood Driver App for submission to the **Google Play Store** (internal testing track) and the **Apple App Store** (TestFlight internal testing).

The app's core logic — authentication, ride management, GPS tracking, timezone handling, and HTTP pipeline — is well-built and close to shippable for Android internal testing. However, **five hard blockers** must be resolved before a signed `.aab` can be uploaded to Google Play, and iOS requires its target to be re-enabled and the full Apple provisioning chain set up from scratch.

**Immediate action is required on all items marked ?? BLOCKER.**

---

## ? What's Solid (Ship-Ready)

The following areas passed review and require no changes before submission.

| Area | Notes |
|---|---|
| **Architecture** | Clean MVVM, DI-first, interface-backed services throughout |
| **Authentication** | JWT, SecureStorage, expiry validation, 401 handling all correct |
| **Ride management** | List, detail, FSM-validated status transitions working |
| **GPS tracking** | Retry logic, dynamic intervals, Haversine proximity, cancellation cleanup |
| **Timezone handling** | `X-Timezone-Id` header on every request; `DisplayPickupTime` fallback pattern |
| **Android manifest — permissions** | All required location, foreground service, and network permissions declared |
| **iOS `Info.plist`** | All 3 location usage strings present; background modes declared |
| **HTTP pipeline** | Middleware chain order correct (`TimezoneHttpHandler ? AuthHttpHandler ? Handler`) |
| **Debug vs. Release separation** | Dev-cert bypass and emulator loopback URLs properly `#if DEBUG` guarded |
| **Forward-compat dispatcher fields** | Nullable optional fields hidden via `IsNotNullConverter` until API returns them |
| **Release signing guard** | Keystore task only fires when `$(AndroidSigningKeyStore)` is actually provided |

---

## ?? Blockers — Must Fix Before Submission

These items will result in a **rejected build or a crashed/broken app in production**. None are optional.

---

### BLOCKER 1 — No Android Foreground Service `<service>` declaration

**Affects**: Google Play submission, background GPS tracking  
**Risk**: Build rejection by Google Play; OS kills tracking after a few minutes in background

You have `FOREGROUND_SERVICE` and `FOREGROUND_SERVICE_LOCATION` *permissions* declared in `AndroidManifest.xml`, but there is **no `<service>` element** in the manifest. Google Play requires API 34+ apps to declare a foreground service with `android:foregroundServiceType="location"`. Without it the Play submission will be rejected, and even if it weren't, the OS will kill the tracking loop when the app is backgrounded.

**Fix required in**: `Platforms/Android/AndroidManifest.xml`

```xml
<service
    android:name=".LocationForegroundService"
    android:foregroundServiceType="location"
    android:exported="false" />
```

A corresponding `LocationForegroundService` class must also be implemented (see Blocker 3).

---

### BLOCKER 2 — `android:usesCleartextTraffic="true"` in release manifest

**Affects**: Google Play submission  
**Risk**: Automatic rejection; security vulnerability in production builds

`android:usesCleartextTraffic="true"` is set unconditionally in `AndroidManifest.xml`. Google rejects apps that globally permit cleartext traffic. This flag should be removed from the release manifest and, if needed for local dev, scoped to a **debug-only** network security config.

**Fix required in**: `Platforms/Android/AndroidManifest.xml` (remove flag) and optionally `Platforms/Android/Resources/xml/network_security_config.xml` (scope to debug).

---

### BLOCKER 3 — Background location tracking is not a true Android Foreground Service

**Affects**: Android background GPS tracking during active rides  
**Risk**: OS terminates the tracking loop within minutes of app being backgrounded — drivers lose live tracking mid-ride

The current `LocationTracker` runs a `Task.Run` loop. This is **not** a bound or started Android foreground service — it has no persistent notification, no `Service` class, and no `StartForeground()` call. The manifest declares the intent (Blocker 1) but the implementation does not back it up. The OS will kill the background task on any modern Android version.

**Fix required**: Create a `LocationForegroundService` class in `Platforms/Android/` that extends Android's `Service`, calls `StartForeground()` with a persistent notification, and hosts the tracking loop for the duration of an active ride.

---

### BLOCKER 4 — `RideDetailViewModel.Cleanup()` is never called

**Affects**: All platforms  
**Risk**: Memory leak and event-handler accumulation on every navigation to/from ride detail; will cause crashes on repeated use in production

`RideDetailViewModel` subscribes to three `ILocationTracker` events in its constructor:
- `TrackingStatusChanged`
- `LocationUpdateFailed`
- `LocationSent`

The `Cleanup()` method that unsubscribes from all three exists but is **never invoked** — not in `RideDetailPage.xaml.cs`, not in `OnDisappearing`, nowhere. Every navigation to a ride detail page that is then navigated away from leaks all three handlers.

**Fix required in**: `Views/RideDetailPage.xaml.cs` — override `OnDisappearing` and call `viewModel.Cleanup()`.

---

### BLOCKER 5 — No Android signing keystore exists / no documented process

**Affects**: Google Play submission  
**Risk**: A signed `.aab` cannot be produced; store submission is impossible without it

There is no `.jks` or `.keystore` file, no CI secret, and no local developer secret supplying `$(AndroidSigningKeyStore)`. The project file now guards the signing task correctly, but the keystore itself must be created, stored securely (outside the repo), and the build process documented so any authorized team member can produce a signed release build.

**Fix required**: 
1. Generate a keystore: `keytool -genkey -v -keystore bellwood-driver.jks -alias bellwood -keyalg RSA -keysize 2048 -validity 10000`  
2. Store it in a secure secrets manager (Azure Key Vault, GitHub Encrypted Secrets, or equivalent).  
3. Document the process in `Docs/30-Deployment-Guide.md`.  
4. ?? **Never commit the `.jks` file to the repository.**

---

### BLOCKER 6 — iOS target is not built, tested, or provisioned

**Affects**: Apple App Store / TestFlight submission  
**Risk**: iOS submission is completely blocked

The `net8.0-ios` target is commented out of the `.csproj`. Beyond re-enabling the target, the full Apple provisioning chain is not yet in place:

- [ ] App ID registered on Apple Developer portal (`com.bellwoodglobal.driver`)
- [ ] Distribution provisioning profile created and downloaded
- [ ] iOS distribution certificate in the Mac keychain
- [ ] `Entitlements.plist` with background modes for iOS (location, fetch)
- [ ] App Store Connect record created
- [ ] TestFlight internal testing group configured
- [ ] Build verified on a physical iOS device or Simulator via a Mac build host

**Fix required**: All of the above. This is the most time-intensive blocker and should be started in parallel with the Android fixes.

---

## ?? High-Priority Issues

These will not block submission but **will cause significant problems in production** or with real drivers on live trips.

---

### HIGH 1 — No token refresh (silent re-authentication)

**Affects**: All platforms  
**Risk**: Drivers are kicked to the login screen mid-ride when their access token expires

`RefreshToken` is modelled in `LoginResponse` and noted as Phase 3, but it is never stored or used. When the access token expires during an active ride, `AuthHttpHandler` clears the token and navigates to `//LoginPage` — interrupting a live tracking session. This is a serious driver UX issue and should be resolved before live operations begin.

**Recommended fix**: Store the refresh token in `SecureStorage` at login; attempt a silent refresh in `GetAccessTokenAsync()` before the token expires; only force logout if the refresh also fails.

---

### HIGH 2 — `Console.WriteLine` used in production code paths

**Affects**: All platforms  
**Risk**: Log noise on Android `logcat`; silent on iOS; some paths log partial token data

There are dozens of `Console.WriteLine` calls outside `#if DEBUG` guards in `RideService`, `LocationTracker`, `AuthService`, and others. On iOS these calls produce no output at all, meaning production diagnostics are silently lost. On Android they pollute `logcat`. Some paths log token previews that should never appear in release builds.

**Recommended fix**: Replace all non-guarded `Console.WriteLine` calls with `#if DEBUG` guards or migrate to `ILogger<T>` injected via DI (already configured via `builder.Logging.AddDebug()`).

---

### HIGH 3 — No push notifications

**Affects**: All platforms  
**Risk**: Drivers have no way to know a new ride has been assigned while the app is closed or in the background

This is noted as Phase 2 in the roadmap and is not a submission blocker, but it is a significant gap for internal testers who will expect to be notified of new assignments without needing to manually refresh the app.

**Recommended fix**: Integrate Firebase Cloud Messaging (FCM) for Android and Apple Push Notification Service (APNs) for iOS. A backend notification trigger on ride assignment is also required.

---

## ??? PRIORITY — App Icon (Replace Immediately)

> **This item has been escalated to high priority.** App icon work is notoriously time-consuming — asset generation across every required size and density, platform-specific format requirements (adaptive icons on Android, `.appiconset` on iOS), and the MAUI resource pipeline all interact in ways that routinely block builds at the last minute. Do **not** leave this until pre-submission. Start it now in parallel with the blockers above.

**Affects**: Both stores  
**Risk**: Generic placeholder icon is present; both stores flag this in review; embarrassing for internal testers and guaranteed to draw comments

The `appicon.svg` / `appiconfg.svg` files appear to be the default MAUI purple diamond placeholder. Both the Play Store and App Store will flag a generic/stock icon during review. Beyond the review risk, internal testers will see the placeholder icon on their home screens from day one, which sets a poor first impression for a product that is otherwise polished.

**What is needed**:

| Platform | Format | Required Sizes |
|---|---|---|
| Android | Adaptive icon (foreground + background layers) | mdpi, hdpi, xhdpi, xxhdpi, xxxhdpi + Play Store 512×512 PNG |
| iOS | `Assets.xcassets/appicon.appiconset` | 20pt ? 1024pt across all scales |

**Recommended actions**:
1. Obtain final Bellwood brand assets from the design team immediately.
2. Use a tool such as [IconKitchen](https://icon.kitchen) or [MakeAppIcon](https://makeappicon.com) to generate all required sizes from a single master SVG.
3. For Android adaptive icons, provide separate foreground (`appiconfg.svg`) and background layers.
4. Replace both `Resources/AppIcon/appicon.svg` and `Resources/AppIcon/appiconfg.svg` with the final branded assets.
5. Verify the icon renders correctly in the emulator before moving on.

**Do not wait until submission week to address this.**

---

## ?? Polish / Minor Gaps

These are low-risk items that should be addressed before going live but will not block an internal test submission.

| # | Item | Notes |
|---|---|---|
| P1 | **Production API URLs** | Confirm `auth.elitebellwood.com` and `api.elitebellwood.com` have DNS records, valid TLS certs, and point to deployed services |
| P2 | **Version numbers** | `ApplicationDisplayVersion=1.0` / `ApplicationVersion=1` — confirm these align with what is registered on App Store Connect and Google Play Console |
| P3 | **`IRideService` API contract** | Do a quick pass to confirm all endpoint paths and response shapes match the current AdminAPI contract before the first internal test |

---

## ?? Recommended Action Order

Work the list top-to-bottom. Items in the same group can be parallelised across team members.

```
GROUP A — Start immediately (parallel)
  ?? BLOCKER 5  ? Generate and securely store Android keystore; document the process
  ?? BLOCKER 6  ? Register iOS App ID; begin Apple provisioning chain
  ???  ICON       ? Get brand assets from design; generate and replace icon files NOW

GROUP B — Android fixes (sequential)
  ?? BLOCKER 2  ? Remove android:usesCleartextTraffic from release manifest
  ?? BLOCKER 1  ? Add <service> foreground service declaration to AndroidManifest.xml
  ?? BLOCKER 3  ? Implement LocationForegroundService Android service class

GROUP C — App code fixes
  ?? BLOCKER 4  ? Call RideDetailViewModel.Cleanup() in RideDetailPage.OnDisappearing
  ??  HIGH 2    ? Guard or replace all Console.WriteLine with #if DEBUG / ILogger<T>

GROUP D — Pre-live (can follow first internal test)
  ??  HIGH 1    ? Implement silent token refresh flow
  ??  HIGH 3    ? Integrate FCM / APNs push notifications

GROUP E — Polish (before public release)
  ?? P1         ? Verify production DNS and TLS
  ?? P2         ? Confirm version numbers match store registrations
  ?? P3         ? Verify IRideService endpoint contract against AdminAPI
```

---

## ?? Submission Readiness Summary

| Platform | Status | Blockers Remaining |
|---|---|---|
| **Android — Internal Testing** | ?? Not Ready | 5 (Blockers 1–5) |
| **Android — Production** | ?? Not Ready | 5 + High 1, High 2 |
| **iOS — TestFlight** | ?? Not Ready | Blocker 6 + full provisioning chain |
| **iOS — Production** | ?? Not Ready | All of the above |

---

## ?? Related Documentation

- `Docs/PHASE1-DEPLOYMENT-CHECKLIST.md` — Pre-deployment verification checklist
- `Docs/DRIVER-TRACKING-IMPLEMENTATION.md` — GPS tracking implementation detail
- `Docs/AUTHORIZATION-FIX.md` — Auth handler and 401 handling
- `Docs/HOTFIX-TOKEN-EXPIRATION.md` — Token expiration validation
- `Docs/TESTING-GUIDE.md` — End-to-end test scenarios
- `README.md` — Platform notes and deployment commands

---

**Last Updated**: January 2026  
**Status**: ?? Active — Action Required  
**Next Review**: After Group A and Group B items are resolved
