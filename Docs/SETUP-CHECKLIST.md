# Pre-Testing Setup Checklist

## ? Backend Services

### 1. AuthServer Status
- [ ] AuthServer running on `https://localhost:5001`
- [ ] Can access login endpoint: `POST /api/auth/login`
- [ ] Test driver account exists with:
  - [ ] Username: charlie
  - [ ] Password: password
  - [ ] Role: `driver`
  - [ ] UID: `driver-001`

### 2. AdminAPI Status
- [ ] AdminAPI running on `https://localhost:5206`
- [ ] Swagger available at `https://localhost:5206/swagger`
- [ ] Driver endpoints accessible:
  - [ ] `GET /driver/rides/today`
  - [ ] `GET /driver/rides/{id}`
  - [ ] `POST /driver/rides/{id}/status`
  - [ ] `POST /driver/location/update`

### 3. Test Data
- [ ] Seeded test bookings via `POST /bookings/seed`
- [ ] Bookings have `AssignedDriverUid = "driver-001"`
- [ ] Bookings have `CurrentRideStatus = Scheduled`
- [ ] At least one booking has pickup time within next 24 hours

---

## ? Android Emulator

### 1. Emulator Configuration
- [ ] Android emulator created and running
- [ ] API Level: 21 or higher (Android 5.0+)
- [ ] Google Play Services installed (for maps)
- [ ] Internet connectivity working

### 2. Emulator Location Settings
- [ ] GPS enabled in emulator
- [ ] Location set via Extended Controls ? Location
- [ ] Test coordinates: Latitude/Longitude configured

### 3. Network Accessibility
- [ ] Emulator can reach `10.0.2.2:5001` (AuthServer)
- [ ] Emulator can reach `10.0.2.2:5206` (AdminAPI)
- [ ] Test with browser in emulator: `http://10.0.2.2:5206/swagger`

---

## ? Mobile App

### 1. Build & Deploy
- [ ] Solution restored: `dotnet restore`
- [ ] Build successful: `dotnet build`
- [ ] App deployed to emulator
- [ ] App launches without crashes

### 2. Initial App State
- [ ] Login page displays
- [ ] No previous tokens stored (fresh install)
- [ ] UI elements render correctly
- [ ] Keyboard appears when tapping input fields

---

## ?? Quick Smoke Test

### Test 1: Login
1. [ ] Enter driver credentials
2. [ ] Tap "Sign In"
3. [ ] Loading indicator appears
4. [ ] Successfully navigates to "Today's Rides" page

### Test 2: Ride List
1. [ ] Rides appear in list
2. [ ] Each ride shows pickup time, passenger, locations
3. [ ] Pull-to-refresh works
4. [ ] Can tap a ride to view details

### Test 3: Ride Details
1. [ ] Ride detail page loads
2. [ ] All information displays correctly
3. [ ] "Call Passenger" button present
4. [ ] "Navigate" buttons present
5. [ ] Status update buttons visible

### Test 4: Status Update
1. [ ] Tap "Start Trip (On Route)"
2. [ ] Confirmation dialog appears
3. [ ] Status updates successfully
4. [ ] Location tracking indicator appears (green badge)

### Test 5: Location Tracking
1. [ ] Location permission dialog appears
2. [ ] Grant permission
3. [ ] Wait 30 seconds
4. [ ] Check AdminAPI logs for location update
5. [ ] Verify location received at `/driver/location/update`

---

## ?? Common Issues & Solutions

### "Unable to connect to server"
**Solution:**
- Verify services are running on correct ports
- Check Windows Firewall allows emulator connections
- Test `http://10.0.2.2:5206/swagger` in emulator browser

### "Invalid credentials"
**Solution:**
- Verify driver account exists in AuthServer database
- Check username/password are correct
- Ensure account has `role=driver` claim

### "No rides appear"
**Solution:**
- Run seed endpoint: `POST /bookings/seed`
- Verify bookings have `AssignedDriverUid = "driver-001"`
- Check pickup times are within next 24 hours
- Pull-to-refresh on home page

### "Location permission denied"
**Solution:**
- Go to Android Settings ? Apps ? Bellwood Driver ? Permissions
- Enable Location permission
- Restart app and try again

### "Location tracking not working"
**Solution:**
- Enable GPS in emulator settings
- Set location in Extended Controls ? Location
- Check AdminAPI logs for errors (429 = too frequent)
- Verify ride status is OnRoute/Arrived/PassengerOnboard

---

## ?? Test Data Verification

### Verify JWT Claims (use jwt.io)
After login, check SecureStorage token includes:
```json
{
  "sub": "auth0|12345",
  "uid": "driver-001",
  "role": "driver",
  "exp": 1234567890
}
```

### Verify Seeded Booking
Example booking structure:
```json
{
  "id": "abc123",
  "assignedDriverUid": "driver-001",
  "currentRideStatus": "Scheduled",
  "pickupDateTime": "2024-11-28T14:30:00Z",
  "passengerName": "Test Passenger",
  "pickupLocation": "O'Hare Airport",
  "dropoffLocation": "Downtown Chicago"
}
```

---

## ? Ready to Test!

Once all checkboxes are complete, proceed to [TESTING-GUIDE.md](TESTING-GUIDE.md) for comprehensive test scenarios.

**Questions? Issues?** Check the troubleshooting section or contact the development team.
