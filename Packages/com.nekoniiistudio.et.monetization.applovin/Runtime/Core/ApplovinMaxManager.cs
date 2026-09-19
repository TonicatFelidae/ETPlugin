#if ENABLE_MAX
#if ENABLE_ADJUST
using AdjustSdk;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ET.Analytics;
using UnityEngine;
using UnityEngine.Events;
using static MaxSdkBase;
using static MaxSdkCallbacks;
namespace ET.Monetization
{
    /// <summary>
    /// WARNING the ApplovinMax and Adjust version may change. Need rewrite this for newest version
    /// </summary>
    public class ApplovinMaxManager
    {
        private bool _autoInitializeAds;
        private ApplovinMaxData _data;
        private MonoBehaviour _body;
        // _rewardedReceived removed: susumu-style, dismiss = completed
        private bool _interstitialReceived;
        private bool isBannerShowing;
        private bool isMRecShowing;

        // Ad funnel instrumentation — see docs/analytics/spec/ad-events.md.
        // One tracker per format because MAX's callbacks are static and shared: a
        // rewarded background refill fires the same events as the interstitial that
        // is on screen right now, and a single tracker would let one overwrite the
        // other's placement / request id.
        private readonly AdFunnelTracker _interstitialFunnel = new AdFunnelTracker(AdAnalytics.Format.Interstitial);
        private readonly AdFunnelTracker _rewardedFunnel = new AdFunnelTracker(AdAnalytics.Format.Rewarded);

        // Distinguishes "no ad because the SDK never came up" from "no ad because
        // nothing filled" on ad_show_request.
        private bool _sdkInitialized;

        // Start of the current MREC exposure, for ad_display_end's duration. The
        // banner has no equivalent because it is never hidden — see ShowBannerAds.
        private float _mrecShownAt = -1f;

        public Action<AdInfo> TrackInterstitialAdImpression;
        public Action<AdInfo> TrackRewardedAdImpression;
        public Action<AdInfo> TrackBannerAdImpression;
        public Action<AdInfo> TrackMRecAdImpression;
        // Temporary callbacks for single reward ad show
        // Returns the bottom space to reserve for the banner, in SCREEN PIXELS — measured from the
        // BOTTOM OF THE SCREEN to the TOP of the banner. That is NOT the same as the banner's own
        // height, because MAX anchors the ad view differently per platform:
        //   Android: the ad view is pinned to the bottom of the Unity window, so reserve == height.
        //   iOS:     the ad view's bottom is pinned to superview.safeAreaLayoutGuide.bottomAnchor
        //            (MAUnityAdManager.m, the "bottom_center" branches), so the banner floats ABOVE
        //            the home indicator and reserve == safe-area bottom inset + height. Returning
        //            just the height put the bottom nav bar underneath the banner on every notch
        //            iPhone; Android hid the bug because Screen.safeArea.y is 0 there.
        // SafeArea.ApplySafeArea() works entirely in screen pixels (Screen.safeArea, normalized by
        // Screen.width/height), so this MUST be screen pixels — NOT canvas units. (The old
        // `/pixelsPerUnit` produced canvas units and only matched at width == CanvasScaler ref width 1080.)
        // mainCanvasRect is kept only for call-site compatibility and is intentionally unused.
        public float GetBannerAdsHeight(Rect mainCanvasRect)
        {
#if UNITY_EDITOR
            // EDITOR ONLY: AppLovin doesn't render a real banner in the Editor — it shows a STUB
            // (MaxSdk/Prefabs/BannerBottom.prefab), a FIXED 168px-tall Constant-Pixel-Size overlay,
            // and MaxSdk.GetBannerLayout() returns Rect.zero here. The dp-based calc below (≈34px at
            // the monitor's ~108 dpi) is far smaller than the 168px stub, so the menu ends up under it.
            // Reserve the stub's real height so the Editor preview clears the fake banner.
            // NOTE: the stub is NOT representative of the real device banner — verify layout on a
            // device / Device Simulator, not by the absolute size of this stub.
            const float editorStubBannerHeightPx = 168f;
            return editorStubBannerHeightPx;
#elif UNITY_IOS
            // Preferred path: measure the live ad view instead of predicting it. GetBannerLayout
            // returns the UIKit frame in POINTS with a top-left origin, so `Screen.height - top`
            // is the reserve and the safe-area inset is already baked into it, whatever the device.
            // The frame spans the full screen width (SetBannerBackgroundColor pins the ad view to
            // the superview's left/right anchors), which is what makes width the exact points ->
            // pixels scale — Screen.dpi cannot do that job on iOS: Unity resolves it from a
            // per-model lookup table, so it is ~4% low on 3x phones, ~18% low on iPad, and 0 on
            // any device the table doesn't know yet.
            Rect layoutPoints = GetBannerLayoutPointsOrZero();
            if (layoutPoints.width > 0f && layoutPoints.height > 0f)
            {
                float pointsToPixels = Screen.width / layoutPoints.width;
                float reserve = Screen.height - layoutPoints.y * pointsToPixels;
                // Sanity gate: a bottom banner never occupies a third of the screen. Anything
                // bigger means we read the frame mid-layout, so fall through to the estimate.
                if (reserve > 0f && reserve < Screen.height * 0.35f) return reserve;
            }

            // Fallback for the window before the ad view exists (Init calls this once eagerly).
            // Not an estimate: GetAdaptiveBannerHeight() runs the same [adFormat adaptiveSizeForWidth:]
            // the plugin uses to size the real ad view, and GetScreenDensity() is UIScreen.nativeScale
            // (MAUnityPlugin.mm:964) — the exact points -> pixels factor, and the one thing Screen.dpi
            // could never be. Adding Screen.safeArea.y reproduces the safeAreaLayoutGuide anchoring.
            // Valid only because iOS resolution scaling is off (ProjectSettings resolutionScalingMode: 0),
            // so Unity's pixel space is the native one; with FixedDPI on, nativeScale would not match
            // Screen.safeArea and only the measured path above stays correct.
            float scale = MaxSdkUtils.GetScreenDensity();
            if (scale <= 0f) scale = Screen.dpi > 1f ? Screen.dpi / 160f : 2f;
            float heightPoints = MaxSdkUtils.GetAdaptiveBannerHeight();
            if (heightPoints <= 0f) heightPoints = IsTablet() ? 90f : 50f;
            return Screen.safeArea.y + heightPoints * scale;
#else
            // ANDROID: GetAdaptiveBannerHeight() already returns the correct dp height for THIS device
            // (50dp phone / 90dp tablet), so do NOT multiply by 90/50 again (that double-counted and
            // over-reserved, worsened by IsTablet()'s too-low 6.45" threshold). dp -> screen px via dpi,
            // which is exact here because Android's dp is defined as dpi/160. The ad view sits at the
            // very bottom of the window, so the height IS the reserve.
            float heightNative = MaxSdkUtils.GetAdaptiveBannerHeight();
            float bannerHeightPixels = heightNative * Screen.dpi / 160;
            return bannerHeightPixels;
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        // Rect.zero whenever the frame can't be trusted: before the SDK is up, before CreateBanner,
        // or if the native side hands back something unparseable.
        private Rect GetBannerLayoutPointsOrZero()
        {
            if (!_sdkInitialized) return Rect.zero;
            if (_data == null || string.IsNullOrEmpty(_data.ADS_ID_banner)) return Rect.zero;

            try
            {
                return MaxSdk.GetBannerLayout(_data.ADS_ID_banner);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Banner] GetBannerLayout failed: {e.Message}");
                return Rect.zero;
            }
        }
#endif
        public static bool IsTablet()
        {
            float dpi = Screen.dpi;
            if (dpi == 0)
                dpi = 160;

            float screenHeightInInches = Screen.height / Screen.dpi;
            float screenWidthInInches = Screen.width / Screen.dpi;
            float diagonalSizeInInches = Mathf.Sqrt(Mathf.Pow(screenWidthInInches, 2) + Mathf.Pow(screenHeightInInches, 2));
            return (diagonalSizeInInches >= 6.45f);
        }
        public Action<string, MaxSdk.AdInfo> OnBannerAdLoadedEvent;
        public Action<string, MaxSdk.AdInfo> OnMRECAdLoadedEvent;
        // Fired once with the SDK-resolved country code (ISO alpha-2) after MAX init.
        public Action<string> OnCountryCodeResolved;
        private bool _countryCodeReported;
        public UnityEvent rewardedAdCompleted = new();
        public UnityEvent rewardedAdFailed = new();
        public UnityEvent rewardedAdShown = new();
        public UnityEvent rewardedAdEnded = new();
        public UnityEvent interstitialAdCompleted = new();
        public UnityEvent interstitialAdFailed = new();
        public UnityEvent interstitialAdShown = new();
        public UnityEvent interstitialAdEnded = new();

        #region Developer
        private ApplovinDummyAds _dummyAds;  // created
        private Canvas _testCanvas;
        #endregion
        #region Initiation

        // MaxSdk.InitializeSdk() must run at most once per process. Init is now called
        // from the login flow (GeneralObject.Init), which re-runs whenever the player
        // returns to the title after a system block and taps to retry.
        private bool _initStarted;

        /// <summary>
        /// Answers "is this player on the server's ad-tester list?" (the user-id
        /// based /ad-config). When true, this device registers its OWN advertising
        /// id with MAX and is served test creatives — no per-device GAID/IDFA
        /// registry. Optional; null simply means "nobody is a tester this session".
        /// Kept as a delegate because this assembly must not reference the Game
        /// assemblies where the server service lives.
        /// </summary>
        private Func<UniTask<bool>> _fetchIsAdTester;

        /// <summary>How long the tester-flag fetch may hold up ad initialisation.</summary>
        private const float TestDeviceFetchTimeoutSeconds = 3f;

        public void Init(
            ApplovinMaxData applovinMaxData,
            MonoBehaviour body,
            ApplovinDummyAds applovinDummyAds,
            // created, not prefab
            bool autoInitializeAds = true,
            Func<UniTask<bool>> fetchIsAdTester = null)
        {
            if (_initStarted)
            {
                Debug.Log("[MAX] Init already ran this process — ignoring");
                return;
            }
            _initStarted = true;

            _autoInitializeAds = autoInitializeAds;
            _body = body;
            _data = applovinMaxData;
            _fetchIsAdTester = fetchIsAdTester;
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdk.SdkConfiguration sdkConfiguration) =>
            {
                OnOnSdkInitialized();
                MaxSdk.SetVerboseLogging(true);
                // Report the SDK-resolved country code (fallback "JP" when empty),
                // mirroring susumu. Consumer sends it to the server for the country ban.
                var country = string.IsNullOrEmpty(sdkConfiguration.CountryCode)
                    ? "JP"
                    : sdkConfiguration.CountryCode;
                ReportCountryCode(country);
            };
            _body.StartCoroutine(InitializeSdkRoutine());
#if UNITY_EDITOR
            _dummyAds = applovinDummyAds;
            if (_dummyAds != null) _dummyAds.Hide();
#endif
        }

        // Everything that has to happen before MaxSdk.InitializeSdk(), in order.
        // MAX and every mediated network read the IDFA once, at init, so the ATT
        // answer has to be in hand by then — asking later leaves the whole session
        // on a zeroed IDFA.
        private IEnumerator InitializeSdkRoutine()
        {
            yield return AppTracking.RequestRoutine();

#if UNITY_IOS && !UNITY_EDITOR
            // Meta Audience Network wants the *real* consent state, not a blanket
            // true. Currently a no-op stub (see AudienceNetwork/AdSettings.cs) —
            // wired up so it carries the right value the day an iOS Meta adapter
            // lands in the project.
            AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(
                AppTracking.Status == AppTrackingStatus.Authorized);
#endif

            // Test-device ids can come from two triggers and MUST be applied in ONE
            // call: SetTestDeviceAdvertisingIdentifiers overwrites, it does not
            // append, so calling it per source would leave only the last one
            // registered. And it is silently ignored after InitializeSdk, so both
            // have to land before the init below.
            var testDeviceIds = new List<string>();
            bool ownIdCollected = false;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            // Dev/QA builds always self-register, server flag or not.
            yield return CollectOwnAdvertisingIdRoutine(testDeviceIds);
            ownIdCollected = true;
#endif

            // Server-driven debug mode (user-id based /ad-config): when THIS player
            // is on the Ad Monetization test list, register this device's own
            // advertising id — that is how a RELEASE build lands on test creatives
            // without anyone collecting GAID/IDFA values by hand.
            var isTester = new bool[1];
            yield return CollectServerAdTesterFlagRoutine(isTester);
            if (isTester[0] && !ownIdCollected)
            {
                yield return CollectOwnAdvertisingIdRoutine(testDeviceIds);
            }

            if (testDeviceIds.Count > 0)
            {
                var merged = testDeviceIds.Distinct().ToArray();
                MaxSdk.SetTestDeviceAdvertisingIdentifiers(merged);
                Debug.Log($"[MAX] Test devices registered ({merged.Length}): {string.Join(", ", merged)}");
            }
            else
            {
                Debug.Log("[MAX] No test device ids — this session will be served LIVE ads");
            }

            Debug.Log($"[MAX] Initializing SDK (ATT: {AppTracking.Status})");
            MaxSdk.InitializeSdk();
        }

        // Runs the injected fetcher under a hard deadline. The delegate has its own
        // request timeout, but the guarantee that ad init is never stalled by a slow
        // or hanging network belongs here, where init actually waits. Any failure —
        // timeout, exception, no delegate — degrades to "not a tester" (live ads).
        private IEnumerator CollectServerAdTesterFlagRoutine(bool[] into)
        {
            if (_fetchIsAdTester == null) yield break;

            bool fetched = false;
            bool done = false;
            FetchAdTesterFlagAsync(result => { fetched = result; done = true; }).Forget();

            float waited = 0f;
            while (!done && waited < TestDeviceFetchTimeoutSeconds)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!done)
            {
                Debug.LogWarning($"[MAX] Ad-tester flag fetch exceeded {TestDeviceFetchTimeoutSeconds:F0}s "
                                 + "— initialising as a normal (live-ads) session");
                yield break;
            }

            into[0] = fetched;
            if (fetched)
            {
                Debug.Log("[MAX] Server says this user is an ad tester — enabling debug mode");
            }
        }

        private async UniTaskVoid FetchAdTesterFlagAsync(Action<bool> onDone)
        {
            bool result = false;
            try
            {
                result = await _fetchIsAdTester();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MAX] Ad-tester flag fetch threw ({ex.GetType().Name}: {ex.Message})");
            }
            onDone(result);
        }

        // Appends this device's own advertising id to the pending test-device list.
        // Deliberately does NOT call SetTestDeviceAdvertisingIdentifiers itself —
        // that overwrites, so the caller applies the merged list exactly once.
        // Runs after ATT: on iOS the identifier is all-zeros until ATT is authorized,
        // which is why this never registered a test device there.
        // Compiled into ALL builds since the server's ad-tester flag (FPSA-158
        // branch): a release build self-registers when its USER is flagged; dev/QA
        // builds also run it unconditionally at init.
        private IEnumerator CollectOwnAdvertisingIdRoutine(List<string> into)
        {
            string advertisingId = null;
#if UNITY_ANDROID && !UNITY_EDITOR
            var done = false;
            // AdvertisingIdClient.getAdvertisingIdInfo must run off the main thread.
            var thread = new System.Threading.Thread(() =>
            {
                try { advertisingId = FetchAndroidAdvertisingId(); }
                catch (Exception e) { Debug.LogWarning($"[MAX] Fetch advertising id failed: {e.Message}"); }
                finally { done = true; }
            });
            thread.Start();
            while (!done) yield return null;
#elif UNITY_IOS && !UNITY_EDITOR
            advertisingId = UnityEngine.iOS.Device.advertisingIdentifier;
            yield return null;
#else
            yield return null;
#endif
            if (!string.IsNullOrEmpty(advertisingId)
                && advertisingId != "00000000-0000-0000-0000-000000000000")
            {
                into.Add(advertisingId);
                Debug.Log($"[MAX] Own advertising id (dev build): {advertisingId}");
            }
            else
            {
                // On iOS this is the all-zeros identifier you get without ATT
                // consent, so the status explains the miss.
                Debug.LogWarning("[MAX] No usable advertising id — this device will NOT be "
                                 + $"served test ads. (ATT: {AppTracking.Status})");
            }
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static string FetchAndroidAdvertisingId()
        {
            AndroidJNI.AttachCurrentThread();
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var context = activity.Call<AndroidJavaObject>("getApplicationContext"))
                using (var client = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient"))
                using (var adInfo = client.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", context))
                {
                    return adInfo.Call<string>("getId");
                }
            }
            finally
            {
                AndroidJNI.DetachCurrentThread();
            }
        }
#endif

        public ApplovinDummyAds CreateDummyAds(ApplovinDummyAds pp, Canvas testCanvas)
        {
            _testCanvas = testCanvas;
            return GameObject.Instantiate(pp, _testCanvas.transform);
        }
        #endregion

        public void OnOnSdkInitialized()
        {
            Debug.Log("ApplovinMax Ready");
            _sdkInitialized = true;
            if (_autoInitializeAds)
            {
                AutoInitializeAds();
            }
        }

        // Invokes OnCountryCodeResolved at most once per process; ignores empty input.
        public void ReportCountryCode(string country)
        {
            if (_countryCodeReported) return;
            if (string.IsNullOrEmpty(country)) return;
            _countryCodeReported = true;
            OnCountryCodeResolved?.Invoke(country);
        }
        public void ShowBannerAds()
        {
            if (_data.InitBanner && !string.IsNullOrEmpty(_data.ADS_ID_banner))
            {
                MaxSdk.ShowBanner(_data.ADS_ID_banner);

                // NOTE: there is no matching ad_display_end for the banner — nothing
                // in the project ever calls MaxSdk.HideBanner, so the exposure only
                // ends when the app does. Add the paired event with the hide path,
                // not before, or ad_visible_ms would be measured off a guess.
                if (!isBannerShowing)
                {
                    isBannerShowing = true;
                    AdAnalytics.DisplayStart(_data.ADS_ID_banner, AdAnalytics.Format.Banner, null);
                }
            }
        }
        public void AutoInitializeAds()
        {
            if (_data.InitBanner && !string.IsNullOrEmpty(_data.ADS_ID_banner))
            {
                var adViewConfiguration = new MaxSdk.AdViewConfiguration(MaxSdk.AdViewPosition.BottomCenter);
                // Adaptive (anchored) banner: the height follows the device instead of a flat 50/90dp.
                // AdViewConfiguration already defaults IsAdaptive to true, but pin it explicitly —
                // this is a deliberate choice, not something to inherit from an SDK default that a
                // future MAX release could flip. Note the height is then device- and width-dependent,
                // which is why GetBannerAdsHeight() measures the ad view instead of assuming 50/90.
                adViewConfiguration.IsAdaptive = true;
                MaxSdk.CreateBanner(_data.ADS_ID_banner, adViewConfiguration);
                MaxSdk.SetBannerBackgroundColor(_data.ADS_ID_banner, Color.black);
                MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
                MaxSdkCallbacks.Banner.OnAdLoadedEvent += (adUnitId, adInfo) => Debug.Log("Banner loaded");
                MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
                MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
            }
            if (!string.IsNullOrEmpty(_data.ADS_ID_interstitial))
            {
                InitializeInterstitialAds();
            }
            if (!string.IsNullOrEmpty(_data.ADS_ID_rewarded))
            {
                InitializeRewardedAds();
            }
            if (_data.InitMREC && !string.IsNullOrEmpty(_data.ADS_ID_MREC))
            {
                InitializeMRecAds();
                LoadMRecAds();
            }
        }

        // Banner load failures were previously invisible: the callback was never
        // subscribed, so a banner that never filled looked identical to one quietly
        // earning money. Only the failure is reported — banner load *successes*
        // arrive on MAX's ~60s auto-refresh and would swamp every other event.
        private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            Debug.Log("Banner failed to load with error code: " + errorInfo.Code);
            AdAnalytics.LoadFailed(adUnitId, AdAnalytics.Format.Banner,
                (int)errorInfo.Code, errorInfo.Message, 0);
        }

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Banner ad clicked");
            AdAnalytics.Clicked(adUnitId, AdAnalytics.Format.Banner, null, adInfo.NetworkName);
        }

        private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Banner ad revenue paid");
            TrackAdRevenue(adInfo);
            TrackBannerAdImpression?.Invoke(adInfo);
        }

        #region Interstitial Ad Methods
        int interstitialRetryAttempt = 0;
        private void InitializeInterstitialAds()
        {
            // Attach callbacks
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialRevenuePaidEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialAdDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialAdClickedEvent;


            // Load the first interstitial. Routed through LoadInterstitial() rather
            // than MaxSdk directly so the very first load also reports ad_load_start —
            // otherwise the opening request would be missing from the fill-rate
            // denominator while its success or failure still counted.
            LoadInterstitial();
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'

            Debug.Log("Interstitial loaded");

            _interstitialFunnel.LoadSuccess(adUnitId, adInfo.NetworkName, interstitialRetryAttempt);

            // Reset retry attempt
            interstitialRetryAttempt = 0;
        }

        private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // The attempt number of the load that just failed, captured before the
            // counter moves on: fill rate is defined on attempt 0 only, so shifting
            // this by one would leave that bucket permanently empty.
            int failedAttempt = interstitialRetryAttempt;

            // Interstitial ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
            interstitialRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempt));

            Debug.Log("Interstitial failed to load with error code: " + errorInfo.Code);

            _interstitialFunnel.LoadFailed(adUnitId, (int)errorInfo.Code, errorInfo.Message, failedAttempt);

            // Same defect the rewarded path had: Invoke looks the name up on _body
            // (GameInstaller), which has no LoadInterstitial, so the retry never ran.
            _body.StartCoroutine(LoadInterstitialAfterDelay((float)retryDelay));
        }

        private void OnInterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad failed to display. We recommend loading the next ad
            Debug.Log("Interstitial failed to display with error code: " + errorInfo.Code);
            _interstitialFunnel.ShowFailed(adUnitId, adInfo.NetworkName, (int)errorInfo.Code, errorInfo.Message);
            LoadInterstitial();
        }

        private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Interstitial ad dismissed");
            // Before the flags below are cleared: _interstitialReceived is what says
            // the ad actually reached the screen.
            _interstitialFunnel.Dismissed(adUnitId, adInfo.NetworkName, _interstitialReceived);
            interstitialAdEnded?.Invoke();
            if (!_interstitialReceived)
            {
                interstitialAdFailed?.Invoke();
            }
            else
            {

                interstitialAdCompleted?.Invoke();
            }
            _interstitialReceived = false;
            Debug.Log("Interstitial dismissed");
            LoadInterstitial();
        }

        private void OnInterstitialRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad revenue paid. Use this callback to track user revenue.
            Debug.Log("Interstitial revenue paid");

            // Ad revenue
            double revenue = adInfo.Revenue;

            // Miscellaneous data
            string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD"!
            string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
            string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
            string placement = adInfo.Placement; // The placement this ad's postbacks are tied to

            TrackAdRevenue(adInfo);
            TrackInterstitialAdImpression?.Invoke(adInfo);
        }
        private void OnInterstitialAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Interstitial ad displayed");
            _interstitialReceived = true;
            _interstitialFunnel.ShowSuccess(adUnitId, adInfo.NetworkName);
            interstitialAdShown?.Invoke();
        }

        private void OnInterstitialAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Interstitial ad clicked");
            _interstitialFunnel.Clicked(adUnitId, adInfo.NetworkName);
        }

        public void LoadInterstitial()
        {
            _interstitialFunnel.LoadStart(_data.ADS_ID_interstitial, interstitialRetryAttempt);
            MaxSdk.LoadInterstitial(_data.ADS_ID_interstitial);
        }

        private IEnumerator LoadInterstitialAfterDelay(float delaySeconds)
        {
            if (delaySeconds > 0f)
            {
                yield return new WaitForSeconds(delaySeconds);
            }

            LoadInterstitial();
        }

        /// <param name="requestId">
        /// Correlation id for the whole show, carried on every funnel event. Callers
        /// that opened a server ad-check pass its impressionId so the Firebase rows
        /// can be joined to the ad_impressions table later.
        /// </param>
        public void ShowInterstitialAds(UnityAction onCompleted, UnityAction onFailed,
            string placement = "", string requestId = null)
        {
            interstitialAdCompleted.RemoveAllListeners();
            interstitialAdFailed.RemoveAllListeners();
            if (onCompleted != null) interstitialAdCompleted.AddListener(onCompleted);
            if (onFailed != null) interstitialAdFailed.AddListener(onFailed);
            ShowInterstitial(placement, requestId);
        }
        private void ShowInterstitial(string placement = "", string requestId = null)
        {
            bool ready = MaxSdk.IsInterstitialReady(_data.ADS_ID_interstitial);

            // Logged on both branches, and before the readiness check decides
            // anything: the share of requests that find no ad is the point of the
            // event, so a "not ready" must count in the same denominator as a hit.
            _interstitialFunnel.ShowRequest(_data.ADS_ID_interstitial, placement, ready,
                _sdkInitialized, requestId, waitMs: 0);

            if (ready)
            {
                _interstitialFunnel.ShowStart(_data.ADS_ID_interstitial);

                if (string.IsNullOrEmpty(placement))
                {
                    MaxSdk.ShowInterstitial(_data.ADS_ID_interstitial);
                }
                else
                {
                    MaxSdk.ShowInterstitial(_data.ADS_ID_interstitial, placement);
                }
            }
            else
            {
                Debug.LogError("Interstitial Not Ready");
                interstitialAdFailed?.Invoke();
                LoadInterstitial();
            }
        }

        #endregion
        #region Rewarded Ad Methods
        int rewardedRetryAttempt = 0;
        private void InitializeRewardedAds()
        {
            // Attach callbacks
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += RewardedFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;

            // Load the available rewarded ad units
            LoadRewarded(_data.ADS_ID_rewarded);
        }
        // State for the ONE show currently in flight. MAX fires load/display/reward/
        // hidden on the same static callbacks whether or not anyone asked for an ad,
        // so without this the listeners from the last show get invoked by unrelated
        // background events.
        private bool _rewardedShowPending;   // a ShowRewardAds is awaiting its outcome
        private bool _rewardedReceived;      // OnAdReceivedRewardEvent fired for it
        private bool _rewardedResolved;      // completed/failed already dispatched

        /// <param name="requestId">
        /// Correlation id for the whole show, carried on every funnel event. Rewarded
        /// callers pass the server's impressionId from /ad-checks/start, so a Firebase
        /// row and its ad_impressions row share a key.
        /// </param>
        /// <param name="waitMs">
        /// How long the player was already held on the "loading ad" spinner before
        /// this call. Reported on ad_show_start, where it is the wait they actually
        /// experienced — measuring it inside this method would always read zero.
        /// </param>
        public void ShowRewardAds(UnityAction onCompleted, UnityAction onFailed, string adUnitId = null,
            string placement = "", string requestId = null, long waitMs = 0)
        {
            rewardedAdCompleted.RemoveAllListeners();
            rewardedAdFailed.RemoveAllListeners();
            if (onCompleted != null) rewardedAdCompleted.AddListener(onCompleted);
            if (onFailed != null) rewardedAdFailed.AddListener(onFailed);

            _rewardedShowPending = true;
            _rewardedReceived = false;
            _rewardedResolved = false;

            ShowRewarded(adUnitId, placement, requestId, waitMs);
        }

        // Dispatch the outcome of the in-flight show exactly once. Every path that
        // can end a show funnels through here; MAX happily fires both a display
        // failure and a hidden event for the same ad, which used to invoke the
        // caller's onFailed and onCompleted for a single attempt.
        private void ResolveRewarded(bool completed)
        {
            if (!_rewardedShowPending || _rewardedResolved) return;
            _rewardedResolved = true;
            _rewardedShowPending = false;

            if (completed) rewardedAdCompleted?.Invoke();
            else rewardedAdFailed?.Invoke();
        }

        private void ShowRewarded(string adUnitId = null, string placement = "",
            string requestId = null, long waitMs = 0)
        {
            var targetAdUnitId = ResolveRewardedAdUnitId(adUnitId);
            if (string.IsNullOrEmpty(targetAdUnitId))
            {
                Debug.LogError("Rewarded ad unit id is not configured");
                // A missing ad unit id is a misconfigured build, not an ad
                // opportunity, so it stays out of the show_request denominator.
                ResolveRewarded(false);
                return;
            }

            bool ready = MaxSdk.IsRewardedAdReady(targetAdUnitId);

            // Both branches, before the outcome is known: requests that find no ad
            // are the metric this event exists for.
            _rewardedFunnel.ShowRequest(targetAdUnitId, placement, ready, _sdkInitialized, requestId, waitMs);

            if (ready)
            {
                _rewardedFunnel.ShowStart(targetAdUnitId);

                if (string.IsNullOrEmpty(placement))
                {
                    MaxSdk.ShowRewardedAd(targetAdUnitId);
                }
                else
                {
                    MaxSdk.ShowRewardedAd(targetAdUnitId, placement);
                }
            }
            else
            {
                Debug.LogError("Rewarded Not Ready");
                ResolveRewarded(false);
                LoadRewarded(targetAdUnitId);
            }
        }

        public bool IsRewardedReady
        {
            get
            {
                var id = ResolveRewardedAdUnitId(null);
                return !string.IsNullOrEmpty(id) && MaxSdk.IsRewardedAdReady(id);
            }
        }

        /// <summary>
        /// Waits for a rewarded ad to finish preloading, up to <paramref name="timeoutSeconds"/>.
        /// Returns whether one is ready.
        ///
        /// Ad init now runs after login rather than at scene build, so the preload
        /// window opens much later: a player who taps an ad button the moment Home
        /// appears would otherwise hit "Rewarded Not Ready" and lose the reward on a
        /// perfectly healthy network. Callers show a spinner and wait instead.
        /// </summary>
        public async UniTask<bool> WaitForRewardedReadyAsync(float timeoutSeconds = 5f)
        {
            if (IsRewardedReady) return true;

            float waited = 0f;
            while (waited < timeoutSeconds)
            {
                await UniTask.Yield();
                waited += Time.unscaledDeltaTime;
                if (IsRewardedReady) return true;
            }

            Debug.LogWarning($"[MAX] Rewarded still not ready after {timeoutSeconds:F0}s");
            return false;
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Rewarded ad loaded");
            _rewardedFunnel.LoadSuccess(adUnitId, adInfo.NetworkName, rewardedRetryAttempt);
            rewardedRetryAttempt = 0;
        }

        private void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Captured before the counter advances — see the interstitial twin.
            int failedAttempt = rewardedRetryAttempt;

            rewardedRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, rewardedRetryAttempt));
            Debug.Log("Rewarded ad failed to load with error code: " + errorInfo.Code);
            _rewardedFunnel.LoadFailed(adUnitId, (int)errorInfo.Code, errorInfo.Message, failedAttempt);
            ScheduleRewardedReload(adUnitId, (float)retryDelay);

            // This fires for background refills too, with nobody waiting. Reporting
            // failure then would run the previous show's onFailed a second time —
            // an error popup out of nowhere, minutes after the player's ad.
            ResolveRewarded(false);
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Rewarded ad displayed");
            _rewardedFunnel.ShowSuccess(adUnitId, adInfo.NetworkName);
            rewardedAdShown?.Invoke();
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Rewarded ad clicked");
            _rewardedFunnel.Clicked(adUnitId, adInfo.NetworkName);
        }

        private void RewardedFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Rewarded ad failed to display with error code: " + errorInfo.Code);
            _rewardedFunnel.ShowFailed(adUnitId, adInfo.NetworkName, (int)errorInfo.Code, errorInfo.Message);
            LoadRewarded(adUnitId);
            ResolveRewarded(false);
        }

        private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // The reward event decides the outcome, not the dismiss. Closing the ad
            // early is a cancel: the player watched nothing, so they get nothing and
            // are invited to try again. (This replaces the earlier susumu-style
            // "dismiss == completed", under which skipping the video still paid out.)
            Debug.Log($"Rewarded ad dismissed — received={_rewardedReceived}");
            // ad_rewarded says whether this dismissal paid out. Logged before the
            // reload below, which clears the funnel's show state.
            _rewardedFunnel.Dismissed(adUnitId, adInfo.NetworkName, _rewardedReceived);
            rewardedAdEnded?.Invoke();
            ResolveRewarded(_rewardedReceived);
            LoadRewarded(adUnitId);
        }

        private void OnRewardedRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Rewarded ad revenue paid");
            TrackAdRevenue(adInfo);
            TrackRewardedAdImpression?.Invoke(adInfo);
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            // The only signal that the player actually watched to the end. Recorded
            // here and read on dismiss, because MAX fires this BEFORE the ad closes.
            Debug.Log("Rewarded ad received reward");
            _rewardedReceived = true;
            // The other half of the drop-off metric: this fires only when the player
            // watched far enough, which ad_show_success does not tell you.
            _rewardedFunnel.RewardEarned(adUnitId, adInfo.NetworkName);
        }
        private void LoadRewarded()
        {
            LoadRewarded(_data.ADS_ID_rewarded);
        }

        private void LoadRewarded(string adUnitId)
        {
            if (string.IsNullOrEmpty(adUnitId))
            {
                return;
            }
            _rewardedFunnel.LoadStart(adUnitId, rewardedRetryAttempt);
            MaxSdk.LoadRewardedAd(adUnitId);
        }

        private string ResolveRewardedAdUnitId(string adUnitId)
        {
            if (!string.IsNullOrEmpty(adUnitId))
            {
                return adUnitId;
            }

            if (!string.IsNullOrEmpty(_data.ADS_ID_rewarded))
            {
                return _data.ADS_ID_rewarded;
            }

            return null;
        }

        private void ScheduleRewardedReload(string adUnitId, float delaySeconds)
        {
            var targetAdUnitId = ResolveRewardedAdUnitId(adUnitId);
            if (string.IsNullOrEmpty(targetAdUnitId) || _body == null)
            {
                return;
            }

            // MonoBehaviour.Invoke resolves the method name against _body's OWN type
            // (GameInstaller), which has no LoadRewarded — so this silently never
            // reloaded. It only ever took this branch for the DEFAULT ad unit, i.e.
            // the one every caller actually uses: one failed load and rewarded ads
            // were gone until the app restarted. The coroutine below always worked;
            // it just never ran for the id that mattered.
            _body.StartCoroutine(LoadRewardedAfterDelay(targetAdUnitId, delaySeconds));
        }

        private IEnumerator LoadRewardedAfterDelay(string adUnitId, float delaySeconds)
        {
            if (delaySeconds > 0f)
            {
                yield return new WaitForSeconds(delaySeconds);
            }

            LoadRewarded(adUnitId);
        }
        #endregion
        #region MREC Ads Methods

        public void InitializeMRecAds()
        {
            MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecAdLoadedEvent;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecAdFailedEvent;
            MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecAdClickedEvent;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnMRecAdRevenuePaidEvent;
        }
        // Must run after MaxSdk.InitializeSdk(): the native plugin drops CreateMRec
        // (with a "please ensure the plugin has been initialized" error) when it is
        // called earlier, and a later ShowMRec then has no ad view to show.
        public void LoadMRecAds()
        {
            if (!_sdkInitialized)
            {
                Debug.LogWarning("[MAX] LoadMRecAds called before SDK init — ignored");
                return;
            }
            MaxSdk.CreateMRec(_data.ADS_ID_MREC, new AdViewConfiguration(AdViewPosition.Centered));
        }

        public void ShowMRecAds(Vector2 unityPosition, Vector2 applovinPosition)
        {
            if (_data == null) return;
            if (_data.InitMREC && !string.IsNullOrEmpty(_data.ADS_ID_MREC))
            {
                MaxSdk.ShowMRec(_data.ADS_ID_MREC);
                MaxSdk.UpdateMRecPosition(_data.ADS_ID_MREC, applovinPosition.x, applovinPosition.y);

                // Guarded so a reposition-driven re-show does not restart the clock
                // and split one exposure into several.
                if (!isMRecShowing)
                {
                    isMRecShowing = true;
                    _mrecShownAt = Time.realtimeSinceStartup;
                    AdAnalytics.DisplayStart(_data.ADS_ID_MREC, AdAnalytics.Format.MRec, null);
                }
            }
        }
        public void HideMRecAds()
        {
            if (_data.InitMREC && !string.IsNullOrEmpty(_data.ADS_ID_MREC))
            {
                MaxSdk.HideMRec(_data.ADS_ID_MREC);

                if (isMRecShowing)
                {
                    isMRecShowing = false;
                    long visibleMs = _mrecShownAt < 0f
                        ? 0L
                        : (long)((Time.realtimeSinceStartup - _mrecShownAt) * 1000f);
                    _mrecShownAt = -1f;
                    AdAnalytics.DisplayEnd(_data.ADS_ID_MREC, AdAnalytics.Format.MRec, null, visibleMs);
                }
            }
        }

        private void OnMRecAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnMRECAdLoadedEvent?.Invoke(adUnitId, adInfo);
            //ShowMRecAds();
            // MRec ad is ready to be shown.
            // If you have already called MaxSdk.ShowMRec(MRecAdUnitId) it will automatically be shown on the next MRec refresh.
            Debug.Log("MRec ad loaded");
        }

        private void OnMRecAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // MRec ad failed to load. MAX will automatically try loading a new ad internally.
            Debug.Log("MRec ad failed to load with error code: " + errorInfo.Code);
            // Failures only, like the banner: MREC load successes ride the auto-refresh.
            AdAnalytics.LoadFailed(adUnitId, AdAnalytics.Format.MRec,
                (int)errorInfo.Code, errorInfo.Message, 0);
        }

        private void OnMRecAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("MRec ad clicked");
            AdAnalytics.Clicked(adUnitId, AdAnalytics.Format.MRec, null, adInfo.NetworkName);
        }

        private void OnMRecAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // MRec ad revenue paid. Use this callback to track user revenue.
            Debug.Log("MRec ad revenue paid");

            // Ad revenue
            double revenue = adInfo.Revenue;

            // Miscellaneous data
            string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD"!
            string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
            string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
            string placement = adInfo.Placement; // The placement this ad's postbacks are tied to

            TrackAdRevenue(adInfo);
            TrackMRecAdImpression?.Invoke(adInfo);
        }

        #endregion
        #region Revenue attribution
        /// <summary>
        /// One impression's revenue, reported to both destinations that need it:
        /// Adjust for attribution/ROAS, and Firebase as the standard
        /// <c>ad_impression</c> event that GA4 folds into ad revenue on its own.
        ///
        /// Called from all four revenue callbacks (banner / interstitial / rewarded /
        /// MREC), so <c>adInfo.AdFormat</c> — MAX's own format string — is what
        /// distinguishes them; it is deliberately not mapped onto our ad_format
        /// vocabulary, because GA4 reads this event with its own schema.
        /// </summary>
        private void TrackAdRevenue(MaxSdkBase.AdInfo adInfo)
        {
            AdAnalytics.Impression(adInfo.NetworkName, adInfo.AdUnitIdentifier, adInfo.AdFormat, adInfo.Revenue);

#if ENABLE_ADJUST
            AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue("applovin_max_sdk");

            adjustAdRevenue.SetRevenue(adInfo.Revenue, "USD");
            adjustAdRevenue.AdRevenueNetwork = adInfo.NetworkName;
            adjustAdRevenue.AdRevenueUnit = adInfo.AdUnitIdentifier;
            adjustAdRevenue.AdRevenuePlacement = adInfo.Placement;

            Adjust.TrackAdRevenue(adjustAdRevenue);
#endif
        }
        #endregion


    }
}
#endif
