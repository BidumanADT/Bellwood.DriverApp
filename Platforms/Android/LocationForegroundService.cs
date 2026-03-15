using Android.App;
using Android.Content;
using Android.OS;

namespace Bellwood.DriverApp;

[Service(Exported = false, ForegroundServiceType = ForegroundService.TypeLocation)]
public class LocationForegroundService : Service
{
    public const string ActionStartTracking = "com.bellwoodglobal.driver.action.START_TRACKING";
    public const string ActionStopTracking = "com.bellwoodglobal.driver.action.STOP_TRACKING";
    public const string ExtraRideId = "ride_id";

    private const string NotificationChannelId = "driver_location_tracking";
    private const int NotificationId = 1001;

    private string? _currentRideId;

    public static Intent CreateStartIntent(Context context, string rideId)
    {
        var intent = new Intent(context, typeof(LocationForegroundService));
        intent.SetAction(ActionStartTracking);
        intent.PutExtra(ExtraRideId, rideId);
        return intent;
    }

    public static Intent CreateStopIntent(Context context)
    {
        var intent = new Intent(context, typeof(LocationForegroundService));
        intent.SetAction(ActionStopTracking);
        return intent;
    }

    public override void OnCreate()
    {
        base.OnCreate();
        EnsureNotificationChannel();
    }

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        StartForeground(NotificationId, BuildNotification("Location tracking is active."));

        var action = intent?.Action;
        if (action == ActionStartTracking)
        {
            _currentRideId = intent?.GetStringExtra(ExtraRideId);

            // TODO: Call into existing LocationTracker start logic here, passing _currentRideId.
            UpdateForegroundNotification($"Tracking ride {_currentRideId ?? "(unknown)"}.");
        }
        else if (action == ActionStopTracking)
        {
            // TODO: Call into existing LocationTracker stop logic here.
            _currentRideId = null;
            StopForeground(StopForegroundFlags.Remove);
            StopSelf();
        }

        return StartCommandResult.Sticky;
    }

    public override IBinder? OnBind(Intent? intent)
    {
        return null;
    }

    private void EnsureNotificationChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
        {
            return;
        }

        var channel = new NotificationChannel(
            NotificationChannelId,
            "Location Tracking",
            NotificationImportance.Low)
        {
            Description = "Keeps ride location tracking active while a trip is in progress."
        };

        var manager = (NotificationManager?)GetSystemService(NotificationService);
        manager?.CreateNotificationChannel(channel);
    }

    private Notification BuildNotification(string contentText)
    {
        var launchIntent = PackageManager?.GetLaunchIntentForPackage(PackageName)
            ?? new Intent(this, typeof(MainActivity));
        launchIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

        var pendingIntentFlags = PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent;
        var pendingIntent = PendingIntent.GetActivity(this, 0, launchIntent, pendingIntentFlags);

        return new Notification.Builder(this, NotificationChannelId)
            .SetContentTitle("Bellwood Driver")
            .SetContentText(contentText)
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetOngoing(true)
            .SetOnlyAlertOnce(true)
            .SetContentIntent(pendingIntent)
            .Build();
    }

    private void UpdateForegroundNotification(string contentText)
    {
        var manager = (NotificationManager?)GetSystemService(NotificationService);
        manager?.Notify(NotificationId, BuildNotification(contentText));
    }
}
