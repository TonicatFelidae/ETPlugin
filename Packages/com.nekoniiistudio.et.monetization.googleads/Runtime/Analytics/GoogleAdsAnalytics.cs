using System;
using System.Collections.Generic;

namespace ET.Analytics
{
    /// <summary>
    /// Decoupled ad analytics dispatcher for Google Mobile Ads.
    /// External analytics SDKs (Firebase, Adjust, GameAnalytics, etc.) hook into OnLogEvent
    /// to observe and transmit standardized ad events.
    /// </summary>
    public static class GoogleAdsAnalytics
    {
        public enum Format
        {
            Banner,
            Interstitial,
            Rewarded,
            RewardedInterstitial,
            AppOpen
        }

        /// <summary>
        /// Hook for listening to ad telemetry events: (eventName, eventParameters).
        /// </summary>
        public static Action<string, Dictionary<string, object>> OnLogEvent;

        public static void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
            OnLogEvent?.Invoke(eventName, parameters);
        }

        public static string FormatToString(Format format)
        {
            return format switch
            {
                Format.Banner => "banner",
                Format.Interstitial => "interstitial",
                Format.Rewarded => "rewarded",
                Format.RewardedInterstitial => "rewarded_interstitial",
                Format.AppOpen => "app_open",
                _ => "unknown"
            };
        }
    }
}
