using System;
using System.Collections.Generic;

namespace ET.Analytics
{
    /// <summary>
    /// Decoupled In-App Purchasing analytics dispatcher.
    /// External analytics SDKs (Firebase, Adjust, GameAnalytics, etc.) hook into OnLogEvent
    /// to observe and transmit standardized IAP funnel and revenue events.
    /// </summary>
    public static class IAPAnalytics
    {
        /// <summary>
        /// Hook for listening to IAP telemetry events: (eventName, eventParameters).
        /// </summary>
        public static Action<string, Dictionary<string, object>> OnLogEvent;

        public static void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
            OnLogEvent?.Invoke(eventName, parameters);
        }
    }
}
