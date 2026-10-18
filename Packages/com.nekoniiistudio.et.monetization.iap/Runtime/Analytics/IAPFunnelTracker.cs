using System.Collections.Generic;
using UnityEngine;

namespace ET.Analytics
{
    /// <summary>
    /// Measures In-App Purchasing lifecycle timings and publishes standardized funnel events.
    /// </summary>
    public class IAPFunnelTracker
    {
        private float _initRequestedAt = -1f;
        private float _restoreRequestedAt = -1f;
        private readonly Dictionary<string, float> _purchaseRequestedAt = new Dictionary<string, float>();
        private readonly Dictionary<string, string> _activePlacements = new Dictionary<string, string>();

        public void TrackInitRequest()
        {
            _initRequestedAt = Time.realtimeSinceStartup;
            IAPAnalytics.LogEvent(IAPAnalyticsParams.EVENT_IAP_INIT_REQUEST, new Dictionary<string, object>());
        }

        public void TrackInitialized(int productCount)
        {
            long latencyMs = _initRequestedAt > 0f ? (long)((Time.realtimeSinceStartup - _initRequestedAt) * 1000f) : 0;
            var parameters = new Dictionary<string, object>
            {
                { IAPAnalyticsParams.PARAM_PRODUCT_COUNT, productCount },
                { IAPAnalyticsParams.PARAM_LATENCY_MS, latencyMs }
            };
            IAPAnalytics.LogEvent(IAPAnalyticsParams.EVENT_IAP_INITIALIZED, parameters);
        }

        public void TrackInitFailed(string errorMessage)
        {
            long latencyMs = _initRequestedAt > 0f ? (long)((Time.realtimeSinceStartup - _initRequestedAt) * 1000f) : 0;
            var parameters = new Dictionary<string, object>
            {
                { IAPAnalyticsParams.PARAM_ERROR_MESSAGE, errorMessage ?? "" },
                { IAPAnalyticsParams.PARAM_LATENCY_MS, latencyMs }
            };
            IAPAnalytics.LogEvent(IAPAnalyticsParams.EVENT_IAP_INIT_FAILED, parameters);
        }

        public void TrackPurchaseStart(string productId, string productType, string placement)
        {
            if (string.IsNullOrEmpty(productId)) return;

            _purchaseRequestedAt[productId] = Time.realtimeSinceStartup;
            _activePlacements[productId] = placement ?? "";

            var parameters = new Dictionary<string, object>
            {
                { IAPAnalyticsParams.PARAM_PRODUCT_ID, productId },
                { IAPAnalyticsParams.PARAM_PRODUCT_TYPE, productType ?? "" },
                { IAPAnalyticsParams.PARAM_PLACEMENT, placement ?? "" }
            };
            IAPAnalytics.LogEvent(IAPAnalyticsParams.EVENT_IAP_PURCHASE_START, parameters);
        }

        public void TrackPurchaseSuccess(string productId, string productType, string transactionId, decimal price, string currency, bool isRestored)
        {
            if (string.IsNullOrEmpty(productId)) return;

            long latencyMs = 0;
            if (_purchaseRequestedAt.TryGetValue(productId, out float startedAt) && startedAt > 0f)
            {
                latencyMs = (long)((Time.realtimeSinceStartup - startedAt) * 1000f);
                _purchaseRequestedAt.Remove(productId);
            }

            _activePlacements.TryGetValue(productId, out string placement);
            _activePlacements.Remove(productId);

            var parameters = new Dictionary<string, object>
            {
                { IAPAnalyticsParams.PARAM_PRODUCT_ID, productId },
                { IAPAnalyticsParams.PARAM_PRODUCT_TYPE, productType ?? "" },
                { IAPAnalyticsParams.PARAM_TRANSACTION_ID, transactionId ?? "" },
                { IAPAnalyticsParams.PARAM_PRICE, (double)price },
                { IAPAnalyticsParams.PARAM_CURRENCY, currency ?? "USD" },
                { IAPAnalyticsParams.PARAM_IS_RESTORED, isRestored },
                { IAPAnalyticsParams.PARAM_PLACEMENT, placement ?? "" },
                { IAPAnalyticsParams.PARAM_LATENCY_MS, latencyMs }
            };
            IAPAnalytics.LogEvent(IAPAnalyticsParams.EVENT_IAP_PURCHASE_SUCCESS, parameters);
        }

        public void TrackPurchaseFailed(string productId, string failureReason, string errorMessage)
        {
            if (string.IsNullOrEmpty(productId)) return;

            long latencyMs = 0;
            if (_purchaseRequestedAt.TryGetValue(productId, out float startedAt) && startedAt > 0f)
            {
                latencyMs = (long)((Time.realtimeSinceStartup - startedAt) * 1000f);
                _purchaseRequestedAt.Remove(productId);
            }

            _activePlacements.TryGetValue(productId, out string placement);
            _activePlacements.Remove(productId);

            var parameters = new Dictionary<string, object>
            {
                { IAPAnalyticsParams.PARAM_PRODUCT_ID, productId },
                { IAPAnalyticsParams.PARAM_FAILURE_REASON, failureReason ?? "" },
                { IAPAnalyticsParams.PARAM_ERROR_MESSAGE, errorMessage ?? "" },
                { IAPAnalyticsParams.PARAM_PLACEMENT, placement ?? "" },
                { IAPAnalyticsParams.PARAM_LATENCY_MS, latencyMs }
            };
            IAPAnalytics.LogEvent(IAPAnalyticsParams.EVENT_IAP_PURCHASE_FAILED, parameters);
        }

        public void TrackPurchaseDeferred(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return;

            _activePlacements.TryGetValue(productId, out string placement);
            var parameters = new Dictionary<string, object>
            {
                { IAPAnalyticsParams.PARAM_PRODUCT_ID, productId },
                { IAPAnalyticsParams.PARAM_PLACEMENT, placement ?? "" }
            };
            IAPAnalytics.LogEvent(IAPAnalyticsParams.EVENT_IAP_PURCHASE_DEFERRED, parameters);
        }

        public void TrackRestoreStart()
        {
            _restoreRequestedAt = Time.realtimeSinceStartup;
            IAPAnalytics.LogEvent(IAPAnalyticsParams.EVENT_IAP_RESTORE_START, new Dictionary<string, object>());
        }

        public void TrackRestoreComplete(bool success, string errorMessage)
        {
            long latencyMs = _restoreRequestedAt > 0f ? (long)((Time.realtimeSinceStartup - _restoreRequestedAt) * 1000f) : 0;
            _restoreRequestedAt = -1f;

            var parameters = new Dictionary<string, object>
            {
                { IAPAnalyticsParams.PARAM_SUCCESS, success },
                { IAPAnalyticsParams.PARAM_ERROR_MESSAGE, errorMessage ?? "" },
                { IAPAnalyticsParams.PARAM_LATENCY_MS, latencyMs }
            };
            IAPAnalytics.LogEvent(IAPAnalyticsParams.EVENT_IAP_RESTORE_COMPLETE, parameters);
        }
    }
}
