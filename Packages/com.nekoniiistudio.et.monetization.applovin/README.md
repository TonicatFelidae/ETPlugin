# ET Monetization AppLovin (`com.nekoniiistudio.et.monetization.applovin`)

A production-ready Unity Package Manager (UPM) package providing centralized AppLovin MAX monetization management, ad funnel telemetry, ATT (App Tracking Transparency) gating, and mediation attribution helpers (Adjust, Facebook/Meta SDK, Audience Network).

---

## Features

- **AppLovin MAX Management**:
  - Full support for Banner (adaptive), Interstitial, Rewarded, and MREC ads.
  - Safe Area calculation for anchored banners across iOS and Android.
  - Exponential backoff retry logic for ad preloading.
  - MREC position conversion between Unity canvas coordinates and AppLovin screen coordinates with dummy overlay previews for Unity Editor.
  - Server-driven and build-driven test device GAID/IDFA self-registration.
- **Ad Funnel Analytics & Instrumentation**:
  - Full funnel event instrumentation (13 standard ad funnel events: load start/success/fail, show request/start/success/fail, reward earned, dismissal, click, revenue impression, banner/mrec display duration).
  - Decoupled telemetry hook (`AdAnalytics.OnLogEvent`) allowing effortless routing to Firebase Analytics, GameAnalytics, Adjust, Unity Analytics, or custom backends.
  - Single-line concise console logging (`[Ads] ...`) for debugging on device without third-party dashboards.
- **Attribution & Consent Management**:
  - iOS App Tracking Transparency (ATT) lifecycle handler (`AppTracking`) with settle delay and focus timeout before prompt.
  - Integrated iOS Objective-C ATT bridge (`ETAppTracking.mm`).
  - Adjust attribution helper (`AdjustManager`) with async ADID retrieval.
  - Facebook/Meta app-events and session resumption attribution (`FacebookManager`).
  - Meta Audience Network iOS advertiser tracking flag bridge (`AdSettings`).
- **Android Resolver Dependencies**:
  - EDM4U Play Services Ads Identifier XML dependency included for Google Advertising ID (GAID) retrieval in development builds.

---

## Installation

### Via Unity Package Manager (Git URL)

Add the package to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.nekoniiistudio.et.monetization.applovin": "https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.monetization.applovin#main"
  }
}
```

Or install via Unity Package Manager Window:
1. Open **Window > Package Manager**.
2. Click **+** > **Add package from git URL...**.
3. Enter:
   `https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.monetization.applovin#main`

---

## Prerequisites & Dependencies

1. **AppLovin MAX Unity SDK** (`com.applovin.mediation.ads`):
   Configure AppLovin scoped registry in `Packages/manifest.json`:
   ```json
   "scopedRegistries": [
     {
       "name": "AppLovin MAX Unity",
       "url": "https://unity.packages.applovin.com/",
       "scopes": [
         "com.applovin.mediation.ads",
         "com.applovin.mediation.adapters",
         "com.applovin.mediation.dsp"
       ]
     }
   ]
   ```
2. **UniTask** (`com.cysharp.unitask`):
   ```json
   "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"
   ```
3. **External Dependency Manager for Unity (EDM4U)** (`com.google.external-dependency-manager`):
   Used to resolve Android `play-services-ads-identifier`.

---

## Scripting Define Symbols

| Symbol | Behavior |
|---|---|
| `ENABLE_MAX` | Enables AppLovin MAX SDK routines. Automatically defined via `versionDefines` when `com.applovin.mediation.ads` is present in the project. |
| `ENABLE_ADJUST` | Enables Adjust attribution routines. Automatically defined via `versionDefines` when `com.adjust.sdk` is present. |
| `ENABLE_FACEBOOK` | Define in Player Settings when Facebook Unity SDK is installed. |
| `ENABLE_AUDIENCE_NETWORK` | Define in iOS Player Settings when native Meta Audience Network symbols are available. |

---

## Quick Start

### 1. Initialize Ads

```csharp
using ET.Monetization;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private ApplovinDummyAds pp_dummyAds;

    private ApplovinMaxManager _maxManager;

    private void Start()
    {
        var adData = new ApplovinMaxData
        {
            ADS_ID_rewarded = "YOUR_REWARDED_AD_UNIT_ID",
            ADS_ID_interstitial = "YOUR_INTERSTITIAL_AD_UNIT_ID",
            ADS_ID_banner = "YOUR_BANNER_AD_UNIT_ID",
            ADS_ID_MREC = "YOUR_MREC_AD_UNIT_ID",
            InitBanner = true,
            InitMREC = false
        };

        _maxManager = new ApplovinMaxManager();
        _maxManager.Init(adData, this, pp_dummyAds, autoInitializeAds: true);
    }
}
```

### 2. Show Rewarded Ads

```csharp
if (_maxManager.IsRewardedReady)
{
    _maxManager.ShowRewardAds(
        onCompleted: () => {
            Debug.Log("Reward granted!");
        },
        onFailed: () => {
            Debug.Log("Ad failed or user dismissed before reward.");
        },
        placement: "shop_double_coins"
    );
}
```

### 3. Connect Telemetry & Analytics

To route ad funnel analytics to your telemetry provider (e.g., Firebase Analytics, GameAnalytics):

```csharp
using ET.Analytics;

// In your analytics setup or bootstrap:
AdAnalytics.OnLogEvent += (eventName, parameters) =>
{
    // Forward eventName and parameters to your analytics service:
    // FirebaseAnalytics.LogEvent(eventName, ...);
};
```

---

## iOS Configuration

For iOS builds:
1. Ensure `NSUserTrackingUsageDescription` is added to your `Info.plist`.
2. Weak-link `AppTrackingTransparency.framework` in Xcode (`UnityFramework` target).
