using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ET.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Events;

#if ENABLE_UNITY_IAP
using UnityEngine.Purchasing;
#endif

namespace ET.Monetization
{
    /// <summary>
    /// Modern In-App Purchasing manager built for Unity IAP v5+ (StoreController API).
    /// Provides unified product catalog management, asynchronous store connection,
    /// cross-product bundle entitlement resolution, offline PlayerPrefs caching,
    /// and decoupled telemetry tracking with zero obsolete v4 calls.
    /// </summary>
    public class UnityIAPManager
    {
        private static UnityIAPManager _defaultInstance;

        /// <summary>
        /// Active manager instance for convenient UI component binding (e.g. ETIAPButton)
        /// when not explicitly injected via DI.
        /// </summary>
        public static UnityIAPManager DefaultInstance
        {
            get => _defaultInstance;
            set => _defaultInstance = value;
        }

        private UnityIAPData _data;
        private bool _isInitialized;
        private bool _isInitializing;
        private bool _initialProductsFetched;

        private readonly IAPFunnelTracker _funnelTracker = new IAPFunnelTracker();
        private readonly Dictionary<string, bool> _ownedProducts = new Dictionary<string, bool>(StringComparer.Ordinal);
        private readonly Dictionary<string, UnityIAPProductMetadata> _productMetadataMap = new Dictionary<string, UnityIAPProductMetadata>(StringComparer.Ordinal);
        private readonly Dictionary<string, PendingPurchaseCallback> _pendingPurchaseCallbacks = new Dictionary<string, PendingPurchaseCallback>(StringComparer.Ordinal);

#if ENABLE_UNITY_IAP_V5
        private StoreController _storeController;
        private CatalogProvider _catalogProvider;
#endif

        // Events
        public event Action OnInitialized;
        public event Action<string> OnInitializeFailed;
        public event Action<IReadOnlyList<UnityIAPProductMetadata>> OnProductsFetched;
        public event Action<UnityIAPPurchaseResult> OnPurchaseSucceeded;
        public event Action<UnityIAPPurchaseFailure> OnPurchaseFailed;
        public event Action<string> OnPurchaseDeferred;
        public event Action<bool, string> OnPurchasesRestored;
        public event Action<string, bool> OnOwnershipChanged;

        public UnityIAPData Data => _data;
        public bool IsInitialized => _isInitialized;

        private struct PendingPurchaseCallback
        {
            public UnityAction<UnityIAPPurchaseResult> OnSuccess;
            public UnityAction<UnityIAPPurchaseFailure> OnFailed;
            public string Placement;
        }

        public UnityIAPManager(UnityIAPData data = null)
        {
            _data = data ?? new UnityIAPData();
            _defaultInstance = this;

            LoadCachedOwnershipFromPrefs();

            if (_data.AutoInitialize)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Initializes Unity Gaming Services (if enabled) and connects to the store.
        /// </summary>
        public void Initialize(UnityIAPData data = null, Action onInitialized = null)
        {
            InitializeInternalAsync(data, onInitialized).Forget();
        }

        /// <summary>
        /// Asynchronously initializes Unity Gaming Services and connects to the store.
        /// </summary>
        public async UniTask<bool> InitializeAsync(UnityIAPData data = null)
        {
            var tcs = new UniTaskCompletionSource<bool>();

            if (_isInitialized && data == null)
            {
                return true;
            }

            void HandleInit()
            {
                OnInitialized -= HandleInit;
                OnInitializeFailed -= HandleFail;
                tcs.TrySetResult(true);
            }

            void HandleFail(string _)
            {
                OnInitialized -= HandleInit;
                OnInitializeFailed -= HandleFail;
                tcs.TrySetResult(false);
            }

            OnInitialized += HandleInit;
            OnInitializeFailed += HandleFail;

            await InitializeInternalAsync(data, null);
            return await tcs.Task;
        }

        private async UniTask InitializeInternalAsync(UnityIAPData data, Action onInitialized)
        {
            if (data != null)
            {
                _data = data;
                _data.RebuildLookup();
                LoadCachedOwnershipFromPrefs();
            }

            if (_isInitialized)
            {
                onInitialized?.Invoke();
                return;
            }

            if (_isInitializing)
            {
                return;
            }

            _isInitializing = true;
            _initialProductsFetched = false;
            _funnelTracker.TrackInitRequest();

            try
            {
                if (_data.AutoInitializeUnityGamingServices && UnityServices.State == ServicesInitializationState.Uninitialized)
                {
                    await UnityServices.InitializeAsync().AsUniTask();
                }

#if ENABLE_UNITY_IAP_V5
                InitializeStoreControllerV5(onInitialized);
                await _storeController.Connect().AsUniTask();
#else
                SimulateOfflineInitialization(onInitialized);
#endif
            }
            catch (Exception ex)
            {
                _isInitializing = false;
                string errorMsg = ex.Message;
                Debug.LogError($"[UnityIAPManager] Store initialization failed: {errorMsg}");
                _funnelTracker.TrackInitFailed(errorMsg);
                OnInitializeFailed?.Invoke(errorMsg);
            }
        }

        #region Unity IAP v5 StoreController Implementation
#if ENABLE_UNITY_IAP_V5
        private void InitializeStoreControllerV5(Action onInitializedCallback)
        {
            if (_storeController != null)
            {
                UnsubscribeStoreControllerEvents();
            }

            _storeController = UnityIAPServices.StoreController();
            int maxRetries = Mathf.Max(1, _data.MaxRetryAttempts);
            _storeController.SetStoreReconnectionRetryPolicyOnDisconnection(new MaximumNumberOfAttemptsRetryPolicy(maxRetries));
            _storeController.ProcessPendingOrdersOnPurchasesFetched(true);

            _catalogProvider = BuildCatalogProvider();

            SubscribeStoreControllerEvents(onInitializedCallback);
        }

        private CatalogProvider BuildCatalogProvider()
        {
            var provider = _data.UseCodelessCatalog
                ? CodelessCatalogProvider.PopulateCatalogProvider(ProductCatalog.LoadDefaultCatalog())
                : new CatalogProvider();

            var existingIds = new HashSet<string>(
                provider.GetProducts().Select(p => p.id),
                StringComparer.Ordinal
            );

            if (_data.Products != null)
            {
                for (int i = 0; i < _data.Products.Count; i++)
                {
                    var entry = _data.Products[i];
                    if (entry == null || string.IsNullOrEmpty(entry.productId)) continue;

                    // Populate fallback metadata immediately before store fetch finishes
                    CacheFallbackMetadata(entry);

                    if (existingIds.Contains(entry.productId)) continue;

                    var storeSpecificIds = new StoreSpecificIds();
                    bool hasStoreOverride = false;

                    if (!string.IsNullOrEmpty(entry.googlePlayId))
                    {
                        storeSpecificIds.Add(entry.googlePlayId, GooglePlay.Name);
                        hasStoreOverride = true;
                    }

                    if (!string.IsNullOrEmpty(entry.appleAppStoreId))
                    {
                        storeSpecificIds.Add(entry.appleAppStoreId, AppleAppStore.Name);
                        hasStoreOverride = true;
                    }

                    var unityType = ConvertToUnityProductType(entry.productType);
                    provider.AddProduct(entry.productId, unityType, hasStoreOverride ? storeSpecificIds : null);
                    existingIds.Add(entry.productId);
                }
            }

            // Ensure codeless catalog products are also registered in UnityIAPData lookup & fallback metadata
            if (_data.UseCodelessCatalog)
            {
                var defaultCatalog = ProductCatalog.LoadDefaultCatalog();
                if (defaultCatalog != null && defaultCatalog.allProducts != null)
                {
                    foreach (var catItem in defaultCatalog.allProducts)
                    {
                        if (catItem == null || string.IsNullOrEmpty(catItem.id)) continue;

                        if (_data.GetProductEntry(catItem.id) == null)
                        {
                            var autoEntry = new UnityIAPProductEntry
                            {
                                productId = catItem.id,
                                productType = ConvertFromUnityProductType(catItem.type),
                                defaultTitle = catItem.defaultDescription?.Title ?? catItem.id,
                                defaultDescription = catItem.defaultDescription?.Description ?? "",
                                defaultPriceString = catItem.googlePrice?.value > 0 ? $"${catItem.googlePrice.value:0.00}" : "$0.99"
                            };
                            _data.AddOrUpdateProduct(autoEntry);
                            CacheFallbackMetadata(autoEntry);
                        }
                    }
                }
            }

            return provider;
        }

        private void SubscribeStoreControllerEvents(Action onInitializedCallback)
        {
            _storeController.OnStoreConnected += HandleStoreConnected;
            _storeController.OnStoreDisconnected += HandleStoreDisconnected;
            _storeController.OnProductsFetched += products => HandleProductsFetched(products, onInitializedCallback);
            _storeController.OnProductsFetchFailed += HandleProductsFetchFailed;
            _storeController.OnPurchasesFetched += HandlePurchasesFetched;
            _storeController.OnPurchasesFetchFailed += HandlePurchasesFetchFailed;
            _storeController.OnPurchasePending += HandlePurchasePending;
            _storeController.OnPurchaseConfirmed += HandlePurchaseConfirmed;
            _storeController.OnPurchaseFailed += HandlePurchaseFailed;
            _storeController.OnPurchaseDeferred += HandlePurchaseDeferred;
            _storeController.OnCheckEntitlement += HandleCheckEntitlement;
        }

        private void UnsubscribeStoreControllerEvents()
        {
            if (_storeController == null) return;

            _storeController.OnStoreConnected -= HandleStoreConnected;
            _storeController.OnStoreDisconnected -= HandleStoreDisconnected;
            _storeController.OnProductsFetchFailed -= HandleProductsFetchFailed;
            _storeController.OnPurchasesFetched -= HandlePurchasesFetched;
            _storeController.OnPurchasesFetchFailed -= HandlePurchasesFetchFailed;
            _storeController.OnPurchasePending -= HandlePurchasePending;
            _storeController.OnPurchaseConfirmed -= HandlePurchaseConfirmed;
            _storeController.OnPurchaseFailed -= HandlePurchaseFailed;
            _storeController.OnPurchaseDeferred -= HandlePurchaseDeferred;
            _storeController.OnCheckEntitlement -= HandleCheckEntitlement;
        }

        private void HandleStoreConnected()
        {
            string storeName = UnityIAPServices.GetDefaultStore();
            var definitions = _catalogProvider.GetProducts(storeName);

            if (definitions == null || definitions.Count == 0)
            {
                CompleteInitialization(null);
                return;
            }

            int maxRetries = Mathf.Max(1, _data.MaxRetryAttempts);
            _storeController.FetchProducts(definitions, new MaximumNumberOfAttemptsRetryPolicy(maxRetries));
        }

        private void HandleStoreDisconnected(StoreConnectionFailureDescription failure)
        {
            string message = failure?.Message ?? "Store disconnected";
            Debug.LogWarning($"[UnityIAPManager] Store disconnected: {message}");

            if (!_isInitialized)
            {
                _isInitializing = false;
                _funnelTracker.TrackInitFailed(message);
                OnInitializeFailed?.Invoke(message);
            }
        }

        private void HandleProductsFetched(List<Product> products, Action onInitializedCallback)
        {
            if (products != null)
            {
                for (int i = 0; i < products.Count; i++)
                {
                    var product = products[i];
                    if (product?.definition == null) continue;

                    string id = product.definition.id;
                    bool isOwned = IsProductOwned(id);
                    var entry = _data.GetProductEntry(id);

                    var metadata = new UnityIAPProductMetadata
                    {
                        ProductId = id,
                        ProductType = ConvertFromUnityProductType(product.definition.type),
                        LocalizedPriceString = !string.IsNullOrEmpty(product.metadata?.localizedPriceString)
                            ? product.metadata.localizedPriceString
                            : (entry?.defaultPriceString ?? "$0.99"),
                        LocalizedPrice = product.metadata?.localizedPrice ?? 0m,
                        IsoCurrencyCode = !string.IsNullOrEmpty(product.metadata?.isoCurrencyCode)
                            ? product.metadata.isoCurrencyCode
                            : "USD",
                        LocalizedTitle = !string.IsNullOrEmpty(product.metadata?.localizedTitle)
                            ? product.metadata.localizedTitle
                            : (entry?.defaultTitle ?? id),
                        LocalizedDescription = !string.IsNullOrEmpty(product.metadata?.localizedDescription)
                            ? product.metadata.localizedDescription
                            : (entry?.defaultDescription ?? ""),
                        IsAvailableToPurchase = product.availableToPurchase,
                        IsOwned = isOwned
                    };

                    _productMetadataMap[id] = metadata;
                }
            }

            if (!_initialProductsFetched)
            {
                _initialProductsFetched = true;
                _storeController.FetchPurchases();
                CompleteInitialization(onInitializedCallback);
            }

            OnProductsFetched?.Invoke(GetAllProducts());
        }

        private void HandleProductsFetchFailed(ProductFetchFailed failure)
        {
            string reason = failure?.FailureReason ?? "Products fetch failed";
            Debug.LogWarning($"[UnityIAPManager] Products fetch failed: {reason}");

            if (!_isInitialized)
            {
                _isInitializing = false;
                _funnelTracker.TrackInitFailed(reason);
                OnInitializeFailed?.Invoke(reason);
            }
        }

        private void HandlePurchasesFetched(Orders existingOrders)
        {
            if (existingOrders == null) return;

            if (existingOrders.ConfirmedOrders != null)
            {
                foreach (var confirmedOrder in existingOrders.ConfirmedOrders)
                {
                    ProcessOrderEntitlements(confirmedOrder, isRestoredOrFetched: true);
                }
            }

            if (existingOrders.PendingOrders != null)
            {
                foreach (var pendingOrder in existingOrders.PendingOrders)
                {
                    ProcessOrderEntitlements(pendingOrder, isRestoredOrFetched: true);
                    if (_data.AutoConfirmPurchases)
                    {
                        _storeController.ConfirmPurchase(pendingOrder);
                    }
                }
            }

            ResolveCrossProductOwnership();
        }

        private void HandlePurchasesFetchFailed(PurchasesFetchFailureDescription failure)
        {
            Debug.LogWarning($"[UnityIAPManager] Existing purchases fetch failed: {failure?.Message}");
        }

        private void HandlePurchasePending(PendingOrder pendingOrder)
        {
            if (pendingOrder?.CartOrdered?.Items() == null) return;

            foreach (var cartItem in pendingOrder.CartOrdered.Items())
            {
                var product = cartItem?.Product;
                if (product?.definition == null) continue;

                string productId = product.definition.id;
                var productType = ConvertFromUnityProductType(product.definition.type);

                if (productType == UnityIAPProductType.NonConsumable || productType == UnityIAPProductType.Subscription)
                {
                    MarkProductOwned(productId);
                    ResolveCrossProductOwnership();
                }

                string placement = "";
                if (_pendingPurchaseCallbacks.TryGetValue(productId, out var pendingCallback))
                {
                    placement = pendingCallback.Placement ?? "";
                }

                var result = new UnityIAPPurchaseResult
                {
                    ProductId = productId,
                    ProductType = productType,
                    TransactionId = pendingOrder.Info?.TransactionID ?? "",
                    Receipt = pendingOrder.Info?.Receipt ?? "",
                    LocalizedPrice = product.metadata?.localizedPrice ?? 0m,
                    IsoCurrencyCode = product.metadata?.isoCurrencyCode ?? "USD",
                    IsRestored = false,
                    Placement = placement
                };

                _funnelTracker.TrackPurchaseSuccess(
                    productId,
                    productType.ToString(),
                    result.TransactionId,
                    result.LocalizedPrice,
                    result.IsoCurrencyCode,
                    isRestored: false
                );

                if (_pendingPurchaseCallbacks.TryGetValue(productId, out var cb))
                {
                    _pendingPurchaseCallbacks.Remove(productId);
                    cb.OnSuccess?.Invoke(result);
                }

                OnPurchaseSucceeded?.Invoke(result);
            }

            if (_data.AutoConfirmPurchases)
            {
                _storeController.ConfirmPurchase(pendingOrder);
            }
        }

        private void HandlePurchaseConfirmed(Order order)
        {
            if (order is FailedOrder failedOrder)
            {
                HandlePurchaseFailed(failedOrder);
            }
        }

        private void HandlePurchaseFailed(FailedOrder failedOrder)
        {
            string productId = ExtractFirstProductId(failedOrder);
            var reason = ConvertFailureReason(failedOrder != null ? failedOrder.FailureReason : PurchaseFailureReason.Unknown);
            string details = failedOrder?.Details ?? reason.ToString();

            DispatchPurchaseFailure(productId, reason, details);
        }

        private void HandlePurchaseDeferred(DeferredOrder deferredOrder)
        {
            string productId = ExtractFirstProductId(deferredOrder);
            if (!string.IsNullOrEmpty(productId))
            {
                _funnelTracker.TrackPurchaseDeferred(productId);
                OnPurchaseDeferred?.Invoke(productId);
            }
        }

        private void HandleCheckEntitlement(Entitlement entitlement)
        {
            if (entitlement?.Product?.definition == null) return;

            string productId = entitlement.Product.definition.id;
            if (entitlement.Status == EntitlementStatus.FullyEntitled ||
                entitlement.Status == EntitlementStatus.EntitledButNotFinished)
            {
                MarkProductOwned(productId);
                ResolveCrossProductOwnership();
            }
        }

        private void ProcessOrderEntitlements(Order order, bool isRestoredOrFetched)
        {
            if (order?.CartOrdered?.Items() == null) return;

            foreach (var cartItem in order.CartOrdered.Items())
            {
                var product = cartItem?.Product;
                if (product?.definition == null) continue;

                string productId = product.definition.id;
                var productType = ConvertFromUnityProductType(product.definition.type);

                if (productType == UnityIAPProductType.NonConsumable || productType == UnityIAPProductType.Subscription)
                {
                    bool wasAlreadyOwned = IsProductOwned(productId);
                    MarkProductOwned(productId);

                    if (!wasAlreadyOwned && isRestoredOrFetched)
                    {
                        var restoredResult = new UnityIAPPurchaseResult
                        {
                            ProductId = productId,
                            ProductType = productType,
                            TransactionId = order.Info?.TransactionID ?? "",
                            Receipt = order.Info?.Receipt ?? "",
                            LocalizedPrice = product.metadata?.localizedPrice ?? 0m,
                            IsoCurrencyCode = product.metadata?.isoCurrencyCode ?? "USD",
                            IsRestored = true,
                            Placement = "restore"
                        };
                        OnPurchaseSucceeded?.Invoke(restoredResult);
                    }
                }
            }
        }

        private static string ExtractFirstProductId(Order order)
        {
            if (order?.CartOrdered?.Items() == null) return "";
            var firstItem = order.CartOrdered.Items().FirstOrDefault();
            return firstItem?.Product?.definition?.id ?? "";
        }

        private static ProductType ConvertToUnityProductType(UnityIAPProductType type)
        {
            return type switch
            {
                UnityIAPProductType.Consumable => ProductType.Consumable,
                UnityIAPProductType.NonConsumable => ProductType.NonConsumable,
                UnityIAPProductType.Subscription => ProductType.Subscription,
                _ => ProductType.Consumable
            };
        }

        private static UnityIAPProductType ConvertFromUnityProductType(ProductType type)
        {
            return type switch
            {
                ProductType.Consumable => UnityIAPProductType.Consumable,
                ProductType.NonConsumable => UnityIAPProductType.NonConsumable,
                ProductType.Subscription => UnityIAPProductType.Subscription,
                _ => UnityIAPProductType.Unknown
            };
        }

        private static UnityIAPPurchaseFailureReason ConvertFailureReason(PurchaseFailureReason reason)
        {
            return reason switch
            {
                PurchaseFailureReason.PurchasingUnavailable => UnityIAPPurchaseFailureReason.PurchasingUnavailable,
                PurchaseFailureReason.ExistingPurchasePending => UnityIAPPurchaseFailureReason.ExistingPurchasePending,
                PurchaseFailureReason.ProductUnavailable => UnityIAPPurchaseFailureReason.ProductUnavailable,
                PurchaseFailureReason.SignatureInvalid => UnityIAPPurchaseFailureReason.SignatureInvalid,
                PurchaseFailureReason.UserCancelled => UnityIAPPurchaseFailureReason.UserCancelled,
                PurchaseFailureReason.PaymentDeclined => UnityIAPPurchaseFailureReason.PaymentDeclined,
                PurchaseFailureReason.DuplicateTransaction => UnityIAPPurchaseFailureReason.DuplicateTransaction,
                _ => UnityIAPPurchaseFailureReason.Unknown
            };
        }
#endif
        #endregion

        #region Purchase & Restore API
        /// <summary>
        /// Initiates an in-app purchase for the given product ID.
        /// </summary>
        public void Purchase(
            string productId,
            UnityAction<UnityIAPPurchaseResult> onSuccess = null,
            UnityAction<UnityIAPPurchaseFailure> onFailed = null,
            string placement = "")
        {
            if (string.IsNullOrEmpty(productId))
            {
                var emptyFail = new UnityIAPPurchaseFailure
                {
                    ProductId = "",
                    Reason = UnityIAPPurchaseFailureReason.ProductUnavailable,
                    Message = "Product ID cannot be null or empty.",
                    Placement = placement
                };
                onFailed?.Invoke(emptyFail);
                OnPurchaseFailed?.Invoke(emptyFail);
                return;
            }

            if (_pendingPurchaseCallbacks.ContainsKey(productId))
            {
                var pendingFail = new UnityIAPPurchaseFailure
                {
                    ProductId = productId,
                    Reason = UnityIAPPurchaseFailureReason.ExistingPurchasePending,
                    Message = $"A purchase for '{productId}' is already in progress.",
                    Placement = placement
                };
                onFailed?.Invoke(pendingFail);
                OnPurchaseFailed?.Invoke(pendingFail);
                return;
            }

            var meta = GetProductMetadata(productId);
            if ((meta.ProductType == UnityIAPProductType.NonConsumable || meta.ProductType == UnityIAPProductType.Subscription) &&
                IsProductOwned(productId))
            {
                var ownedResult = new UnityIAPPurchaseResult
                {
                    ProductId = productId,
                    ProductType = meta.ProductType,
                    TransactionId = "already_owned",
                    Receipt = "",
                    LocalizedPrice = meta.LocalizedPrice,
                    IsoCurrencyCode = meta.IsoCurrencyCode,
                    IsRestored = true,
                    Placement = placement
                };
                onSuccess?.Invoke(ownedResult);
                OnPurchaseSucceeded?.Invoke(ownedResult);
                return;
            }

            _funnelTracker.TrackPurchaseStart(productId, meta.ProductType.ToString(), placement);

            if (Application.isEditor && _data.SimulateSuccessInEditor && !_isInitialized)
            {
                SimulateEditorPurchaseSuccess(productId, meta, onSuccess, placement);
                return;
            }

            if (!_isInitialized)
            {
                DispatchPurchaseFailure(
                    productId,
                    UnityIAPPurchaseFailureReason.PurchasingUnavailable,
                    "Store is not initialized yet.",
                    onFailed,
                    placement
                );
                return;
            }

#if ENABLE_UNITY_IAP_V5
            var product = _storeController?.GetProductById(productId);
            if (product == null || !product.availableToPurchase)
            {
                if (Application.isEditor && _data.SimulateSuccessInEditor)
                {
                    SimulateEditorPurchaseSuccess(productId, meta, onSuccess, placement);
                    return;
                }

                DispatchPurchaseFailure(
                    productId,
                    UnityIAPPurchaseFailureReason.ProductUnavailable,
                    $"Product '{productId}' is not available for purchase in the store.",
                    onFailed,
                    placement
                );
                return;
            }

            _pendingPurchaseCallbacks[productId] = new PendingPurchaseCallback
            {
                OnSuccess = onSuccess,
                OnFailed = onFailed,
                Placement = placement
            };

            try
            {
                _storeController.PurchaseProduct(product);
            }
            catch (Exception ex)
            {
                DispatchPurchaseFailure(productId, UnityIAPPurchaseFailureReason.Unknown, ex.Message);
            }
#else
            SimulateEditorPurchaseSuccess(productId, meta, onSuccess, placement);
#endif
        }

        /// <summary>
        /// Asynchronously purchases a product and awaits its completion or failure.
        /// </summary>
        public async UniTask<(bool success, UnityIAPPurchaseResult result, UnityIAPPurchaseFailure failure)> PurchaseAsync(
            string productId,
            string placement = "")
        {
            var tcs = new UniTaskCompletionSource<(bool, UnityIAPPurchaseResult, UnityIAPPurchaseFailure)>();
            Purchase(
                productId,
                onSuccess: res => tcs.TrySetResult((true, res, default)),
                onFailed: fail => tcs.TrySetResult((false, default, fail)),
                placement: placement
            );
            return await tcs.Task;
        }

        /// <summary>
        /// Restores previously purchased non-consumable and subscription transactions from the store.
        /// </summary>
        public void RestorePurchases(UnityAction<bool, string> onCompleted = null)
        {
            _funnelTracker.TrackRestoreStart();

            if (!_isInitialized)
            {
                string err = "Cannot restore purchases: Store is not initialized.";
                _funnelTracker.TrackRestoreComplete(false, err);
                onCompleted?.Invoke(false, err);
                OnPurchasesRestored?.Invoke(false, err);
                return;
            }

#if ENABLE_UNITY_IAP_V5
            _storeController.RestoreTransactions((success, error) =>
            {
                if (success)
                {
                    _storeController.FetchPurchases();
                    ResolveCrossProductOwnership();
                }

                _funnelTracker.TrackRestoreComplete(success, error);
                onCompleted?.Invoke(success, error);
                OnPurchasesRestored?.Invoke(success, error);
            });
#else
            ResolveCrossProductOwnership();
            _funnelTracker.TrackRestoreComplete(true, null);
            onCompleted?.Invoke(true, null);
            OnPurchasesRestored?.Invoke(true, null);
#endif
        }

        /// <summary>
        /// Asynchronously restores previously purchased transactions.
        /// </summary>
        public async UniTask<bool> RestorePurchasesAsync()
        {
            var tcs = new UniTaskCompletionSource<bool>();
            RestorePurchases((success, _) => tcs.TrySetResult(success));
            return await tcs.Task;
        }
        #endregion

        #region Ownership & Cross-Product Dependency Resolution
        /// <summary>
        /// Returns true if the specified non-consumable or subscription product is owned
        /// either directly or via a cross-product bundle rule (OwnedIfOwnedProducts).
        /// </summary>
        public bool IsProductOwned(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return false;
            return _ownedProducts.TryGetValue(productId, out bool owned) && owned;
        }

        /// <summary>
        /// Returns true if the product is fetched and available for purchase.
        /// </summary>
        public bool IsProductAvailable(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return false;
            return _productMetadataMap.TryGetValue(productId, out var meta) && meta.IsAvailableToPurchase;
        }

        /// <summary>
        /// Retrieves the localized metadata and ownership state for a product.
        /// </summary>
        public UnityIAPProductMetadata GetProductMetadata(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return default;

            if (_productMetadataMap.TryGetValue(productId, out var meta))
            {
                meta.IsOwned = IsProductOwned(productId);
                return meta;
            }

            var entry = _data.GetProductEntry(productId);
            if (entry != null)
            {
                CacheFallbackMetadata(entry);
                return _productMetadataMap[productId];
            }

            return new UnityIAPProductMetadata
            {
                ProductId = productId,
                ProductType = UnityIAPProductType.Unknown,
                LocalizedPriceString = "$0.99",
                IsoCurrencyCode = "USD",
                LocalizedTitle = productId,
                IsAvailableToPurchase = false,
                IsOwned = IsProductOwned(productId)
            };
        }

        /// <summary>
        /// Returns all known product metadata entries.
        /// </summary>
        public IReadOnlyList<UnityIAPProductMetadata> GetAllProducts()
        {
            var list = new List<UnityIAPProductMetadata>(_productMetadataMap.Count);
            foreach (var kvp in _productMetadataMap)
            {
                var item = kvp.Value;
                item.IsOwned = IsProductOwned(kvp.Key);
                list.Add(item);
            }
            return list;
        }

        /// <summary>
        /// Returns the localized price string (e.g. "$0.99") for a product, or fallback if unavailable.
        /// </summary>
        public string GetLocalizedPriceString(string productId, string fallback = "")
        {
            var meta = GetProductMetadata(productId);
            if (!string.IsNullOrEmpty(meta.LocalizedPriceString))
            {
                return meta.LocalizedPriceString;
            }
            return !string.IsNullOrEmpty(fallback) ? fallback : "$0.99";
        }

        /// <summary>
        /// Evaluates cross-product bundle ownership rules (OwnedIfOwnedProducts) transitively.
        /// If a bundle is owned, all dependent child products are automatically marked as owned.
        /// </summary>
        public void ResolveCrossProductOwnership()
        {
            if (_data?.Products == null) return;

            bool changed;
            do
            {
                changed = false;
                for (int i = 0; i < _data.Products.Count; i++)
                {
                    var item = _data.Products[i];
                    if (item == null || string.IsNullOrEmpty(item.productId)) continue;
                    if (IsProductOwned(item.productId)) continue;
                    if (item.ownedIfOwnedProducts == null || item.ownedIfOwnedProducts.Length == 0) continue;

                    for (int j = 0; j < item.ownedIfOwnedProducts.Length; j++)
                    {
                        string parentBundleId = item.ownedIfOwnedProducts[j];
                        if (!string.IsNullOrEmpty(parentBundleId) && IsProductOwned(parentBundleId))
                        {
                            MarkProductOwned(item.productId);
                            changed = true;
                            break;
                        }
                    }
                }
            } while (changed);
        }

        public void MarkProductOwned(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return;

            bool alreadyOwned = IsProductOwned(productId);
            _ownedProducts[productId] = true;

            if (_productMetadataMap.TryGetValue(productId, out var meta))
            {
                meta.IsOwned = true;
                _productMetadataMap[productId] = meta;
            }

            var entry = _data.GetProductEntry(productId);
            if (_data.PersistOwnershipToPlayerPrefs)
            {
                PlayerPrefs.SetInt(_data.GetOwnershipPrefKey(productId), 1);

                if (entry?.refPlayerPrefs != null)
                {
                    for (int i = 0; i < entry.refPlayerPrefs.Length; i++)
                    {
                        string prefKey = entry.refPlayerPrefs[i];
                        if (!string.IsNullOrEmpty(prefKey))
                        {
                            PlayerPrefs.SetInt(prefKey, 1);
                        }
                    }
                }

                PlayerPrefs.Save();
            }

            if (!alreadyOwned)
            {
                OnOwnershipChanged?.Invoke(productId, true);
            }
        }

        private void LoadCachedOwnershipFromPrefs()
        {
            if (_data?.Products == null) return;

            for (int i = 0; i < _data.Products.Count; i++)
            {
                var entry = _data.Products[i];
                if (entry == null || string.IsNullOrEmpty(entry.productId)) continue;

                CacheFallbackMetadata(entry);

                if (_data.PersistOwnershipToPlayerPrefs)
                {
                    bool ownedInPrefs = PlayerPrefs.GetInt(_data.GetOwnershipPrefKey(entry.productId), 0) == 1;
                    if (!ownedInPrefs && entry.refPlayerPrefs != null)
                    {
                        for (int j = 0; j < entry.refPlayerPrefs.Length; j++)
                        {
                            string refKey = entry.refPlayerPrefs[j];
                            if (!string.IsNullOrEmpty(refKey) && PlayerPrefs.GetInt(refKey, 0) == 1)
                            {
                                ownedInPrefs = true;
                                break;
                            }
                        }
                    }

                    if (ownedInPrefs)
                    {
                        _ownedProducts[entry.productId] = true;
                        var meta = _productMetadataMap[entry.productId];
                        meta.IsOwned = true;
                        _productMetadataMap[entry.productId] = meta;
                    }
                }
            }

            ResolveCrossProductOwnership();
        }

        private void CacheFallbackMetadata(UnityIAPProductEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.productId)) return;
            if (_productMetadataMap.ContainsKey(entry.productId)) return;

            _productMetadataMap[entry.productId] = new UnityIAPProductMetadata
            {
                ProductId = entry.productId,
                ProductType = entry.productType,
                LocalizedPriceString = !string.IsNullOrEmpty(entry.defaultPriceString) ? entry.defaultPriceString : "$0.99",
                LocalizedPrice = 0.99m,
                IsoCurrencyCode = "USD",
                LocalizedTitle = !string.IsNullOrEmpty(entry.defaultTitle) ? entry.defaultTitle : entry.productId,
                LocalizedDescription = entry.defaultDescription ?? "",
                IsAvailableToPurchase = true,
                IsOwned = IsProductOwned(entry.productId)
            };
        }
        #endregion

        #region Internal Helpers & Simulation
        private void CompleteInitialization(Action onInitializedCallback)
        {
            _isInitializing = false;
            _isInitialized = true;
            ResolveCrossProductOwnership();
            _funnelTracker.TrackInitialized(_productMetadataMap.Count);
            onInitializedCallback?.Invoke();
            OnInitialized?.Invoke();
        }

        private void SimulateOfflineInitialization(Action onInitializedCallback)
        {
            CompleteInitialization(onInitializedCallback);
            OnProductsFetched?.Invoke(GetAllProducts());
        }

        private void SimulateEditorPurchaseSuccess(
            string productId,
            UnityIAPProductMetadata meta,
            UnityAction<UnityIAPPurchaseResult> onSuccess,
            string placement)
        {
            if (meta.ProductType == UnityIAPProductType.NonConsumable || meta.ProductType == UnityIAPProductType.Subscription)
            {
                MarkProductOwned(productId);
                ResolveCrossProductOwnership();
            }

            var simulatedResult = new UnityIAPPurchaseResult
            {
                ProductId = productId,
                ProductType = meta.ProductType,
                TransactionId = $"editor_sim_{Guid.NewGuid():N}",
                Receipt = "{\"Store\":\"FakeStore\"}",
                LocalizedPrice = meta.LocalizedPrice,
                IsoCurrencyCode = !string.IsNullOrEmpty(meta.IsoCurrencyCode) ? meta.IsoCurrencyCode : "USD",
                IsRestored = false,
                Placement = placement
            };

            _funnelTracker.TrackPurchaseSuccess(
                productId,
                meta.ProductType.ToString(),
                simulatedResult.TransactionId,
                simulatedResult.LocalizedPrice,
                simulatedResult.IsoCurrencyCode,
                isRestored: false
            );

            onSuccess?.Invoke(simulatedResult);
            OnPurchaseSucceeded?.Invoke(simulatedResult);
        }

        private void DispatchPurchaseFailure(
            string productId,
            UnityIAPPurchaseFailureReason reason,
            string message,
            UnityAction<UnityIAPPurchaseFailure> directFailCallback = null,
            string directPlacement = "")
        {
            string placement = directPlacement;
            UnityAction<UnityIAPPurchaseFailure> callback = directFailCallback;

            if (!string.IsNullOrEmpty(productId) && _pendingPurchaseCallbacks.TryGetValue(productId, out var pending))
            {
                _pendingPurchaseCallbacks.Remove(productId);
                if (callback == null) callback = pending.OnFailed;
                if (string.IsNullOrEmpty(placement)) placement = pending.Placement;
            }

            var failure = new UnityIAPPurchaseFailure
            {
                ProductId = productId ?? "",
                Reason = reason,
                Message = message ?? reason.ToString(),
                Placement = placement ?? ""
            };

            _funnelTracker.TrackPurchaseFailed(productId, reason.ToString(), failure.Message);
            callback?.Invoke(failure);
            OnPurchaseFailed?.Invoke(failure);
        }
        #endregion
    }
}
