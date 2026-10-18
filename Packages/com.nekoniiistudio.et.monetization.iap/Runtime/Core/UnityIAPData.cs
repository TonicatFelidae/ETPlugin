using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Monetization
{
    /// <summary>
    /// Product type classification for ET In-App Purchasing.
    /// </summary>
    public enum UnityIAPProductType
    {
        Consumable = 0,
        NonConsumable = 1,
        Subscription = 2,
        Unknown = 3
    }

    /// <summary>
    /// Standardized purchase failure reasons for ET In-App Purchasing.
    /// </summary>
    public enum UnityIAPPurchaseFailureReason
    {
        PurchasingUnavailable = 0,
        ExistingPurchasePending = 1,
        ProductUnavailable = 2,
        SignatureInvalid = 3,
        UserCancelled = 4,
        PaymentDeclined = 5,
        DuplicateTransaction = 6,
        Unknown = 7
    }

    /// <summary>
    /// Serializable configuration entry for a single IAP product.
    /// Supports store-specific IDs, PlayerPrefs persistence keys, and cross-product bundle ownership rules.
    /// </summary>
    [Serializable]
    public class UnityIAPProductEntry
    {
        [Tooltip("Unified catalog product identifier (e.g. 'remove_ads', 'full_pack_remove_ads').")]
        public string productId;

        [Tooltip("Product classification (Consumable, NonConsumable, Subscription).")]
        public UnityIAPProductType productType = UnityIAPProductType.NonConsumable;

        [Header("Store-Specific ID Overrides (Optional)")]
        [Tooltip("Optional Google Play Store product ID override. Leave empty to use productId.")]
        public string googlePlayId;

        [Tooltip("Optional Apple App Store product ID override. Leave empty to use productId.")]
        public string appleAppStoreId;

        [Header("Fallback Display Metadata")]
        [Tooltip("Fallback localized price string before store metadata is fetched.")]
        public string defaultPriceString = "$0.99";

        [Tooltip("Fallback localized title before store metadata is fetched.")]
        public string defaultTitle;

        [Tooltip("Fallback localized description before store metadata is fetched.")]
        public string defaultDescription;

        [Header("Ownership & Cross-Product Bundle Rules")]
        [Tooltip("Additional PlayerPrefs integer keys automatically set to 1 when this product is owned.")]
        public string[] refPlayerPrefs = Array.Empty<string>();

        [Tooltip("If any of the listed product IDs (e.g. an Ultimate Bundle) are owned, this product is automatically marked as owned.")]
        public string[] ownedIfOwnedProducts = Array.Empty<string>();
    }

    /// <summary>
    /// Runtime metadata and ownership state for a product.
    /// </summary>
    [Serializable]
    public struct UnityIAPProductMetadata
    {
        public string ProductId;
        public UnityIAPProductType ProductType;
        public string LocalizedPriceString;
        public decimal LocalizedPrice;
        public string IsoCurrencyCode;
        public string LocalizedTitle;
        public string LocalizedDescription;
        public bool IsAvailableToPurchase;
        public bool IsOwned;
    }

    /// <summary>
    /// Result payload emitted when a product purchase or restoration succeeds.
    /// </summary>
    [Serializable]
    public struct UnityIAPPurchaseResult
    {
        public string ProductId;
        public UnityIAPProductType ProductType;
        public string TransactionId;
        public string Receipt;
        public decimal LocalizedPrice;
        public string IsoCurrencyCode;
        public bool IsRestored;
        public string Placement;
    }

    /// <summary>
    /// Failure payload emitted when a product purchase fails.
    /// </summary>
    [Serializable]
    public struct UnityIAPPurchaseFailure
    {
        public string ProductId;
        public UnityIAPPurchaseFailureReason Reason;
        public string Message;
        public string Placement;
    }

    /// <summary>
    /// Rich data configuration container for Unity In-App Purchasing (v5+).
    /// Holds product definitions, store initialization flags, offline persistence rules,
    /// and lookup helper functions.
    /// </summary>
    [Serializable]
    public class UnityIAPData
    {
        [Header("Initialization Settings")]
        [Tooltip("Automatically initialize Unity Gaming Services and connect to the store on startup.")]
        public bool AutoInitialize = true;

        [Tooltip("Automatically initialize Unity Gaming Services (UnityServices.InitializeAsync) before connecting.")]
        public bool AutoInitializeUnityGamingServices = true;

        [Tooltip("Automatically merge products from Unity's default Resources/IAPProductCatalog.json.")]
        public bool UseCodelessCatalog = true;

        [Tooltip("Automatically confirm pending orders once purchase callbacks execute.")]
        public bool AutoConfirmPurchases = true;

        [Tooltip("Maximum retry attempts when store connection or product fetching fails.")]
        public int MaxRetryAttempts = 5;

        [Header("Ownership & Editor Settings")]
        [Tooltip("Cache Non-Consumable and Subscription ownership in PlayerPrefs for instant offline startup checks.")]
        public bool PersistOwnershipToPlayerPrefs = true;

        [Tooltip("PlayerPrefs key prefix used for persisting product ownership.")]
        public string PlayerPrefsPrefix = "ET_IAP_OWNED_";

        [Tooltip("When running in the Unity Editor without a live store, simulate immediate successful purchases.")]
        public bool SimulateSuccessInEditor = true;

        [Header("Configured Products")]
        [Tooltip("List of configured IAP products and cross-product ownership rules.")]
        public List<UnityIAPProductEntry> Products = new List<UnityIAPProductEntry>();

        private Dictionary<string, UnityIAPProductEntry> _productLookup;

        public Dictionary<string, UnityIAPProductEntry> ProductLookup
        {
            get
            {
                if (_productLookup == null)
                {
                    RebuildLookup();
                }
                return _productLookup;
            }
        }

        public void RebuildLookup()
        {
            _productLookup = new Dictionary<string, UnityIAPProductEntry>(StringComparer.Ordinal);
            if (Products == null) return;

            for (int i = 0; i < Products.Count; i++)
            {
                var entry = Products[i];
                if (entry != null && !string.IsNullOrEmpty(entry.productId))
                {
                    _productLookup[entry.productId] = entry;
                }
            }
        }

        public UnityIAPProductEntry GetProductEntry(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return null;
            return ProductLookup.TryGetValue(productId, out var entry) ? entry : null;
        }

        public void AddOrUpdateProduct(UnityIAPProductEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.productId)) return;

            if (Products == null)
            {
                Products = new List<UnityIAPProductEntry>();
            }

            int existingIndex = Products.FindIndex(p => p != null && p.productId == entry.productId);
            if (existingIndex >= 0)
            {
                Products[existingIndex] = entry;
            }
            else
            {
                Products.Add(entry);
            }

            RebuildLookup();
        }

        public string GetOwnershipPrefKey(string productId)
        {
            return $"{PlayerPrefsPrefix}{productId}";
        }
    }
}
