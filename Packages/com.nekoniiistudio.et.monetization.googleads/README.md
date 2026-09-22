# ET Monetization - Google Mobile Ads (AdMob)

Package name: `com.nekoniiistudio.et.monetization.googleads`  
Namespace: `ET.Monetization`

A production-ready, modular Google Mobile Ads (AdMob) package designed for seamless integration into any Unity project as a Git package or local UPM package.

---

## Features
- **Modern GMA SDK Support**: Built for Google Mobile Ads Unity SDK v11.x+ API.
- **Full Format Coverage**:
  - Anchored Adaptive & Standard Banners (`BannerView`)
  - Interstitial Ads (`InterstitialAd`)
  - Rewarded Ads (`RewardedAd`)
  - Rewarded Interstitial Ads (`RewardedInterstitialAd`)
  - App Open Ads (`AppOpenAd`)
- **Official Test IDs Built-in**: Verified Google test IDs for Android and iOS available via `TestAdUnits`.
- **Safe Area UI Integration**: `GoogleAdsSafeArea` automatically adjusts UI layout when banners load or dismiss.
- **Decoupled Telemetry**: `GoogleAdsAnalytics.OnLogEvent` and `GoogleAdsFunnelTracker` track latencies, impressions, and show requests without hard dependencies on any specific analytics provider.
- **ETEngine Abstract Compatibility**: Integrates seamlessly with ETEngine's `IMonetizationService` via `GenericAdProvider<GoogleMobileAdsManager>`.
- **Zero-Error Fallback**: Runs in mock simulation mode if `ENABLE_GOOGLE_ADS` is not yet active or when testing in Editor.

---

## Installation

### 1. Install via Unity Package Manager (Git URL)
In Unity, open **Window > Package Manager**, click the **+** button, select **Add package from git URL...**, and enter:
```
https://github.com/TonicatFelidae/ETCode.git?path=Assets/Plugins/ETPlugin/Packages/com.nekoniiistudio.et.monetization.googleads
```

### 2. Add Google Mobile Ads SDK Dependency
If not already in your project, add the Google Mobile Ads SDK to your `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.google.ads.mobile": "11.5.0",
    "com.google.external-dependency-manager": "1.2.187"
  },
  "scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.google.ads.mobile",
        "com.google.external-dependency-manager"
      ]
    }
  ]
}
```

### 3. Configure AdMob App IDs
Go to **Assets > Google Mobile Ads > Settings...** in the Unity menu:
- Enable **Google AdMob**.
- Enter your production AdMob App IDs for Android and iOS.

---

## Quick Start

### 1. Initialize Ad Manager
```csharp
using ET.Monetization;
using UnityEngine;

public class AdBootstrap : MonoBehaviour
{
    private GoogleMobileAdsManager _adManager;

    private void Start()
    {
        var config = new GoogleMobileAdsData
        {
            UseTestAds = true, // Set to false in production builds
            AutoInitialize = true,
            AutoLoadAds = true,
            InitBanner = true,
            AdaptiveBanner = true,
            BannerPosition = GoogleBannerPosition.Bottom
        };

        _adManager = new GoogleMobileAdsManager(config);
    }
}
```

### 2. Show Interstitial Ad
```csharp
_adManager.ShowInterstitial(
    onCompleted: () => Debug.Log("Interstitial completed!"),
    onFailed: () => Debug.Log("Interstitial failed or skipped."),
    placement: "level_finish"
);
```

### 3. Show Rewarded Ad
```csharp
_adManager.ShowRewarded(
    onCompleted: () => GrantRewardToPlayer(),
    onFailed: () => Debug.Log("Reward cancelled."),
    placement: "shop_free_gems"
);
```

### 4. Banners & Safe Area
Add `GoogleAdsSafeArea` to any RectTransform you want to automatically shift away from the banner view:
```csharp
_adManager.ShowBanner();
_adManager.HideBanner();
```

---

## Integration with ETEngine

If your project utilizes `ETEngine`, bridge `GoogleMobileAdsManager` using `GenericAdProvider`:

```csharp
using ET.Monetization;
using VContainer;

public class GoogleAdsProvider : GenericAdProvider<GoogleMobileAdsManager>
{
    public GoogleAdsProvider(GoogleMobileAdsManager manager) : base(manager) { }

    public override void Initialize() => _manager.Initialize();
    public override bool IsRewardedReady => _manager.IsRewardedReady;
    public override bool IsInterstitialReady => _manager.IsInterstitialReady;
    public override bool IsBannerShowing => _manager.IsBannerShowing;

    public override void ShowBanner(string placement = "") => _manager.ShowBanner(placement);
    public override void HideBanner() => _manager.HideBanner();
    public override float GetBannerHeight(Rect canvasRect = default) => _manager.GetBannerHeight(canvasRect);

    public override void ShowInterstitial(UnityAction onCompleted = null, UnityAction onFailed = null, string placement = "", string requestId = null)
        => _manager.ShowInterstitial(onCompleted, onFailed, placement, requestId);

    public override void ShowRewarded(UnityAction onCompleted, UnityAction onFailed, string placement = "", string requestId = null, long waitMs = 0)
        => _manager.ShowRewarded(onCompleted, onFailed, placement, requestId, waitMs);
}
```

In your VContainer `LifetimeScope`:
```csharp
builder.RegisterInstance(new GoogleMobileAdsManager(new GoogleMobileAdsData()));
builder.Register<GoogleAdsProvider>(Lifetime.Singleton).As<IAdProvider>();
builder.Register<GenericMonetizationService<GoogleAdsProvider>>(Lifetime.Singleton).As<IMonetizationService>();
```

---

## Telemetry & Analytics Hook
Bind any analytics platform (Firebase, Adjust, Mixpanel) to `GoogleAdsAnalytics.OnLogEvent`:
```csharp
GoogleAdsAnalytics.OnLogEvent += (eventName, parameters) =>
{
    // E.g., FirebaseAnalytics.LogEvent(eventName, ...);
};
```
