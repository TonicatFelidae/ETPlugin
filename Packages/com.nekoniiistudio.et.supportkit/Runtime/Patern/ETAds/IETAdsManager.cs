using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace ET.ETAds
{
    /// <summary>
    /// IETAdsManager interface blueprint
    /// </summary>
    public interface IETAdsManager
    {
        List<IAdsUnit> AdsUnits { get; set; }
        void ShowAds(AdType adType, UnityAction rewardPlayer = null);
        void ShowAds(AdType adType, int adID, UnityAction rewardPlayer = null);
    }
    public enum AdPosition
    {
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center
    }
    /// <summary>
    /// For ad UI adjsutment
    /// </summary>
    public enum BannerOverallAdPosition
    {
        Top,
        Bottom,
        TopBot,
    }
    public enum AdType
    {
        Banner,
        Interstitial,
        Rewarned,
        AppOpen,
        RewardedInterstitial,
        Native
    }
}
