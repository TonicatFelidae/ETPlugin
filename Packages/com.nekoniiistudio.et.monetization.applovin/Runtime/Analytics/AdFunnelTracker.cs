#if ENABLE_MAX
using ET.Analytics;
using UnityEngine;

namespace ET.Monetization
{
    /// <summary>
    /// Holds the funnel state for ONE ad format (interstitial or rewarded) and emits
    /// the ad events as that state changes.
    ///
    /// It exists because several parameters in the tracking plan cannot be read back
    /// from the MAX SDK at the moment they are needed:
    ///
    /// * <c>ad_network</c> on <c>ad_show_request</c> — MAX exposes no AdInfo for a
    ///   cached ad, only <c>IsRewardedAdReady()</c>. The network of the ad that is
    ///   about to show is the one from the last successful load, so it is remembered
    ///   here.
    /// * <c>ad_latency_ms</c> / <c>ad_cache_age_ms</c> — both are durations measured
    ///   between two unrelated callbacks.
    /// * <c>ad_placement</c> / <c>ad_request_id</c> — supplied by the caller at show
    ///   time, then needed again on the display / reward / dismiss callbacks, which
    ///   MAX fires with no memory of who asked.
    ///
    /// One instance per format: MAX's callbacks are static and shared, so a single
    /// shared instance would let a background rewarded refill overwrite the
    /// interstitial currently on screen.
    /// </summary>
    internal sealed class AdFunnelTracker
    {
        private readonly string _format;

        private float _loadStartedAt;
        private float _loadedAt = -1f;
        private bool _loadInFlight;
        private bool _lastLoadFailed;

        public AdFunnelTracker(string format)
        {
            _format = format;
        }

        /// <summary>Network of the ad currently cached, or null when nothing is cached.</summary>
        public string Network { get; private set; }

        /// <summary>Placement of the show currently in flight.</summary>
        public string Placement { get; private set; }

        /// <summary>
        /// Correlation id of the show currently in flight. For rewarded this is the
        /// server's <c>impressionId</c> from /ad-checks/start, deliberately reused so
        /// Firebase rows can later be joined to the <c>ad_impressions</c> table.
        /// </summary>
        public string RequestId { get; private set; }

        /// <summary>How long the player was held on the "ad is loading" spinner.</summary>
        public long WaitMs { get; private set; }

        // ===== Funnel B — load =====

        public void LoadStart(string adUnitId, int retryAttempt)
        {
            _loadStartedAt = Time.realtimeSinceStartup;
            _loadInFlight = true;
            AdAnalytics.LoadStart(adUnitId, _format, retryAttempt);
        }

        public void LoadSuccess(string adUnitId, string network, int retryAttempt)
        {
            _loadedAt = Time.realtimeSinceStartup;
            _loadInFlight = false;
            _lastLoadFailed = false;
            Network = network;
            AdAnalytics.LoadSuccess(adUnitId, _format, network, retryAttempt, ToMs(_loadedAt - _loadStartedAt));
        }

        /// <summary>
        /// <paramref name="retryAttempt"/> is the attempt number of the load that just
        /// failed, i.e. the value BEFORE the caller's retry counter is incremented —
        /// otherwise attempt 0 would never appear and fill rate, which is defined on
        /// attempt 0 only, could not be computed at all.
        /// </summary>
        public void LoadFailed(string adUnitId, int errorCode, string errorMessage, int retryAttempt)
        {
            _loadInFlight = false;
            _lastLoadFailed = true;
            // Nothing is cached any more, so the remembered network would be a lie on
            // the next show_request.
            Network = null;
            _loadedAt = -1f;
            AdAnalytics.LoadFailed(adUnitId, _format, errorCode, errorMessage, retryAttempt);
        }

        // ===== Funnel A — show =====

        public void ShowRequest(string adUnitId, string placement, bool ready, bool sdkInitialized,
            string requestId, long waitMs)
        {
            Placement = placement;
            RequestId = requestId;
            WaitMs = waitMs;

            AdAnalytics.ShowRequest(adUnitId, _format, placement, ready,
                ready ? null : ResolveNotReadyReason(sdkInitialized),
                ready ? Network : null,
                requestId);
        }

        public void ShowStart(string adUnitId)
        {
            AdAnalytics.ShowStart(adUnitId, _format, Placement, Network, CacheAgeMs(), WaitMs, RequestId);
        }

        public void ShowSuccess(string adUnitId, string network)
        {
            if (!string.IsNullOrEmpty(network)) Network = network;
            AdAnalytics.ShowSuccess(adUnitId, _format, Placement, Network, RequestId);
        }

        public void ShowFailed(string adUnitId, string network, int errorCode, string errorMessage)
        {
            if (!string.IsNullOrEmpty(network)) Network = network;
            AdAnalytics.ShowFailed(adUnitId, _format, Placement, Network, errorCode, errorMessage, RequestId);
        }

        public void RewardEarned(string adUnitId, string network)
        {
            if (!string.IsNullOrEmpty(network)) Network = network;
            AdAnalytics.RewardEarned(adUnitId, Placement, Network, RequestId);
        }

        public void Dismissed(string adUnitId, string network, bool rewarded)
        {
            if (!string.IsNullOrEmpty(network)) Network = network;
            AdAnalytics.Dismissed(adUnitId, _format, Placement, Network, rewarded, RequestId);

            // The show is over; the next one supplies its own placement and id.
            Placement = null;
            RequestId = null;
            WaitMs = 0;
        }

        public void Clicked(string adUnitId, string network)
        {
            AdAnalytics.Clicked(adUnitId, _format, Placement,
                string.IsNullOrEmpty(network) ? Network : network);
        }

        /// <summary>
        /// Why no ad was available. Only three of the five documented reasons can be
        /// answered from here — <c>cooldown</c> and <c>cap_reached</c> are gameplay
        /// gates that live above this layer and would have to be passed in by whoever
        /// enforces them.
        /// </summary>
        private string ResolveNotReadyReason(bool sdkInitialized)
        {
            if (!sdkInitialized) return AdAnalytics.NotReadyReason.SdkNotInit;
            if (_loadInFlight) return AdAnalytics.NotReadyReason.Loading;
            if (_lastLoadFailed) return AdAnalytics.NotReadyReason.NoFill;
            return AdAnalytics.NotReadyReason.Loading;
        }

        /// <summary>How long the ad about to be shown has been sitting in cache.</summary>
        private long CacheAgeMs() => _loadedAt < 0f ? 0L : ToMs(Time.realtimeSinceStartup - _loadedAt);

        private static long ToMs(float seconds) => seconds <= 0f ? 0L : (long)(seconds * 1000f);
    }
}
#endif
