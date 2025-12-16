# Bellwood Driver App - Testing Guide

## ?? Testing Checklist

### Prerequisites
- ? AuthServer running on `https://localhost:5001`
- ? AdminAPI running on `https://localhost:5206`
- ? Android emulator configured and running
- ? Test driver credentials available

---

## Test Credentials

Based on the API summary, we need a driver account with:
- **Username**: `driver001` (or as configured in AuthServer)
- **Password**: (your test password)
- **Expected JWT Claims**:
  - `role`: `driver`
  - `uid`: `driver-001`

---

## Test Scenarios

### **1. Authentication Flow** ?

#### Test Case 1.1: Successful Login
**Steps:**
1. Launch the app
2. Enter valid driver username and password
3. Tap "Sign In"

**Expected Result:**
- Loading indicator appears
- Successfully navigates to "Today's Rides" page
- JWT token stored in SecureStorage

#### Test Case 1.2: Invalid Credentials
**Steps:**
1. Enter incorrect username or password
2. Tap "Sign In"

**Expected Result:**
- Error message: "Invalid username or password"
- Remains on login page

#### Test Case 1.3: Empty Fields
**Steps:**
1. Leave username or password empty
2. Tap "Sign In"

**Expected Result:**
- Error message: "Please enter your username" or "Please enter your password"
- No API call made

---

### **2. Ride List (Home Page)** ?

#### Test Case 2.1: Load Today's Rides
**Prerequisite:** Seed test data via AdminAPI:
```bash
POST https://localhost:5206/bookings/seed
Authorization: Bearer {admin-jwt}
```

**Steps:**
1. Login successfully
2. Observe rides list on home page

**Expected Result:**
- Shows all rides assigned to `driver-001`
- Each ride card displays:
  - Pickup time (formatted)
  - Passenger name
  - Pickup location
  - Dropoff location
  - Status badge

#### Test Case 2.2: Empty Ride List
**Steps:**
1. Login when no rides are assigned
2. Observe home page

**Expected Result:**
- Empty state message: "No rides scheduled for today"
- "Pull down to refresh" hint

#### Test Case 2.3: Pull to Refresh
**Steps:**
1. Pull down on rides list
2. Observe refresh indicator

**Expected Result:**
- Refresh indicator shows
- API call to `/driver/rides/today`
- Rides list updates

#### Test Case 2.4: Sign Out
**Steps:**
1. Tap toolbar "Sign Out" button
2. Confirm sign out in dialog

**Expected Result:**
- Confirmation dialog appears
- On confirm: navigates to login page
- Token cleared from SecureStorage
- Location tracking stopped

---

### **3. Ride Details** ?

#### Test Case 3.1: View Ride Details
**Steps:**
1. Tap a ride from the list
2. Observe ride detail page

**Expected Result:**
- Navigation to ride detail page
- Shows complete ride information:
  - Passenger name and phone
  - Pickup/dropoff locations
  - Pickup style and sign text (if applicable)
  - Flight info (if airport pickup)
  - Passenger count, bags
  - Vehicle class
  - Additional requests
  - Current status

#### Test Case 3.2: Call Passenger
**Steps:**
1. On ride detail page, tap "Call Passenger"

**Expected Result:**
- Phone dialer opens with passenger's number pre-filled

#### Test Case 3.3: Navigate to Pickup
**Steps:**
1. Tap "Navigate to Pick Up" button

**Expected Result:**
- Native maps app opens (Google Maps on Android)
- Shows route to pickup location

#### Test Case 3.4: Navigate to Dropoff
**Steps:**
1. Tap "Navigate to Drop Off" button

**Expected Result:**
- Native maps app opens
- Shows route to dropoff location

---

### **4. Ride Status Updates** ?

#### Test Case 4.1: Start Trip (Scheduled ? OnRoute)
**Prerequisite:** Ride with status `Scheduled`

**Steps:**
1. Open ride detail page
2. Tap "Start Trip (On Route)" button
3. Confirm in dialog

**Expected Result:**
- Confirmation dialog appears
- Status updates to "OnRoute"
- Success message shown
- Location tracking starts automatically
- Green "Location tracking active" indicator appears

#### Test Case 4.2: Mark Arrived (OnRoute ? Arrived)
**Steps:**
1. With ride status "OnRoute"
2. Tap "Mark Arrived" button
3. Confirm

**Expected Result:**
- Status updates to "Arrived"
- Location tracking continues

#### Test Case 4.3: Passenger Onboard (Arrived ? PassengerOnboard)
**Steps:**
1. With ride status "Arrived"
2. Tap "Passenger Onboard" button
3. Confirm

**Expected Result:**
- Status updates to "PassengerOnboard"
- Location tracking continues

#### Test Case 4.4: Complete Ride (PassengerOnboard ? Completed)
**Steps:**
1. With ride status "PassengerOnboard"
2. Tap "Complete Ride" button
3. Confirm

**Expected Result:**
- Status updates to "Completed"
- Location tracking stops
- Green indicator disappears
- No more status buttons visible

#### Test Case 4.5: Cancel Ride
**Steps:**
1. From any non-terminal status
2. Tap "Cancel Ride" button
3. Confirm

**Expected Result:**
- Status updates to "Cancelled"
- Location tracking stops (if active)

#### Test Case 4.6: Invalid Status Transition
**Expected Behavior:**
- Only valid transition buttons are visible
- Cannot skip statuses (e.g., Scheduled ? Completed)

---

### **5. Location Tracking** ??

#### Test Case 5.1: Location Permission Request
**Steps:**
1. Start a trip (Scheduled ? OnRoute)
2. Observe permission dialog

**Expected Result:**
- Android permission dialog appears
- Message explains why location is needed
- Can grant or deny permission

#### Test Case 5.2: Location Permission Denied
**Steps:**
1. Deny location permission
2. Try to start trip

**Expected Result:**
- Alert: "Unable to start location tracking. Please check location permissions."
- Location tracking indicator does NOT appear
- Ride status still updates

#### Test Case 5.3: Location Updates During Active Ride
**Prerequisite:** Location permission granted

**Steps:**
1. Start trip (OnRoute)
2. Wait 30 seconds
3. Check AdminAPI logs or location endpoint

**Expected Result:**
- Location updates sent every 30 seconds
- API receives updates at `/driver/location/update`
- No error messages

#### Test Case 5.4: Location Tracking Stops on Completion
**Steps:**
1. With location tracking active
2. Complete the ride

**Expected Result:**
- Location tracking stops immediately
- No more location updates sent
- Green indicator disappears

#### Test Case 5.5: Location Tracking Persists Across Status Changes
**Steps:**
1. Start trip (OnRoute) - tracking starts
2. Mark Arrived - tracking continues
3. Passenger Onboard - tracking continues
4. Complete - tracking stops

**Expected Result:**
- Tracking active for OnRoute, Arrived, PassengerOnboard
- Stops only on Completed or Cancelled

---

### **6. Network Error Handling** ??

#### Test Case 6.1: No Network on Login
**Steps:**
1. Disable WiFi/data
2. Try to login

**Expected Result:**
- Error message: "Network error: [details]"
- Remains on login page

#### Test Case 6.2: No Network on Ride List Refresh
**Steps:**
1. On home page, disable network
2. Pull to refresh

**Expected Result:**
- Shows previous cached rides (if any)
- Error message displayed
- Refresh indicator stops

#### Test Case 6.3: Network Error During Status Update
**Steps:**
1. Disable network
2. Try to update ride status

**Expected Result:**
- Error message: "Network error: [details]"
- Status does not change locally
- User can retry when network returns

#### Test Case 6.4: 401 Unauthorized (Token Expired)
**Simulation:** Wait for token to expire or delete token

**Expected Result:**
- Automatic sign out
- Redirect to login page
- Message: "Session expired, please login again"

---

### **7. Edge Cases** ??

#### Test Case 7.1: Ride Not Found
**Steps:**
1. Navigate to ride detail with invalid ID
2. Observe error handling

**Expected Result:**
- Error message: "Ride not found"
- Option to go back

#### Test Case 7.2: App Backgrounding During Location Tracking
**Steps:**
1. Start location tracking
2. Background the app
3. Wait 1+ minute
4. Foreground the app

**Expected Result:**
- Location tracking resumes (or stops gracefully)
- No crashes

#### Test Case 7.3: Multiple Rapid Status Updates
**Steps:**
1. Rapidly tap status update button multiple times

**Expected Result:**
- First tap processes
- Subsequent taps ignored while `IsBusy=true`
- No duplicate API calls

---

## ?? Testing Tools

### Check Token in SecureStorage
```csharp
// In App.xaml.cs OnStart, add logging:
var token = await SecureStorage.GetAsync("bellwood_access_token");
Console.WriteLine($"Stored token: {token?.Substring(0, 20)}...");
```

### Monitor API Calls
- Use Charles Proxy or Fiddler to inspect HTTP traffic
- Check Visual Studio Output window for console logs

### Verify Location Updates (AdminAPI)
```bash
# Get latest location for a ride
GET https://localhost:5206/driver/location/{rideId}
Authorization: Bearer {driver-jwt}
```

---

## ?? Manual Testing Workflow

### Full Happy Path Test:
1. ? Launch app ? Login page appears
2. ? Login with driver credentials ? Navigate to home page
3. ? See today's rides (seed data if needed)
4. ? Tap a ride ? View details
5. ? Tap "Call Passenger" ? Phone dialer opens
6. ? Tap "Navigate to Pick Up" ? Maps open
7. ? Tap "Start Trip" ? Status updates, tracking starts
8. ? Wait 30s ? Check location update sent
9. ? Tap "Mark Arrived" ? Status updates, tracking continues
10. ? Tap "Passenger Onboard" ? Status updates
11. ? Tap "Complete Ride" ? Status updates, tracking stops
12. ? Return to home ? Completed ride no longer in list
13. ? Sign out ? Return to login page

---

## ?? Known Issues / Limitations

1. **Location tracking in emulator**: Android emulator GPS may need manual configuration
   - Use Extended Controls ? Location to set coordinates
   - Or use real device for accurate GPS testing

2. **Self-signed certificates**: Development uses `DangerousAcceptAnyServerCertificateValidator`
   - Production will require valid SSL certificates

3. **No offline queue**: Location updates lost if network unavailable
   - Phase 2 feature

4. **UTC times displayed**: No timezone conversion yet
   - Phase 2 feature

---

## ? Test Sign-off

### Test Environment:
- [ ] Android Emulator (API level: ____)
- [ ] Physical Android Device (model: ____)
- [ ] iOS Simulator (if applicable)

### Test Results:
- [ ] All authentication flows working
- [ ] Ride list displays correctly
- [ ] Ride details complete
- [ ] Status updates working
- [ ] Location tracking functional
- [ ] Navigation to maps working
- [ ] Error handling appropriate

### Issues Found:
(List any bugs or unexpected behaviors)

---

## ?? Ready for Testing!

**Next Steps:**
1. Deploy app to Android emulator
2. Run through manual test scenarios
3. Document any issues found
4. Iterate and fix

**Test Credentials Needed:**
- Driver username: ____________
- Driver password: ____________
- Expected UID: `driver-001`
