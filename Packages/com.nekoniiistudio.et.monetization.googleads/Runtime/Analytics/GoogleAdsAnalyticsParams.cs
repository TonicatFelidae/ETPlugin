namespace ET.Analytics
{
    /// <summary>
    /// Telemetry parameter keys and standard ad events for Google Mobile Ads tracking.
    /// </summary>
    public static class GoogleAdsAnalyticsParams
    {
        // Event names
        public const string EventAdShowRequest = "ad_show_request";
        public const string EventAdDisplayStart = "ad_display_start";
        public const string EventAdDisplayEnd = "ad_display_end";
        public const string EventAdLoadRequest = "ad_load_request";
        public const string EventAdLoaded = "ad_loaded";
        public const string EventAdLoadFailed = "ad_load_failed";
        public const string EventAdFailedToShow = "ad_failed_to_show";
        public const string EventAdClicked = "ad_clicked";
        public const string EventAdRewardEarned = "ad_reward_earned";
        public const string EventAdImpression = "ad_impression";

        // Parameter keys
        public const string ParamAdFormat = "ad_format";
        public const string ParamPlacement = "placement";
        public const string ParamAdUnitId = "ad_unit_id";
        public const string ParamLatencyMs = "latency_ms";
        public const string ParamErrorCode = "error_code";
        public const string ParamErrorMessage = "error_message";
        public const string ParamValue = "value";
        public const string ParamCurrency = "currency";
        public const string ParamPrecision = "precision";
        public const string ParamNetwork = "network";
        public const string ParamRewardType = "reward_type";
        public const string ParamRewardAmount = "reward_amount";
        public const string ParamRequestId = "request_id";
    }
}
