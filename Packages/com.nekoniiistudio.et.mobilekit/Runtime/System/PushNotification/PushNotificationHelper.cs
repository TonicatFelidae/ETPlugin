/*
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Ser.RemoteConfig;
using Zenject;
using ET;

#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif
namespace ET.System
{

    public class PushNotificationHelper
    {

        private const int D7InactivityHours = 24 * 7;
        public const string DefaultD7ReminderMessage = "🎁今すぐ200ポイントGET！";
        private const string D7NotificationIntentData = "D7_NOTIFICATION";
#if UNITY_IOS
    private const string D7NotificationUserInfoKey = "d7_notification";
#endif

        public void ScheduleLocalPush(bool includeD7Reminder)
        {
            Debug.Log("Start schedule notification!");
#if UNITY_ANDROID || UNITY_IOS
            int d7Ab = RemoteConfigService.Instance.appConfig.GetInt("AB_d7incentive_reward_push", 0);
            bool enableD7PushByAb = d7Ab == 1;
            bool shouldScheduleD7 = enableD7PushByAb && includeD7Reminder;
#endif
#if UNITY_ANDROID
            var channel = new AndroidNotificationChannel()
            {
                Id = "findthecat_notification",
                Name = "Find The Cat Notifications",
                Importance = Importance.Default,
                Description = "Re-engagement notifications for Find The Cat",
            };
            AndroidNotificationCenter.CancelAllScheduledNotifications();
            AndroidNotificationCenter.RegisterNotificationChannel(channel);

            var scheduledNotifications = new List<AndroidNotification>();
            for (int i = 0; i < Data.notificationItems.Length; i++)
            {
                var noffItem = Data.notificationItems[i];
                if (shouldScheduleD7 && IsD7NotificationItem(noffItem))
                {
                    continue;
                }
                var message = GetMessage(noffItem.messeger, noffItem.timeInHour);
                var notification = ScheduleAndroidNotification(message, noffItem.timeInHour);
                scheduledNotifications.Add(notification);
            }

            if (shouldScheduleD7)
            {
                var message = DefaultD7ReminderMessage;
                var notification = ScheduleAndroidNotification(message, D7InactivityHours, D7NotificationIntentData);
                scheduledNotifications.Add(notification);
            }

            LogReservedNotifications(scheduledNotifications);
#elif UNITY_IOS
        iOSNotificationCenter.RemoveAllScheduledNotifications();
        iOSNotificationCenter.RemoveAllDeliveredNotifications();
        iOSNotificationCenter.ApplicationBadge = 0;
        var scheduledNotifications = new List<iOSNotification>();
        for (int i = 0; i < Data.notificationItems.Length; i++)
        {
            var noffItem = Data.notificationItems[i];
            if (shouldScheduleD7 && IsD7NotificationItem(noffItem))
            {
                continue;
            }
            var message = GetMessage(noffItem.messeger, noffItem.timeInHour);
            var notification = ScheduleiOSNotification(message, noffItem.timeInHour);
            scheduledNotifications.Add(notification);
        }
        if (shouldScheduleD7)
        {
            var message = DefaultD7ReminderMessage;
            var userInfo = new Dictionary<string, string>
            {
                { D7NotificationUserInfoKey, "1" }
            };
            var notification = ScheduleiOSNotification(message, D7InactivityHours, userInfo);
            scheduledNotifications.Add(notification);
        }
        LogReservedNotifications(scheduledNotifications);
#else
            _ = includeD7Reminder;
#endif
        }

        public void ScheduleDebugD7Reminder(float delaySeconds = 30f)
        {
            var seconds = Mathf.Max(1f, delaySeconds);
#if UNITY_ANDROID
            var channel = new AndroidNotificationChannel()
            {
                Id = "findthecat_notification",
                Name = "Find The Cat Notifications",
                Importance = Importance.Default,
                Description = "Re-engagement notifications for Find The Cat",
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);

            var notification = new AndroidNotification
            {
                Title = "にゃんこをさがそう！",
                Text = DefaultD7ReminderMessage,
                SmallIcon = "icon_small",
                LargeIcon = "icon_large",
                FireTime = DateTime.Now.AddSeconds(seconds)
            };
            notification.IntentData = D7NotificationIntentData;
            AndroidNotificationCenter.SendNotification(notification, "findthecat_notification");
#elif UNITY_IOS
        var timeTrigger = new iOSNotificationTimeIntervalTrigger
        {
            TimeInterval = TimeSpan.FromSeconds(seconds),
            Repeats = false
        };

        var userInfo = new Dictionary<string, string>
        {
            { D7NotificationUserInfoKey, "1" }
        };
        var notification = CreateiOSNotificationWithUserInfo(
            $"findthecat_debug_notify_{DateTime.Now.Ticks}",
            "にゃんこをさがせ",
            DefaultD7ReminderMessage,
            timeTrigger,
            userInfo
        );

        iOSNotificationCenter.ScheduleNotification(notification);
#else
            Debug.Log($"ScheduleDebugD7Reminder invoked with {seconds}s but notifications unsupported on this platform.");
#endif
        }

        private string GetMessage(string defaultMessage, int hoursFromNow)
        {
            if (_gameDat.dailyRewardDats == null)
            {
                return defaultMessage;
            }

            var jstNow = DateTime.UtcNow.AddHours(9);
            var jstTarget = jstNow.AddHours(hoursFromNow);
            var dayDiff = (jstTarget.Date - jstNow.Date).Days;

            MissionState targetState;
            if (dayDiff == 0 || dayDiff == 1)
            {
                targetState = MissionState.Locked;
            }
            else
            {
                return defaultMessage;
            }

            for (int i = 0; i < _gameDat.dailyRewardDats.Count; i++)
            {
                var dat = _gameDat.dailyRewardDats[i];
                if (dat.state == targetState)
                {
                    if (dat.rewardItemKind == 0)
                    {
                        return $"今日ログインしないと{dat.rewardAmount}ポイントを失います";
                    }
                    else
                    {
                        return $"今日ログインしないと{dat.rewardAmount}枚のチケットを失います";
                    }
                }
            }

            return defaultMessage;
        }

        private static bool IsD7NotificationItem(PostGameNotificationItem item)
        {
            return item.timeInHour == D7InactivityHours;
        }

        public bool TryConsumeD7NotificationActivation()
        {
#if UNITY_ANDROID
            var intentData = AndroidNotificationCenter.GetLastNotificationIntent();
            if (intentData != null)
            {
                var notification = intentData.Notification;
                if (notification.IntentData == D7NotificationIntentData)
                {
                    AndroidNotificationCenter.CancelNotification(intentData.Id);
                    return true;
                }
            }
#elif UNITY_IOS
        var lastNotification = iOSNotificationCenter.GetLastRespondedNotification();
        if (lastNotification != null && lastNotification.UserInfo != null &&
            lastNotification.UserInfo.TryGetValue(D7NotificationUserInfoKey, out var value) && value == "1")
        {
            iOSNotificationCenter.RemoveDeliveredNotification(lastNotification.Identifier);
            iOSNotificationCenter.RemoveScheduledNotification(lastNotification.Identifier);
            return true;
        }
#endif
            return false;
        }


#if UNITY_ANDROID
        private void LogReservedNotifications(List<AndroidNotification> notifications)
        {
            foreach (var n in notifications)
            {
                Debug.Log($"Reserved notification: {n.Title} - {n.Text} at {n.FireTime}");
            }
        }
#elif UNITY_IOS
    private void LogReservedNotifications(List<iOSNotification> notifications)
    {
        foreach (var n in notifications)
        {
            Debug.Log($"Reserved notification: {n.Identifier} - {n.Body}");
        }
    }
#endif

#if UNITY_ANDROID
        static AndroidNotification ScheduleAndroidNotification(string message, int hoursFromNow, string intentData = null)
        {
            Debug.Log("ScheduleAndroidNotification: " + message);
            var notification = new AndroidNotification
            {
                Title = "にゃんこをさがそう！",
                Text = message,
                SmallIcon = "icon_small",
                LargeIcon = "icon_large",
                FireTime = DateTime.Now.AddHours(hoursFromNow)
            };
            if (!string.IsNullOrEmpty(intentData))
            {
                notification.IntentData = intentData;
            }
            AndroidNotificationCenter.SendNotification(notification, "findthecat_notification");
            return notification;
        }
#endif

#if UNITY_IOS
    static iOSNotification CreateiOSNotificationWithUserInfo(
        string identifier,
        string title,
        string body,
        iOSNotificationTimeIntervalTrigger trigger,
        Dictionary<string, string> userInfo)
    {
        var notification = new iOSNotification
        {
            Identifier = identifier,
            Title = title,
            Body = body,
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = "findthecat_reminder",
            ThreadIdentifier = "findthecat_reminder_thread",
            Trigger = trigger
        };

        // Work around read-only UserInfo by using reflection to set the property
        if (userInfo != null && userInfo.Count > 0)
        {
            var userInfoProperty = typeof(iOSNotification).GetProperty("UserInfo");
            if (userInfoProperty != null && userInfoProperty.CanWrite)
            {
                userInfoProperty.SetValue(notification, userInfo);
            }
        }

        return notification;
    }

    static iOSNotification ScheduleiOSNotification(string message, int hoursFromNow, Dictionary<string, string> userInfo = null)
    {
        var timeTrigger = new iOSNotificationTimeIntervalTrigger
        {
            TimeInterval = new TimeSpan(hoursFromNow, 0, 0),
            Repeats = false
        };

        var notification = userInfo != null
            ? CreateiOSNotificationWithUserInfo(
                $"findthecat_notify_{hoursFromNow}",
                "にゃんこをさがせ",
                message,
                timeTrigger,
                userInfo
            )
            : new iOSNotification
            {
                Identifier = $"findthecat_notify_{hoursFromNow}",
                Title = "にゃんこをさがせ",
                Body = message,
                ShowInForeground = true,
                ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
                CategoryIdentifier = "findthecat_reminder",
                ThreadIdentifier = "findthecat_reminder_thread",
                Trigger = timeTrigger
            };

        iOSNotificationCenter.ScheduleNotification(notification);
        return notification;
    }
#endif
    }
}
*/