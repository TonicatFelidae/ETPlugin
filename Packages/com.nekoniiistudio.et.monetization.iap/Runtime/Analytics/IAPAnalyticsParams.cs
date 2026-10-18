namespace ET.Analytics
{
    /// <summary>
    /// Standardized event names and parameter keys for In-App Purchasing telemetry.
    /// </summary>
    public static class IAPAnalyticsParams
    {
        // Event Names
        public const string EVENT_IAP_INIT_REQUEST = "iap_init_request";
        public const string EVENT_IAP_INITIALIZED = "iap_initialized";
        public const string EVENT_IAP_INIT_FAILED = "iap_init_failed";
        public const string EVENT_IAP_PURCHASE_START = "iap_purchase_start";
        public const string EVENT_IAP_PURCHASE_SUCCESS = "iap_purchase_success";
        public const string EVENT_IAP_PURCHASE_FAILED = "iap_purchase_failed";
        public const string EVENT_IAP_PURCHASE_DEFERRED = "iap_purchase_deferred";
        public const string EVENT_IAP_RESTORE_START = "iap_restore_start";
        public const string EVENT_IAP_RESTORE_COMPLETE = "iap_restore_complete";

        // Parameter Keys
        public const string PARAM_PRODUCT_ID = "product_id";
        public const string PARAM_PRODUCT_TYPE = "product_type";
        public const string PARAM_TRANSACTION_ID = "transaction_id";
        public const string PARAM_PRICE = "price";
        public const string PARAM_CURRENCY = "currency";
        public const string PARAM_PLACEMENT = "placement";
        public const string PARAM_IS_RESTORED = "is_restored";
        public const string PARAM_FAILURE_REASON = "failure_reason";
        public const string PARAM_ERROR_MESSAGE = "error_message";
        public const string PARAM_LATENCY_MS = "latency_ms";
        public const string PARAM_PRODUCT_COUNT = "product_count";
        public const string PARAM_SUCCESS = "success";
    }
}
