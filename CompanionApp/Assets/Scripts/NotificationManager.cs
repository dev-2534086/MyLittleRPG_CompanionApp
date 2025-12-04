using System.Collections;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif

public class NotificationManager : MonoBehaviour
{
    private const string CHANNEL_ID = "rpg_quests_channel";

    void Awake()
    {
        InitializeNotifications();
    }

    void InitializeNotifications()
    {
#if UNITY_ANDROID
        var channel = new AndroidNotificationChannel()
        {
            Id = CHANNEL_ID,
            Name = "Quêtes RPG",
            Importance = Importance.High,
            Description = "Notifications pour les nouvelles quêtes et quêtes accomplies",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif

#if UNITY_IOS
        StartCoroutine(RequestIOSAuthorization());
#endif
    }

#if UNITY_IOS
    IEnumerator RequestIOSAuthorization()
    {
        var authOption = AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound;
        using (var req = new AuthorizationRequest(authOption, true))
        {
            while (!req.IsFinished)
                yield return null;

            Debug.Log($"iOS Notification Authorization - Granted: {req.Granted}, Error: {req.Error}");
        }
    }
#endif

    public void SendQuestNotification(string title, string text)
    {
        if (Application.isFocused)
        {
            Debug.Log($"App au premier plan, notification ignorée: {title}");
            return;
        }

#if UNITY_ANDROID
        SendAndroidNotification(title, text);
#elif UNITY_IOS
        SendIOSNotification(title, text);
#else
        Debug.Log($"Notification non supportée: {title} - {text}");
#endif
    }

#if UNITY_ANDROID
    void SendAndroidNotification(string title, string text)
    {
        var notification = new AndroidNotification
        {
            Title = title,
            Text = text,
            SmallIcon = "icon_small",
            LargeIcon = "icon_large",
            FireTime = System.DateTime.Now.AddSeconds(1)
        };

        AndroidNotificationCenter.SendNotification(notification, CHANNEL_ID);
        Debug.Log($"Notification Android envoyée: {title}");
    }
#endif

#if UNITY_IOS
    void SendIOSNotification(string title, string text)
    {
        var timeTrigger = new iOSNotificationTimeIntervalTrigger
        {
            TimeInterval = new System.TimeSpan(0, 0, 1),
            Repeats = false
        };

        var notification = new iOSNotification
        {
            Identifier = "_notification_" + System.DateTime.Now.Ticks,
            Title = title,
            Body = text,
            ShowInForeground = false,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            Trigger = timeTrigger
        };

        iOSNotificationCenter.ScheduleNotification(notification);
        Debug.Log($"Notification iOS programmée: {title}");
    }
#endif

    public void ClearAllNotifications()
    {
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelAllNotifications();
#elif UNITY_IOS
        iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif
    }
}
