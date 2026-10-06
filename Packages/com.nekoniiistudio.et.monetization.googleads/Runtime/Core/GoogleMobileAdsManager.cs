using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ET.Analytics;
using UnityEngine;
using UnityEngine.Events;

#if ENABLE_GOOGLE_ADS
using GoogleMobileAds.Api;
#endif

namespace ET.Monetization
{
    /// <summary>
    /// Core manager for Google Mobile Ads (AdMob).
    /// Provides unified APIs for Banner, Interstitial, Rewarded, Rewarded Interstitial, and App Open ads.
    /// Supports automatic mediation telemetry, safe area coordination, and decoupled event handling.
    /// Compatible with Google Mobile Ads Unity SDK v11.x+.
    /// </summary>
    public class GoogleMobileAdsManager
    {
        private GoogleMobileAdsData _data;
        private bool _isInitialized;
        private bool _isBannerShowing;

        // Funnel trackers for telemetry
        private readonly GoogleAdsFunnelTracker _funnelBanner = new GoogleAdsFunnelTracker(GoogleAdsAnalytics.Format.Banner);
        private readonly GoogleAdsFunnelTracker _funnelInterstitial = new GoogleAdsFunnelTracker(GoogleAdsAnalytics.Format.Interstitial);
        private readonly GoogleAdsFunnelTracker _funnelRewarded = new GoogleAdsFunnelTracker(GoogleAdsAnalytics.Format.Rewarded);
        private readonly GoogleAdsFunnelTracker _funnelRewardedInterstitial = new GoogleAdsFunnelTracker(GoogleAdsAnalytics.Format.RewardedInterstitial);
        private readonly GoogleAdsFunnelTracker _funnelAppOpen = new GoogleAdsFunnelTracker(GoogleAdsAnalytics.Format.AppOpen);

#if ENABLE_GOOGLE_ADS
        private BannerView _bannerView;
        private InterstitialAd _interstitialAd;
        private RewardedAd _rewardedAd;
        private RewardedInterstitialAd _rewardedInterstitialAd;
        private AppOpenAd _appOpenAd;

        private UnityAction _currentInterstitialOnCompleted;
        private UnityAction _currentInterstitialOnFailed;
        private UnityAction _currentRewardedOnCompleted;
        private UnityAction _currentRewardedOnFailed;
        private UnityAction _currentRewardedInterstitialOnCompleted;
        private UnityAction _currentRewardedInterstitialOnFailed;
        private UnityAction _currentAppOpenOnCompleted;
        private UnityAction _currentAppOpenOnFailed;

        private bool _rewardGranted;
        private bool _rewardedInterstitialGranted;
#endif

        // Public Events
        public event Action<string, double, string> OnAdImpression;
        public event Action<string, double> OnUserEarnedReward;
        public event Action OnInterstitialCompleted;
        public event Action OnInterstitialFailed;
        public event Action OnInterstitialShown;
        public event Action OnInterstitialEnded;
        public event Action OnRewardedCompleted;
        public event Action OnRewardedFailed;
        public event Action OnRewardedShown;
        public event Action OnRewardedEnded;
        public event Action OnBannerLoaded;
        public event Action OnBannerFailed;

        public GoogleMobileAdsData Data => _data;
        public bool IsInitialized => _isInitialized;

        public GoogleMobileAdsManager(GoogleMobileAdsData data = null)
        {
            _data = data ?? new GoogleMobileAdsData();
            if (_data.AutoInitialize)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Initializes the Google Mobile Ads SDK with COPPA / RequestConfiguration settings.
        /// </summary>
        public void Initialize(GoogleMobileAdsData data = null, Action onInitialized = null)
        {
            if (data != null) _data = data;
            if (_data == null) _data = new GoogleMobileAdsData();

#if ENABLE_GOOGLE_ADS
            var requestConfig = new RequestConfiguration();

#pragma warning disable CS0618
            if (_data.IsTagForChildDirectedTreatment)
            {
                requestConfig.TagForChildDirectedTreatment = TagForChildDirectedTreatment.True;
            }

            if (_data.TagForUnderAgeOfConsent)
            {
                requestConfig.TagForUnderAgeOfConsent = TagForUnderAgeOfConsent.True;
            }
#pragma warning restore CS0618

            if (!string.IsNullOrEmpty(_data.MaxAdContentRating))
            {
                requestConfig.MaxAdContentRating = _data.MaxAdContentRating switch
                {
                    "G" => MaxAdContentRating.G,
                    "PG" => MaxAdContentRating.PG,
                    "T" => MaxAdContentRating.T,
                    "MA" => MaxAdContentRating.MA,
                    _ => requestConfig.MaxAdContentRating
                };
            }

            MobileAds.SetRequestConfiguration(requestConfig);

            MobileAds.Initialize((InitializationStatus initStatus) =>
            {
                _isInitialized = true;
                Debug.Log("[GoogleMobileAdsManager] Google Mobile Ads SDK initialized successfully.");

                if (_data.InitBanner)
                {
                    LoadBanner();
                }

                if (_data.AutoLoadAds)
                {
                    LoadInterstitial();
                    LoadRewarded();
                }

                onInitialized?.Invoke();
            });
#else
            _isInitialized = true;
            Debug.LogWarning("[GoogleMobileAdsManager] ENABLE_GOOGLE_ADS is not defined or Google Mobile Ads SDK is not installed. Running in mock mode.");
            onInitialized?.Invoke();
#endif
        }

        #region Banner
        public bool IsBannerShowing => _isBannerShowing;

        public void LoadBanner(string placement = "")
        {
#if ENABLE_GOOGLE_ADS
            string unitId = TestAdUnits.ResolveBannerUnitId(_data.UseTestAds, _data.BannerUnitIdAndroid, _data.BannerUnitIdIOS);
            _funnelBanner.TrackLoadRequest(unitId);

            if (_bannerView != null)
            {
                DestroyBanner();
            }

            AdSize adSize = _data.AdaptiveBanner
                ? AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth)
                : AdSize.Banner;

            AdPosition position = GoogleAdsUtils.ConvertPosition(_data.BannerPosition);
            _bannerView = new BannerView(unitId, adSize, position);

            _bannerView.OnBannerAdLoaded += () =>
            {
                _funnelBanner.TrackLoaded(unitId);
                UpdateBannerSafeArea();
                OnBannerLoaded?.Invoke();
            };

            _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                _funnelBanner.TrackLoadFailed(unitId, error?.GetCode() ?? -1, error?.GetMessage() ?? "Unknown error");
                OnBannerFailed?.Invoke();
            };

            _bannerView.OnAdPaid += (AdValue adValue) =>
            {
                double revenue = adValue.Value / 1000000.0;
                _funnelBanner.TrackImpression(revenue, adValue.CurrencyCode, adValue.Precision.ToString());
                OnAdImpression?.Invoke("banner", revenue, adValue.CurrencyCode);
            };

            _bannerView.OnAdImpressionRecorded += () =>
            {
                _funnelBanner.TrackDisplayStart();
            };

            _bannerView.OnAdClicked += () =>
            {
                _funnelBanner.TrackClicked();
            };

            _bannerView.LoadAd(new AdRequest());
#else
            Debug.Log($"[GoogleMobileAdsManager Mock] LoadBanner requested for placement: '{placement}'");
            OnBannerLoaded?.Invoke();
#endif
        }

        public void ShowBanner(string placement = "")
        {
            _isBannerShowing = true;
            _funnelBanner.TrackShowRequest(placement);

#if ENABLE_GOOGLE_ADS
            if (_bannerView == null)
            {
                LoadBanner(placement);
                UpdateBannerSafeArea();
            }
            else
            {
                _bannerView.Show();
                UpdateBannerSafeArea();
            }
#else
            Debug.Log($"[GoogleMobileAdsManager Mock] ShowBanner displayed for placement: '{placement}'");
#endif
        }

        public void HideBanner()
        {
            _isBannerShowing = false;
            _funnelBanner.TrackDisplayEnd();

#if ENABLE_GOOGLE_ADS
            if (_bannerView != null)
            {
                _bannerView.Hide();
            }
            GoogleAdsSafeAreaHelper.Reset();
#else
            Debug.Log("[GoogleMobileAdsManager Mock] HideBanner called.");
#endif
        }

        public void DestroyBanner()
        {
            _isBannerShowing = false;
#if ENABLE_GOOGLE_ADS
            if (_bannerView != null)
            {
                _bannerView.Destroy();
                _bannerView = null;
            }
            GoogleAdsSafeAreaHelper.Reset();
#else
            Debug.Log("[GoogleMobileAdsManager Mock] DestroyBanner called.");
#endif
        }

        public float GetBannerHeight(Rect canvasRect = default)
        {
#if ENABLE_GOOGLE_ADS
            if (_bannerView != null && _isBannerShowing)
            {
                return _bannerView.GetHeightInPixels();
            }
            return 0f;
#else
            return _isBannerShowing ? 50f : 0f;
#endif
        }

        private void UpdateBannerSafeArea()
        {
#if ENABLE_GOOGLE_ADS
            if (_bannerView == null || !_isBannerShowing)
            {
                GoogleAdsSafeAreaHelper.Reset();
                return;
            }

            float heightInPixels = _bannerView.GetHeightInPixels();
            if (_data.BannerPosition == GoogleBannerPosition.Top ||
                _data.BannerPosition == GoogleBannerPosition.TopLeft ||
                _data.BannerPosition == GoogleBannerPosition.TopRight)
            {
                GoogleAdsSafeAreaHelper.TopAdsHeight = heightInPixels;
            }
            else
            {
                GoogleAdsSafeAreaHelper.BottomAdsHeight = heightInPixels;
            }
#endif
        }
        #endregion

        #region Interstitial
        public bool IsInterstitialReady
        {
            get
            {
#if ENABLE_GOOGLE_ADS
                return _interstitialAd != null && _interstitialAd.CanShowAd();
#else
                return true;
#endif
            }
        }

        public void LoadInterstitial()
        {
#if ENABLE_GOOGLE_ADS
            string unitId = TestAdUnits.ResolveInterstitialUnitId(_data.UseTestAds, _data.InterstitialUnitIdAndroid, _data.InterstitialUnitIdIOS);
            _funnelInterstitial.TrackLoadRequest(unitId);

            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            InterstitialAd.Load(unitId, new AdRequest(), (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    _funnelInterstitial.TrackLoadFailed(unitId, error?.GetCode() ?? -1, error?.GetMessage() ?? "Load error");
                    OnInterstitialFailed?.Invoke();
                    return;
                }

                _interstitialAd = ad;
                _funnelInterstitial.TrackLoaded(unitId);

                ad.OnAdPaid += (AdValue adValue) =>
                {
                    double revenue = adValue.Value / 1000000.0;
                    _funnelInterstitial.TrackImpression(revenue, adValue.CurrencyCode, adValue.Precision.ToString());
                    OnAdImpression?.Invoke("interstitial", revenue, adValue.CurrencyCode);
                };

                ad.OnAdImpressionRecorded += () =>
                {
                    OnInterstitialShown?.Invoke();
                };

                ad.OnAdClicked += () =>
                {
                    _funnelInterstitial.TrackClicked();
                };

                ad.OnAdFullScreenContentOpened += () =>
                {
                    _funnelInterstitial.TrackDisplayStart();
                };

                ad.OnAdFullScreenContentClosed += () =>
                {
                    _funnelInterstitial.TrackDisplayEnd();
                    OnInterstitialEnded?.Invoke();
                    OnInterstitialCompleted?.Invoke();

                    _currentInterstitialOnCompleted?.Invoke();
                    _currentInterstitialOnCompleted = null;
                    _currentInterstitialOnFailed = null;

                    _interstitialAd.Destroy();
                    _interstitialAd = null;

                    if (_data.AutoLoadAds) LoadInterstitial();
                };

                ad.OnAdFullScreenContentFailed += (AdError adError) =>
                {
                    _funnelInterstitial.TrackFailedToShow(adError.GetCode(), adError.GetMessage());
                    OnInterstitialFailed?.Invoke();

                    _currentInterstitialOnFailed?.Invoke();
                    _currentInterstitialOnCompleted = null;
                    _currentInterstitialOnFailed = null;

                    _interstitialAd.Destroy();
                    _interstitialAd = null;

                    if (_data.AutoLoadAds) LoadInterstitial();
                };
            });
#else
            Debug.Log("[GoogleMobileAdsManager Mock] LoadInterstitial called.");
#endif
        }

        public void ShowInterstitial(UnityAction onCompleted = null, UnityAction onFailed = null, string placement = "", string requestId = null)
        {
            _funnelInterstitial.TrackShowRequest(placement, requestId);

#if ENABLE_GOOGLE_ADS
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                _currentInterstitialOnCompleted = onCompleted;
                _currentInterstitialOnFailed = onFailed;
                _interstitialAd.Show();
            }
            else
            {
                _funnelInterstitial.TrackFailedToShow(-1, "Interstitial ad is not ready");
                onFailed?.Invoke();
                OnInterstitialFailed?.Invoke();
                if (_data.AutoLoadAds) LoadInterstitial();
            }
#else
            Debug.Log($"[GoogleMobileAdsManager Mock] ShowInterstitial invoked for placement: '{placement}'");
            OnInterstitialShown?.Invoke();
            OnInterstitialCompleted?.Invoke();
            OnInterstitialEnded?.Invoke();
            onCompleted?.Invoke();
#endif
        }

        public async UniTask<bool> WaitForInterstitialReadyAsync(float timeoutSeconds = 10f)
        {
            if (IsInterstitialReady) return true;
            LoadInterstitial();

            float waited = 0f;
            while (waited < timeoutSeconds)
            {
                if (IsInterstitialReady) return true;
                await UniTask.Yield();
                waited += Time.unscaledDeltaTime;
            }
            return IsInterstitialReady;
        }
        #endregion

        #region Rewarded
        public bool IsRewardedReady
        {
            get
            {
#if ENABLE_GOOGLE_ADS
                return _rewardedAd != null && _rewardedAd.CanShowAd();
#else
                return true;
#endif
            }
        }

        public void LoadRewarded()
        {
#if ENABLE_GOOGLE_ADS
            string unitId = TestAdUnits.ResolveRewardedUnitId(_data.UseTestAds, _data.RewardedUnitIdAndroid, _data.RewardedUnitIdIOS);
            _funnelRewarded.TrackLoadRequest(unitId);

            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            RewardedAd.Load(unitId, new AdRequest(), (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    _funnelRewarded.TrackLoadFailed(unitId, error?.GetCode() ?? -1, error?.GetMessage() ?? "Load error");
                    OnRewardedFailed?.Invoke();
                    return;
                }

                _rewardedAd = ad;
                _funnelRewarded.TrackLoaded(unitId);

                ad.OnAdPaid += (AdValue adValue) =>
                {
                    double revenue = adValue.Value / 1000000.0;
                    _funnelRewarded.TrackImpression(revenue, adValue.CurrencyCode, adValue.Precision.ToString());
                    OnAdImpression?.Invoke("rewarded", revenue, adValue.CurrencyCode);
                };

                ad.OnAdImpressionRecorded += () =>
                {
                    OnRewardedShown?.Invoke();
                };

                ad.OnAdClicked += () =>
                {
                    _funnelRewarded.TrackClicked();
                };

                ad.OnAdFullScreenContentOpened += () =>
                {
                    _funnelRewarded.TrackDisplayStart();
                };

                ad.OnAdFullScreenContentClosed += () =>
                {
                    _funnelRewarded.TrackDisplayEnd();
                    OnRewardedEnded?.Invoke();

                    if (_rewardGranted)
                    {
                        OnRewardedCompleted?.Invoke();
                        _currentRewardedOnCompleted?.Invoke();
                    }
                    else
                    {
                        OnRewardedFailed?.Invoke();
                        _currentRewardedOnFailed?.Invoke();
                    }

                    _currentRewardedOnCompleted = null;
                    _currentRewardedOnFailed = null;
                    _rewardGranted = false;

                    _rewardedAd.Destroy();
                    _rewardedAd = null;

                    if (_data.AutoLoadAds) LoadRewarded();
                };

                ad.OnAdFullScreenContentFailed += (AdError adError) =>
                {
                    _funnelRewarded.TrackFailedToShow(adError.GetCode(), adError.GetMessage());
                    OnRewardedFailed?.Invoke();

                    _currentRewardedOnFailed?.Invoke();
                    _currentRewardedOnCompleted = null;
                    _currentRewardedOnFailed = null;
                    _rewardGranted = false;

                    _rewardedAd.Destroy();
                    _rewardedAd = null;

                    if (_data.AutoLoadAds) LoadRewarded();
                };
            });
#else
            Debug.Log("[GoogleMobileAdsManager Mock] LoadRewarded called.");
#endif
        }

        public void ShowRewarded(UnityAction onCompleted, UnityAction onFailed, string placement = "", string requestId = null, long waitMs = 0)
        {
            if (onCompleted == null) throw new ArgumentNullException(nameof(onCompleted), "onCompleted callback cannot be null when showing rewarded ad.");

            _funnelRewarded.TrackShowRequest(placement, requestId);

#if ENABLE_GOOGLE_ADS
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                _currentRewardedOnCompleted = onCompleted;
                _currentRewardedOnFailed = onFailed;
                _rewardGranted = false;

                _rewardedAd.Show((Reward reward) =>
                {
                    _rewardGranted = true;
                    _funnelRewarded.TrackRewardEarned(reward.Type, reward.Amount);
                    OnUserEarnedReward?.Invoke(reward.Type, reward.Amount);
                });
            }
            else
            {
                _funnelRewarded.TrackFailedToShow(-1, "Rewarded ad is not ready");
                onFailed?.Invoke();
                OnRewardedFailed?.Invoke();
                if (_data.AutoLoadAds) LoadRewarded();
            }
#else
            Debug.Log($"[GoogleMobileAdsManager Mock] ShowRewarded invoked for placement: '{placement}'");
            OnRewardedShown?.Invoke();
            OnUserEarnedReward?.Invoke("mock_coins", 100);
            OnRewardedCompleted?.Invoke();
            OnRewardedEnded?.Invoke();
            onCompleted?.Invoke();
#endif
        }

        public async UniTask<bool> WaitForRewardedReadyAsync(float timeoutSeconds = 10f)
        {
            if (IsRewardedReady) return true;
            LoadRewarded();

            float waited = 0f;
            while (waited < timeoutSeconds)
            {
                if (IsRewardedReady) return true;
                await UniTask.Yield();
                waited += Time.unscaledDeltaTime;
            }
            return IsRewardedReady;
        }
        #endregion

        #region Rewarded Interstitial
        public bool IsRewardedInterstitialReady
        {
            get
            {
#if ENABLE_GOOGLE_ADS
                return _rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd();
#else
                return true;
#endif
            }
        }

        public void LoadRewardedInterstitial()
        {
#if ENABLE_GOOGLE_ADS
            string unitId = TestAdUnits.ResolveRewardedInterstitialUnitId(_data.UseTestAds, _data.RewardedInterstitialUnitIdAndroid, _data.RewardedInterstitialUnitIdIOS);
            _funnelRewardedInterstitial.TrackLoadRequest(unitId);

            if (_rewardedInterstitialAd != null)
            {
                _rewardedInterstitialAd.Destroy();
                _rewardedInterstitialAd = null;
            }

            RewardedInterstitialAd.Load(unitId, new AdRequest(), (RewardedInterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    _funnelRewardedInterstitial.TrackLoadFailed(unitId, error?.GetCode() ?? -1, error?.GetMessage() ?? "Load error");
                    return;
                }

                _rewardedInterstitialAd = ad;
                _funnelRewardedInterstitial.TrackLoaded(unitId);

                ad.OnAdPaid += (AdValue adValue) =>
                {
                    double revenue = adValue.Value / 1000000.0;
                    _funnelRewardedInterstitial.TrackImpression(revenue, adValue.CurrencyCode, adValue.Precision.ToString());
                    OnAdImpression?.Invoke("rewarded_interstitial", revenue, adValue.CurrencyCode);
                };

                ad.OnAdFullScreenContentClosed += () =>
                {
                    _funnelRewardedInterstitial.TrackDisplayEnd();
                    if (_rewardedInterstitialGranted)
                    {
                        _currentRewardedInterstitialOnCompleted?.Invoke();
                    }
                    else
                    {
                        _currentRewardedInterstitialOnFailed?.Invoke();
                    }

                    _currentRewardedInterstitialOnCompleted = null;
                    _currentRewardedInterstitialOnFailed = null;
                    _rewardedInterstitialGranted = false;

                    _rewardedInterstitialAd.Destroy();
                    _rewardedInterstitialAd = null;
                };

                ad.OnAdFullScreenContentFailed += (AdError adError) =>
                {
                    _funnelRewardedInterstitial.TrackFailedToShow(adError.GetCode(), adError.GetMessage());
                    _currentRewardedInterstitialOnFailed?.Invoke();
                    _currentRewardedInterstitialOnCompleted = null;
                    _currentRewardedInterstitialOnFailed = null;
                    _rewardedInterstitialGranted = false;

                    _rewardedInterstitialAd.Destroy();
                    _rewardedInterstitialAd = null;
                };
            });
#endif
        }

        public void ShowRewardedInterstitial(UnityAction onCompleted, UnityAction onFailed, string placement = "")
        {
            _funnelRewardedInterstitial.TrackShowRequest(placement);

#if ENABLE_GOOGLE_ADS
            if (_rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd())
            {
                _currentRewardedInterstitialOnCompleted = onCompleted;
                _currentRewardedInterstitialOnFailed = onFailed;
                _rewardedInterstitialGranted = false;

                _rewardedInterstitialAd.Show((Reward reward) =>
                {
                    _rewardedInterstitialGranted = true;
                    _funnelRewardedInterstitial.TrackRewardEarned(reward.Type, reward.Amount);
                    OnUserEarnedReward?.Invoke(reward.Type, reward.Amount);
                });
            }
            else
            {
                _funnelRewardedInterstitial.TrackFailedToShow(-1, "Rewarded Interstitial not ready");
                onFailed?.Invoke();
            }
#else
            Debug.Log($"[GoogleMobileAdsManager Mock] ShowRewardedInterstitial invoked for placement: '{placement}'");
            onCompleted?.Invoke();
#endif
        }
        #endregion

        #region App Open
        public bool IsAppOpenReady
        {
            get
            {
#if ENABLE_GOOGLE_ADS
                return _appOpenAd != null && _appOpenAd.CanShowAd();
#else
                return true;
#endif
            }
        }

        public void LoadAppOpenAd()
        {
#if ENABLE_GOOGLE_ADS
            string unitId = TestAdUnits.ResolveAppOpenUnitId(_data.UseTestAds, _data.AppOpenUnitIdAndroid, _data.AppOpenUnitIdIOS);
            _funnelAppOpen.TrackLoadRequest(unitId);

            if (_appOpenAd != null)
            {
                _appOpenAd.Destroy();
                _appOpenAd = null;
            }

            AppOpenAd.Load(unitId, new AdRequest(), (AppOpenAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    _funnelAppOpen.TrackLoadFailed(unitId, error?.GetCode() ?? -1, error?.GetMessage() ?? "Load error");
                    return;
                }

                _appOpenAd = ad;
                _funnelAppOpen.TrackLoaded(unitId);

                ad.OnAdPaid += (AdValue adValue) =>
                {
                    double revenue = adValue.Value / 1000000.0;
                    _funnelAppOpen.TrackImpression(revenue, adValue.CurrencyCode, adValue.Precision.ToString());
                    OnAdImpression?.Invoke("app_open", revenue, adValue.CurrencyCode);
                };

                ad.OnAdFullScreenContentClosed += () =>
                {
                    _funnelAppOpen.TrackDisplayEnd();
                    _currentAppOpenOnCompleted?.Invoke();
                    _currentAppOpenOnCompleted = null;
                    _currentAppOpenOnFailed = null;

                    _appOpenAd.Destroy();
                    _appOpenAd = null;
                };

                ad.OnAdFullScreenContentFailed += (AdError adError) =>
                {
                    _funnelAppOpen.TrackFailedToShow(adError.GetCode(), adError.GetMessage());
                    _currentAppOpenOnFailed?.Invoke();
                    _currentAppOpenOnCompleted = null;
                    _currentAppOpenOnFailed = null;

                    _appOpenAd.Destroy();
                    _appOpenAd = null;
                };
            });
#endif
        }

        public void ShowAppOpenAd(UnityAction onCompleted = null, UnityAction onFailed = null)
        {
            _funnelAppOpen.TrackShowRequest("app_open");

#if ENABLE_GOOGLE_ADS
            if (_appOpenAd != null && _appOpenAd.CanShowAd())
            {
                _currentAppOpenOnCompleted = onCompleted;
                _currentAppOpenOnFailed = onFailed;
                _appOpenAd.Show();
            }
            else
            {
                _funnelAppOpen.TrackFailedToShow(-1, "App Open ad not ready");
                onFailed?.Invoke();
            }
#else
            Debug.Log("[GoogleMobileAdsManager Mock] ShowAppOpenAd invoked.");
            onCompleted?.Invoke();
#endif
        }
        #endregion
    }
}
