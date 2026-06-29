using System;
using UnityEngine;
using UnityEngine.Android;
namespace ETMobileKit
{
    public class AndroidHelper
    {
        public Action<string> OnNotificationPermissionGranted;
        public Action<string> OnNotificationPermissionDenied;
        const string NotificationPermission = "android.permission.POST_NOTIFICATIONS";
        public void RequestNotificationPermission()
        {
            PermissionCallbacks callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += OnNotificationPermissionGranted;
            callbacks.PermissionDenied += OnNotificationPermissionDenied;
#if UNITY_ANDROID
            if (Permission.HasUserAuthorizedPermission(NotificationPermission))
            {
                Debug.Log("Notification permission already granted.");
            }
            else
            {
                Permission.RequestUserPermission(NotificationPermission, callbacks);
            }
#endif
        }
        public bool IsGotNotificationPermission()
        {
            return Permission.HasUserAuthorizedPermission(NotificationPermission);
        }
    }
}