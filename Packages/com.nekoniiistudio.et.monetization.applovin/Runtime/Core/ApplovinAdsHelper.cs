using System;
using UnityEngine;

namespace ET.Monetization
{
    public static class SafeAreaHelper
    {
        private static float _bottomAdsHeight;
        public static float BottomAdsHeight
        {
            get => _bottomAdsHeight;
            set
            {
                // Only notify on a real change: the banner height is now re-read a few times after
                // the ad loads (the iOS frame settles a frame or two late), and every listener
                // rebuilds a RectTransform on this event.
                if (Mathf.Approximately(_bottomAdsHeight, value)) return;

                _bottomAdsHeight = value;
                OnBottomAdsHeightChanged?.Invoke();
            }
        }
        public static event Action OnBottomAdsHeightChanged;
    }
    public static class MRECAreaHelper
    {
        public static Rect AdsRect;
    }
}