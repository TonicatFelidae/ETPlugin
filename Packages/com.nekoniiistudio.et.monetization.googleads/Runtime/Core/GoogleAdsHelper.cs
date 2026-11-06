using System;
using UnityEngine;
#if ENABLE_GOOGLE_ADS
using GoogleMobileAds.Api;
#endif

namespace ET.Monetization
{
    /// <summary>
    /// Global notification coordinator for safe-area adjustments based on banner visibility and height.
    /// UI layout components like GoogleAdsSafeArea observe these values.
    /// </summary>
    public static class GoogleAdsSafeAreaHelper
    {
        private static float _bottomAdsHeight;
        private static float _topAdsHeight;

        public static float BottomAdsHeight
        {
            get => _bottomAdsHeight;
            set
            {
                if (Mathf.Approximately(_bottomAdsHeight, value)) return;
                _bottomAdsHeight = value;
                OnBottomAdsHeightChanged?.Invoke();
            }
        }

        public static float TopAdsHeight
        {
            get => _topAdsHeight;
            set
            {
                if (Mathf.Approximately(_topAdsHeight, value)) return;
                _topAdsHeight = value;
                OnTopAdsHeightChanged?.Invoke();
            }
        }

        public static event Action OnBottomAdsHeightChanged;
        public static event Action OnTopAdsHeightChanged;

        public static void Reset()
        {
            BottomAdsHeight = 0f;
            TopAdsHeight = 0f;
        }
    }

    /// <summary>
    /// Utility converters for Google Mobile Ads positions and measurements.
    /// </summary>
    public static class GoogleAdsUtils
    {
#if ENABLE_GOOGLE_ADS
        public static AdPosition ConvertPosition(GoogleBannerPosition position)
        {
            return position switch
            {
                GoogleBannerPosition.Top => AdPosition.Top,
                GoogleBannerPosition.Bottom => AdPosition.Bottom,
                GoogleBannerPosition.TopLeft => AdPosition.TopLeft,
                GoogleBannerPosition.TopRight => AdPosition.TopRight,
                GoogleBannerPosition.BottomLeft => AdPosition.BottomLeft,
                GoogleBannerPosition.BottomRight => AdPosition.BottomRight,
                GoogleBannerPosition.Center => AdPosition.Center,
                _ => AdPosition.Bottom
            };
        }
#endif
    }
}
