using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ET.NotificationSystem
{
    [CreateAssetMenu(fileName = "PushNotificationData", menuName = "ET/PushNotificationData", order = 0)]
    public class PushNotificationData : ScriptableObject
    {
        public PostGameNotificationItem[] notificationItems;
        public int D7InactivityHours = 168;
        public int D14InactivityHours = 336;
        public int D30InactivityHours = 720;

        public string title;
        public string message;
        public int timeInHour;
        public string defaultD7ReminderMessage = "🎁今すぐ200ポイントGET！";

        public PushNotificationData(string title, string message, int timeInHour)
        {
            this.title = title;
            this.message = message;
            this.timeInHour = timeInHour;
        }

        public PushNotificationData GetDefaultPushNotificationData()
            => new PushNotificationData("We miss you!", "Come back and play the game!", D7InactivityHours);
    }
}