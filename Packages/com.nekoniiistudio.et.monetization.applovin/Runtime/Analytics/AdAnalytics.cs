using System;
using System.Text;
using UnityEngine;

namespace ET.Analytics
{
    /// <summary>
    /// The ad tracking plan, in code. One method per event in
    /// <c>docs/analytics/spec/events.md</c>; parameter names and the rules about
    /// which parameter may appear on which event come from
    /// <c>docs/analytics/spec/parameters.md</c>.
    ///
    /// Two funnels are measured, and they are deliberately NOT joined:
    ///
    ///   A (opportunity)  show_request -> show_start -> show_success -> reward_earned
    ///   B (supply)       load_start   -> load_success | load_failed
    ///
    /// Their denominators are unrelated — a preload runs with no player intent
    /// behind it, and a show can consume an ad loaded in a previous session — so any
    /// conversion rate computed across the two is meaningless. The one real link is
    /// <c>ad_show_request</c> with <c>ad_ready = false</c>: that is funnel B failing
    /// funnel A, and it is the headline number of the whole dashboard.
    ///
    /// Three constraints are load-bearing; breaking them silently produces wrong
    /// numbers rather than errors:
    ///
    ///   1. <c>ad_load_failed</c> carries no <c>ad_network</c>. MAX's load-failure
    ///      callback has no AdInfo, and a failed load means *no* network filled —
    ///      there is nothing to name. Use ad_error_code instead.
    ///   2. <c>ad_retry_attempt</c> is mandatory on every load event. The exponential
    ///      backoff fires up to 7 loads for a single opportunity, so fill rate must
    ///      be computed as load_success / load_start FILTERED to attempt 0.
    ///   3. <c>ad_placement</c> never appears on a load event. MAX preloads one ad
    ///      unit and shares it across placements, so a load cannot be attributed to
    ///      one. Placement exists from ad_show_request onward.
    /// </summary>
    public static class AdAnalytics
    {
        /// <summary>Values of the <c>ad_format</c> parameter.</summary>
        public static class Format
        {
            public const string Interstitial = "interstitial";
            public const string Rewarded = "rewarded";
            public const string Banner = "banner";
            public const string MRec = "mrec";
        }

        /// <summary>Values of <c>ad_not_ready_reason</c>, set only when ad_ready is false.</summary>
        public static class NotReadyReason
        {
            public const string NoFill = "no_fill";
            public const string Loading = "loading";
            public const string Cooldown = "cooldown";
            public const string CapReached = "cap_reached";
            public const string SdkNotInit = "sdk_not_init";
        }

        private static class Names
        {
            public const string LoadStart = "ad_load_start";
            public const string LoadSuccess = "ad_load_success";
            public const string LoadFailed = "ad_load_failed";
            public const string ShowRequest = "ad_show_request";
            public const string ShowStart = "ad_show_start";
            public const string ShowSuccess = "ad_show_success";
            public const string ShowFailed = "ad_show_failed";
            public const string RewardEarned = "ad_reward_earned";
            public const string Dismissed = "ad_dismissed";
            public const string Clicked = "ad_clicked";
            public const string Impression = "ad_impression";
            public const string DisplayStart = "ad_display_start";
            public const string DisplayEnd = "ad_display_end";
        }

        private static class P
        {
            public const string UnitId = "ad_unit_id";
            public const string Format = "ad_format";
            public const string Placement = "ad_placement";
            public const string Network = "ad_network";
            public const string Ready = "ad_ready";
            public const string NotReadyReason = "ad_not_ready_reason";
            public const string RetryAttempt = "ad_retry_attempt";
            public const string ErrorCode = "ad_error_code";
            public const string ErrorMessage = "ad_error_message";
            public const string Rewarded = "ad_rewarded";
            public const string RequestId = "ad_request_id";
            public const string LatencyMs = "ad_latency_ms";
            public const string CacheAgeMs = "ad_cache_age_ms";
            public const string WaitMs = "ad_wait_ms";
            public const string VisibleMs = "ad_visible_ms";

            // ad_impression uses Firebase's own vocabulary, NOT the names above.
            // GA4 recognises these exact keys and totals ad revenue from them;
            // renaming them for "consistency" is what breaks that.
            public const string AdPlatform = "ad_platform";
            public const string AdSource = "ad_source";
            public const string AdUnitName = "ad_unit_name";
            public const string Value = "value";
            public const string Currency = "currency";
        }

        /// <summary>
        /// Global event hook for routing ad funnel analytics events to any telemetry backend
        /// (e.g. Firebase Analytics, GameAnalytics, Adjust, etc.).
        /// </summary>
        public static event Action<string, AnalyticsParams> OnLogEvent;

        // Every ad event also goes to the console in one line, so the ad flow can
        // be followed on device without Firebase DebugView:
        //   [Ads] ad_show_start : ad_unit_id:xxx | ad_format:rewarded | ...
        private static void Log(string eventName, AnalyticsParams parameters)
        {
            var sb = new StringBuilder("[Ads] ").Append(eventName).Append(" :");
            var entries = parameters.Entries;
            for (int i = 0; i < entries.Count; i++)
                sb.Append(i == 0 ? " " : " | ").Append(entries[i].Key).Append(':').Append(entries[i].Value);
            Debug.Log(sb.ToString());

            OnLogEvent?.Invoke(eventName, parameters);
        }

        // ===== Funnel B — supply side =====

        public static void LoadStart(string adUnitId, string format, int retryAttempt)
        {
            Log(Names.LoadStart, AnalyticsParams.New(3)
                .Str(P.UnitId, adUnitId)
                .Str(P.Format, format)
                .Int(P.RetryAttempt, retryAttempt));
        }

        public static void LoadSuccess(string adUnitId, string format, string network, int retryAttempt, long latencyMs)
        {
            Log(Names.LoadSuccess, AnalyticsParams.New(5)
                .Str(P.UnitId, adUnitId)
                .Str(P.Format, format)
                .Str(P.Network, network)
                .Int(P.RetryAttempt, retryAttempt)
                .Int(P.LatencyMs, latencyMs));
        }

        /// <summary>
        /// No ad_network parameter by design — see constraint 1 in the class summary.
        /// </summary>
        public static void LoadFailed(string adUnitId, string format, int errorCode, string errorMessage, int retryAttempt)
        {
            Log(Names.LoadFailed, AnalyticsParams.New(5)
                .Str(P.UnitId, adUnitId)
                .Str(P.Format, format)
                .Int(P.ErrorCode, errorCode)
                .Str(P.ErrorMessage, errorMessage)
                .Int(P.RetryAttempt, retryAttempt));
        }

        // ===== Funnel A — opportunity side =====

        /// <summary>
        /// The game decided it wants to show an ad. Logged whether or not one is
        /// available: the share of these with <c>ad_ready = false</c> is the headline
        /// metric — a player who asked for an ad and got nothing.
        /// </summary>
        public static void ShowRequest(string adUnitId, string format, string placement,
            bool ready, string notReadyReason, string network, string requestId)
        {
            Log(Names.ShowRequest, AnalyticsParams.New(7)
                .Str(P.UnitId, adUnitId)
                .Str(P.Format, format)
                .Str(P.Placement, placement)
                .Bool(P.Ready, ready)
                .Str(P.NotReadyReason, ready ? null : notReadyReason)
                .Str(P.Network, network)
                .Str(P.RequestId, requestId));
        }

        public static void ShowStart(string adUnitId, string format, string placement, string network,
            long cacheAgeMs, long waitMs, string requestId)
        {
            Log(Names.ShowStart, AnalyticsParams.New(7)
                .Str(P.UnitId, adUnitId)
                .Str(P.Format, format)
                .Str(P.Placement, placement)
                .Str(P.Network, network)
                .Int(P.CacheAgeMs, cacheAgeMs)
                .Int(P.WaitMs, waitMs)
                .Str(P.RequestId, requestId));
        }

        public static void ShowSuccess(string adUnitId, string format, string placement, string network, string requestId)
        {
            Log(Names.ShowSuccess, AnalyticsParams.New(5)
                .Str(P.UnitId, adUnitId)
                .Str(P.Format, format)
                .Str(P.Placement, placement)
                .Str(P.Network, network)
                .Str(P.RequestId, requestId));
        }

        public static void ShowFailed(string adUnitId, string format, string placement, string network,
            int errorCode, string errorMessage, string requestId)
        {
            Log(Names.ShowFailed, AnalyticsParams.New(7)
                .Str(P.UnitId, adUnitId)
                .Str(P.Format, format)
                .Str(P.Placement, placement)
                .Str(P.Network, network)
                .Int(P.ErrorCode, errorCode)
                .Str(P.ErrorMessage, errorMessage)
                .Str(P.RequestId, requestId));
        }

        /// <summary>
        /// The player watched far enough to earn the reward. Rewarded only, and NOT
        /// implied by <c>ad_show_success</c> — closing the video early still produces
        /// a successful show with no reward. The gap between the two is the drop-off
        /// rate: 1 − reward_earned / show_success.
        ///
        /// Named ad_reward_earned because <c>ad_reward</c> is reserved by Firebase.
        /// </summary>
        public static void RewardEarned(string adUnitId, string placement, string network, string requestId)
        {
            Log(Names.RewardEarned, AnalyticsParams.New(4)
                .Str(P.UnitId, adUnitId)
                .Str(P.Placement, placement)
                .Str(P.Network, network)
                .Str(P.RequestId, requestId));
        }

        public static void Dismissed(string adUnitId, string format, string placement, string network,
            bool rewarded, string requestId)
        {
            Log(Names.Dismissed, AnalyticsParams.New(6)
                .Str(P.UnitId, adUnitId)
                .Str(P.Format, format)
                .Str(P.Placement, placement)
                .Str(P.Network, network)
                .Bool(P.Rewarded, rewarded)
                .Str(P.RequestId, requestId));
        }

        /// <summary>Named ad_clicked because <c>ad_click</c> is reserved by Firebase.</summary>
        public static void Clicked(string adUnitId, string format, string placement, string network)
        {
            Log(Names.Clicked, AnalyticsParams.New(4)
                .Str(P.UnitId, adUnitId)
                .Str(P.Format, format)
                .Str(P.Placement, placement)
                .Str(P.Network, network));
        }

        // ===== Revenue =====

        /// <summary>
        /// Firebase's standard ad-revenue event, logged manually as the Firebase docs
        /// prescribe for mediation. GA4 recognises the event name and its parameter
        /// vocabulary and folds the value into ad revenue on its own — which is why
        /// these parameter names differ from every other method here.
        ///
        /// Currency is always USD: AppLovin reports all revenue in USD.
        /// </summary>
        public static void Impression(string network, string adUnitId, string format, double revenue)
        {
            Log(Names.Impression, AnalyticsParams.New(6)
                .Str(P.AdPlatform, "AppLovin")
                .Str(P.AdSource, network)
                .Str(P.AdUnitName, adUnitId)
                .Str(P.Format, format)
                .Num(P.Value, revenue)
                .Str(P.Currency, "USD"));
        }

        // ===== Banner / MREC =====
        // These formats get display duration and revenue only, never the 7-step
        // funnel: MAX auto-refreshes them roughly every 60s, so load events would
        // outnumber every other event in the project and drown out the real signal.

        public static void DisplayStart(string adUnitId, string format, string placement)
        {
            Log(Names.DisplayStart, AnalyticsParams.New(3)
                .Str(P.UnitId, adUnitId)
                .Str(P.Format, format)
                .Str(P.Placement, placement));
        }

        public static void DisplayEnd(string adUnitId, string format, string placement, long visibleMs)
        {
            Log(Names.DisplayEnd, AnalyticsParams.New(4)
                .Str(P.UnitId, adUnitId)
                .Str(P.Format, format)
                .Str(P.Placement, placement)
                .Int(P.VisibleMs, visibleMs));
        }
    }
}
