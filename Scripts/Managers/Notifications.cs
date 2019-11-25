using System;
using Unity.Notifications.Android;

public class Notification
{
    public AndroidNotificationChannel AndroidChannel { get; private set; }

    public void CreateNotificationChannel(string id, string title, string description, Importance importance)
    {
        AndroidChannel = new AndroidNotificationChannel(id, title, description, importance)
        {
            Name = title,
            EnableLights = true,
            EnableVibration = true,
            LockScreenVisibility = LockScreenVisibility.Public
        };
    }

    public AndroidNotification CreateAndroidNotification(string title, string text, DateTime fareTime)
    {
        return new AndroidNotification(title, text, fareTime);
    }

    public void SendNotification(AndroidNotification androidNotification, string channel)
    {
        if (GameDataManager.data.enableNotificaions)
            AndroidNotificationCenter.SendNotification(androidNotification, channel);
    }

    public static void CancelNotification(int id)
    {
        AndroidNotificationCenter.CancelNotification(id);
    }

    public static void CancelAllNotifications()
    {
        AndroidNotificationCenter.CancelAllNotifications();
    }
}
