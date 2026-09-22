using System.Collections.Generic;
using UnityEngine;

namespace ET.Analytics
{
    /// <summary>
    /// Measures ad lifecycle timings and publishes funnel events.
    /// Dedicated per ad format to prevent concurrent fills or background refreshes
    /// from corrupting active placement or request data.
    /// </summary>
    public class GoogleAdsFunnelTracker
    {
        private readonly GoogleAdsAnalytics.Format _format;
        private readonly string _formatName;

        private float _loadRequestedAt = -1f;
        private float _showRequestedAt = -1f;
        private float _displayedAt = -1f;

        private string _activePlacement = "";
        private string _activeRequestId = "";

        public GoogleAdsFunnelTracker(GoogleAdsAnalytics.Format format)
        {
            _format = format;
            _formatName = GoogleAdsAnalytics.FormatToString(format);
        }

        public void TrackLoadRequest(string adUnitId)
        {
            _loadRequestedAt = Time.realtimeSinceStartup;
            var parameters = new Dictionary<string, object>
            {
                { GoogleAdsAnalyticsParams.ParamAdFormat, _formatName },
                { GoogleAdsAnalyticsParams.ParamAdUnitId, adUnitId }
            };
            GoogleAdsAnalytics.LogEvent(GoogleAdsAnalyticsParams.EventAdLoadRequest, parameters);
        }

        public void TrackLoaded(string adUnitId)
        {
            long latencyMs = _loadRequestedAt > 0f ? (long)((Time.realtimeSinceStartup - _loadRequestedAt) * 1000f) : 0;
            var parameters = new Dictionary<string, object>
            {
                { GoogleAdsAnalyticsParams.ParamAdFormat, _formatName },
                { GoogleAdsAnalyticsParams.ParamAdUnitId, adUnitId },
                { GoogleAdsAnalyticsParams.ParamLatencyMs, latencyMs }
            };
            GoogleAdsAnalytics.LogEvent(GoogleAdsAnalyticsParams.EventAdLoaded, parameters);
        }

        public void TrackLoadFailed(string adUnitId, int errorCode, string errorMessage)
        {
            long latencyMs = _loadRequestedAt > 0f ? (long)((Time.realtimeSinceStartup - _loadRequestedAt) * 1000f) : 0;
            var parameters = new Dictionary<string, object>
            {
                { GoogleAdsAnalyticsParams.ParamAdFormat, _formatName },
                { GoogleAdsAnalyticsParams.ParamAdUnitId, adUnitId },
                { GoogleAdsAnalyticsParams.ParamErrorCode, errorCode },
                { GoogleAdsAnalyticsParams.ParamErrorMessage, errorMessage ?? "" },
                { GoogleAdsAnalyticsParams.ParamLatencyMs, latencyMs }
            };
            GoogleAdsAnalytics.LogEvent(GoogleAdsAnalyticsParams.EventAdLoadFailed, parameters);
        }

        public void TrackShowRequest(string placement, string requestId = null)
        {
            _showRequestedAt = Time.realtimeSinceStartup;
            _activePlacement = placement ?? "";
            _activeRequestId = requestId ?? "";

            var parameters = new Dictionary<string, object>
            {
                { GoogleAdsAnalyticsParams.ParamAdFormat, _formatName },
                { GoogleAdsAnalyticsParams.ParamPlacement, _activePlacement },
                { GoogleAdsAnalyticsParams.ParamRequestId, _activeRequestId }
            };
            GoogleAdsAnalytics.LogEvent(GoogleAdsAnalyticsParams.EventAdShowRequest, parameters);
        }

        public void TrackDisplayStart()
        {
            _displayedAt = Time.realtimeSinceStartup;
            long latencyMs = _showRequestedAt > 0f ? (long)((Time.realtimeSinceStartup - _showRequestedAt) * 1000f) : 0;

            var parameters = new Dictionary<string, object>
            {
                { GoogleAdsAnalyticsParams.ParamAdFormat, _formatName },
                { GoogleAdsAnalyticsParams.ParamPlacement, _activePlacement },
                { GoogleAdsAnalyticsParams.ParamRequestId, _activeRequestId },
                { GoogleAdsAnalyticsParams.ParamLatencyMs, latencyMs }
            };
            GoogleAdsAnalytics.LogEvent(GoogleAdsAnalyticsParams.EventAdDisplayStart, parameters);
        }

        public void TrackDisplayEnd()
        {
            long durationMs = _displayedAt > 0f ? (long)((Time.realtimeSinceStartup - _displayedAt) * 1000f) : 0;

            var parameters = new Dictionary<string, object>
            {
                { GoogleAdsAnalyticsParams.ParamAdFormat, _formatName },
                { GoogleAdsAnalyticsParams.ParamPlacement, _activePlacement },
                { GoogleAdsAnalyticsParams.ParamRequestId, _activeRequestId },
                { GoogleAdsAnalyticsParams.ParamLatencyMs, durationMs }
            };
            GoogleAdsAnalytics.LogEvent(GoogleAdsAnalyticsParams.EventAdDisplayEnd, parameters);

            _displayedAt = -1f;
            _showRequestedAt = -1f;
        }

        public void TrackFailedToShow(int errorCode, string errorMessage)
        {
            long latencyMs = _showRequestedAt > 0f ? (long)((Time.realtimeSinceStartup - _showRequestedAt) * 1000f) : 0;

            var parameters = new Dictionary<string, object>
            {
                { GoogleAdsAnalyticsParams.ParamAdFormat, _formatName },
                { GoogleAdsAnalyticsParams.ParamPlacement, _activePlacement },
                { GoogleAdsAnalyticsParams.ParamRequestId, _activeRequestId },
                { GoogleAdsAnalyticsParams.ParamErrorCode, errorCode },
                { GoogleAdsAnalyticsParams.ParamErrorMessage, errorMessage ?? "" },
                { GoogleAdsAnalyticsParams.ParamLatencyMs, latencyMs }
            };
            GoogleAdsAnalytics.LogEvent(GoogleAdsAnalyticsParams.EventAdFailedToShow, parameters);
        }

        public void TrackClicked()
        {
            var parameters = new Dictionary<string, object>
            {
                { GoogleAdsAnalyticsParams.ParamAdFormat, _formatName },
                { GoogleAdsAnalyticsParams.ParamPlacement, _activePlacement },
                { GoogleAdsAnalyticsParams.ParamRequestId, _activeRequestId }
            };
            GoogleAdsAnalytics.LogEvent(GoogleAdsAnalyticsParams.EventAdClicked, parameters);
        }

        public void TrackRewardEarned(string type, double amount)
        {
            var parameters = new Dictionary<string, object>
            {
                { GoogleAdsAnalyticsParams.ParamAdFormat, _formatName },
                { GoogleAdsAnalyticsParams.ParamPlacement, _activePlacement },
                { GoogleAdsAnalyticsParams.ParamRequestId, _activeRequestId },
                { GoogleAdsAnalyticsParams.ParamRewardType, type ?? "" },
                { GoogleAdsAnalyticsParams.ParamRewardAmount, amount }
            };
            GoogleAdsAnalytics.LogEvent(GoogleAdsAnalyticsParams.EventAdRewardEarned, parameters);
        }

        public void TrackImpression(double revenue, string currency, string precision)
        {
            var parameters = new Dictionary<string, object>
            {
                { GoogleAdsAnalyticsParams.ParamAdFormat, _formatName },
                { GoogleAdsAnalyticsParams.ParamPlacement, _activePlacement },
                { GoogleAdsAnalyticsParams.ParamRequestId, _activeRequestId },
                { GoogleAdsAnalyticsParams.ParamValue, revenue },
                { GoogleAdsAnalyticsParams.ParamCurrency, currency ?? "USD" },
                { GoogleAdsAnalyticsParams.ParamPrecision, precision ?? "" }
            };
            GoogleAdsAnalytics.LogEvent(GoogleAdsAnalyticsParams.EventAdImpression, parameters);
        }
    }
}
