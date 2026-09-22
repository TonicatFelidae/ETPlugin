namespace ET.Monetization
{
    /// <summary>
    /// Official Google Mobile Ads (AdMob) test ad unit IDs for Android and iOS.
    /// Reference: https://developers.google.com/admob/unity/test-ads
    /// </summary>
    public static class TestAdUnits
    {
        public static class Android
        {
            public const string AppOpen = "ca-app-pub-3940256099942544/3419835294";
            public const string Banner = "ca-app-pub-3940256099942544/6300978111";
            public const string Interstitial = "ca-app-pub-3940256099942544/1033173712";
            public const string Rewarded = "ca-app-pub-3940256099942544/5224354917";
            public const string RewardedInterstitial = "ca-app-pub-3940256099942544/5354046379";
            public const string Native = "ca-app-pub-3940256099942544/2247696110";
        }

        public static class IOS
        {
            public const string AppOpen = "ca-app-pub-3940256099942544/5662855259";
            public const string Banner = "ca-app-pub-3940256099942544/2934735716";
            public const string Interstitial = "ca-app-pub-3940256099942544/4411468910";
            public const string Rewarded = "ca-app-pub-3940256099942544/1712485313";
            public const string RewardedInterstitial = "ca-app-pub-3940256099942544/6978759866";
            public const string Native = "ca-app-pub-3940256099942544/3986624511";
        }

        public static string ResolveBannerUnitId(bool useTestAds, string customAndroid = null, string customIOS = null)
        {
            if (useTestAds)
            {
#if UNITY_IOS
                return IOS.Banner;
#else
                return Android.Banner;
#endif
            }
#if UNITY_IOS
            return string.IsNullOrEmpty(customIOS) ? IOS.Banner : customIOS;
#else
            return string.IsNullOrEmpty(customAndroid) ? Android.Banner : customAndroid;
#endif
        }

        public static string ResolveInterstitialUnitId(bool useTestAds, string customAndroid = null, string customIOS = null)
        {
            if (useTestAds)
            {
#if UNITY_IOS
                return IOS.Interstitial;
#else
                return Android.Interstitial;
#endif
            }
#if UNITY_IOS
            return string.IsNullOrEmpty(customIOS) ? IOS.Interstitial : customIOS;
#else
            return string.IsNullOrEmpty(customAndroid) ? Android.Interstitial : customAndroid;
#endif
        }

        public static string ResolveRewardedUnitId(bool useTestAds, string customAndroid = null, string customIOS = null)
        {
            if (useTestAds)
            {
#if UNITY_IOS
                return IOS.Rewarded;
#else
                return Android.Rewarded;
#endif
            }
#if UNITY_IOS
            return string.IsNullOrEmpty(customIOS) ? IOS.Rewarded : customIOS;
#else
            return string.IsNullOrEmpty(customAndroid) ? Android.Rewarded : customAndroid;
#endif
        }

        public static string ResolveRewardedInterstitialUnitId(bool useTestAds, string customAndroid = null, string customIOS = null)
        {
            if (useTestAds)
            {
#if UNITY_IOS
                return IOS.RewardedInterstitial;
#else
                return Android.RewardedInterstitial;
#endif
            }
#if UNITY_IOS
            return string.IsNullOrEmpty(customIOS) ? IOS.RewardedInterstitial : customIOS;
#else
            return string.IsNullOrEmpty(customAndroid) ? Android.RewardedInterstitial : customAndroid;
#endif
        }

        public static string ResolveAppOpenUnitId(bool useTestAds, string customAndroid = null, string customIOS = null)
        {
            if (useTestAds)
            {
#if UNITY_IOS
                return IOS.AppOpen;
#else
                return Android.AppOpen;
#endif
            }
#if UNITY_IOS
            return string.IsNullOrEmpty(customIOS) ? IOS.AppOpen : customIOS;
#else
            return string.IsNullOrEmpty(customAndroid) ? Android.AppOpen : customAndroid;
#endif
        }
    }
}
