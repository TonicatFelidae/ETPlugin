using System;
using UnityEngine;

namespace ET.Monetization
{
    /// <summary>
    /// Configuration data container for Google Mobile Ads (AdMob).
    /// Holds platform-specific Ad Unit IDs, general configuration flags,
    /// child-directed treatment parameters, and banner placement options.
    /// </summary>
    [Serializable]
    public class GoogleMobileAdsData
    {
        [Header("Android Ad Unit IDs")]
        [Tooltip("Ad Unit ID for Android Banner. Leave blank to use official test ID when UseTestAds is enabled.")]
        public string BannerUnitIdAndroid;
        [Tooltip("Ad Unit ID for Android Interstitial.")]
        public string InterstitialUnitIdAndroid;
        [Tooltip("Ad Unit ID for Android Rewarded.")]
        public string RewardedUnitIdAndroid;
        [Tooltip("Ad Unit ID for Android Rewarded Interstitial.")]
        public string RewardedInterstitialUnitIdAndroid;
        [Tooltip("Ad Unit ID for Android App Open.")]
        public string AppOpenUnitIdAndroid;

        [Header("iOS Ad Unit IDs")]
        [Tooltip("Ad Unit ID for iOS Banner. Leave blank to use official test ID when UseTestAds is enabled.")]
        public string BannerUnitIdIOS;
        [Tooltip("Ad Unit ID for iOS Interstitial.")]
        public string InterstitialUnitIdIOS;
        [Tooltip("Ad Unit ID for iOS Rewarded.")]
        public string RewardedUnitIdIOS;
        [Tooltip("Ad Unit ID for iOS Rewarded Interstitial.")]
        public string RewardedInterstitialUnitIdIOS;
        [Tooltip("Ad Unit ID for iOS App Open.")]
        public string AppOpenUnitIdIOS;

        [Header("General Settings")]
        [Tooltip("When enabled, uses Google's verified test ad unit IDs.")]
        public bool UseTestAds = true;
        [Tooltip("Automatically initialize MobileAds SDK on startup.")]
        public bool AutoInitialize = true;
        [Tooltip("Automatically load interstitial, rewarded, and banner ads upon successful initialization.")]
        public bool AutoLoadAds = true;

        [Header("COPPA & Privacy Configurations")]
        [Tooltip("Tag for child-directed treatment (COPPA compliance).")]
        public bool IsTagForChildDirectedTreatment = false;
        [Tooltip("Tag for users under the age of consent in the European Economic Area (EEA).")]
        public bool TagForUnderAgeOfConsent = false;
        [Tooltip("Maximum ad content rating filter: 'G', 'PG', 'T', 'MA', or leave empty for default.")]
        public string MaxAdContentRating = "";

        [Header("Banner Options")]
        [Tooltip("Whether to create and load the banner ad immediately on initialization.")]
        public bool InitBanner = true;
        [Tooltip("Use anchored adaptive banner sizing matching screen width.")]
        public bool AdaptiveBanner = true;
        [Tooltip("Screen position where banner will be anchored.")]
        public GoogleBannerPosition BannerPosition = GoogleBannerPosition.Bottom;
    }

    /// <summary>
    /// Supported anchor positions for Google Mobile Ads banner views.
    /// </summary>
    public enum GoogleBannerPosition
    {
        Top = 0,
        Bottom = 1,
        TopLeft = 2,
        TopRight = 3,
        BottomLeft = 4,
        BottomRight = 5,
        Center = 6
    }
}
