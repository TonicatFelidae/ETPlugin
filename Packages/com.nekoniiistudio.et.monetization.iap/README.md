# ET Monetization - In-App Purchasing (Unity IAP v5+)

Package name: `com.nekoniiistudio.et.monetization.iap`  
Namespace: `ET.Monetization`

A modern, industry-standard In-App Purchasing package built for **Unity IAP v5+ (`StoreController` API)**, designed for seamless integration into any Unity project as a Git package or local UPM package, and completely decoupled from `ETEngine` core via `IIAPService` and `IIAPProvider`.

---

## Features
- **Modern Unity IAP v5+ Architecture**: Built on `UnityIAPServices.StoreController()` (`IStoreService`, `IProductService`, `IPurchaseService`, `PendingOrder`, `ConfirmedOrder`, `Orders`, `Entitlement`) with zero obsolete v4 API calls.
- **Cross-Product Bundle Ownership (`ownedIfOwnedProducts`)**: Automatically marks dependent child products (e.g. `remove_ads`) as owned when a parent bundle (e.g. `full_pack_remove_ads`) is purchased or restored.
- **Offline Ownership Persistence (`refPlayerPrefs`)**: Caches Non-Consumable and Subscription ownership in `PlayerPrefs` for instantaneous frame-0 checks before the store finishes connecting.
- **Codeless + Code-Driven Catalog**: Automatically merges Unity's `Resources/IAPProductCatalog.json` with code-defined `UnityIAPProductEntry` configurations and store-specific ID overrides.
- **Reactive UI Component (`ETIAPButton`)**: Event-driven localized price, title, description, and owned visual state updates with Inspector `TouchPurchaseButton()` and `TouchRestoreButton()` handlers (no race conditions on startup).
- **Decoupled Telemetry**: `IAPAnalytics.OnLogEvent` and `IAPFunnelTracker` track initialization latency, purchase funnel conversion, revenue, currency, and restore events.
- **ETEngine Abstract Compatibility**: Integrates seamlessly with ETEngine's store-agnostic `IIAPService` via `GenericIAPProvider<UnityIAPManager>`.

---

## Quick Start (Standalone)

### 1. Initialize `UnityIAPManager`
```csharp
using System.Collections.Generic;
using ET.Monetization;
using UnityEngine;

public class IAPBootstrap : MonoBehaviour
{
    private UnityIAPManager _iapManager;

    private void Start()
    {
        var config = new UnityIAPData
        {
            AutoInitialize = true,
            UseCodelessCatalog = true,
            PersistOwnershipToPlayerPrefs = true,
            Products = new List<UnityIAPProductEntry>
            {
                new UnityIAPProductEntry
                {
                    productId = "remove_ads",
                    productType = UnityIAPProductType.NonConsumable,
                    defaultPriceString = "$1.99",
                    refPlayerPrefs = new[] { "NO_ADS" },
                    ownedIfOwnedProducts = new[] { "full_pack_remove_ads" }
                },
                new UnityIAPProductEntry
                {
                    productId = "full_pack_remove_ads",
                    productType = UnityIAPProductType.NonConsumable,
                    defaultPriceString = "$4.99",
                    refPlayerPrefs = new[] { "NO_ADS", "ULTIMATE_PACK" }
                }
            }
        };

        _iapManager = new UnityIAPManager(config);
    }
}
```

### 2. Purchase a Product
```csharp
_iapManager.Purchase(
    productId: "remove_ads",
    onSuccess: result => Debug.Log($"Purchased {result.ProductId} (Tx: {result.TransactionId})"),
    onFailed: failure => Debug.LogWarning($"Failed {failure.ProductId}: {failure.Reason} - {failure.Message}"),
    placement: "shop_popup"
);
```

### 3. Restore Purchases
```csharp
_iapManager.RestorePurchases((success, error) =>
{
    Debug.Log($"Restore completed: {success}");
});
```

---

## Integration with ETEngine (`IIAPService` & `GenericIAPProvider`)

`ETEngine` has **zero** dependency on `UnityEngine.Purchasing`. Bridge `UnityIAPManager` to `ETEngine` using `GenericIAPProvider<UnityIAPManager>`:

```csharp
using System.Collections.Generic;
using ET.Monetization;
using UnityEngine.Events;

public class UnityIAPProvider : GenericIAPProvider<UnityIAPManager>
{
    public UnityIAPProvider(UnityIAPManager manager) : base(manager)
    {
        _manager.OnInitialized += DispatchInitialized;
        _manager.OnInitializeFailed += DispatchInitializeFailed;
        _manager.OnProductsFetched += _ => DispatchProductsFetched(GetAllProducts());
        _manager.OnPurchaseSucceeded += r => DispatchPurchaseSucceeded(ConvertResult(r));
        _manager.OnPurchaseFailed += f => DispatchPurchaseFailed(ConvertFailure(f));
        _manager.OnPurchaseDeferred += DispatchPurchaseDeferred;
        _manager.OnPurchasesRestored += DispatchPurchasesRestored;
        _manager.OnOwnershipChanged += DispatchOwnershipChanged;
    }

    public override bool IsInitialized => _manager.IsInitialized;
    public override void Initialize() => _manager.Initialize();

    public override void Purchase(string productId, UnityAction<IAPPurchaseResult> onSuccess = null, UnityAction<IAPPurchaseFailure> onFailed = null, string placement = "")
    {
        _manager.Purchase(
            productId,
            onSuccess: r => onSuccess?.Invoke(ConvertResult(r)),
            onFailed: f => onFailed?.Invoke(ConvertFailure(f)),
            placement: placement
        );
    }

    public override void RestorePurchases(UnityAction<bool, string> onCompleted = null)
        => _manager.RestorePurchases(onCompleted);

    public override bool IsProductOwned(string productId) => _manager.IsProductOwned(productId);
    public override bool IsProductAvailable(string productId) => _manager.IsProductAvailable(productId);
    public override IAPProductInfo GetProductInfo(string productId) => ConvertMeta(_manager.GetProductMetadata(productId));

    public override IReadOnlyList<IAPProductInfo> GetAllProducts()
    {
        var raw = _manager.GetAllProducts();
        var list = new List<IAPProductInfo>(raw.Count);
        for (int i = 0; i < raw.Count; i++) list.Add(ConvertMeta(raw[i]));
        return list;
    }

    private static IAPProductInfo ConvertMeta(UnityIAPProductMetadata m) => new IAPProductInfo
    {
        ProductId = m.ProductId,
        ProductType = (IAPProductType)m.ProductType,
        LocalizedPriceString = m.LocalizedPriceString,
        LocalizedPrice = m.LocalizedPrice,
        IsoCurrencyCode = m.IsoCurrencyCode,
        LocalizedTitle = m.LocalizedTitle,
        LocalizedDescription = m.LocalizedDescription,
        IsAvailableToPurchase = m.IsAvailableToPurchase,
        IsOwned = m.IsOwned
    };

    private static IAPPurchaseResult ConvertResult(UnityIAPPurchaseResult r) => new IAPPurchaseResult
    {
        ProductId = r.ProductId,
        ProductType = (IAPProductType)r.ProductType,
        TransactionId = r.TransactionId,
        Receipt = r.Receipt,
        LocalizedPrice = r.LocalizedPrice,
        IsoCurrencyCode = r.IsoCurrencyCode,
        IsRestored = r.IsRestored,
        Placement = r.Placement
    };

    private static IAPPurchaseFailure ConvertFailure(UnityIAPPurchaseFailure f) => new IAPPurchaseFailure
    {
        ProductId = f.ProductId,
        Reason = (IAPPurchaseFailureReason)f.Reason,
        Message = f.Message,
        Placement = f.Placement
    };
}
```

### Register in VContainer `LifetimeScope`:
```csharp
builder.RegisterInstance(new UnityIAPManager(iapData));
builder.Register<UnityIAPProvider>(Lifetime.Singleton).As<IIAPProvider>();
builder.Register<GenericIAPService<UnityIAPProvider>>(Lifetime.Singleton).As<IIAPService>();
```
