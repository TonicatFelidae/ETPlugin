using ET.ETAds;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ET.ETAds
{
    public interface IAdsUnit
    {
        /// <summary>
        /// get ads size in pixel for UI adjustment
        /// </summary>
        public Vector2 ADsSizeInPixel { get; }

        public AdType adType { get; }
        public AdPosition adPosition { get; }   
        /// <summary>
        /// ID to distinguish between ads of the same type
        /// why int?? so I can use enum index on it
        /// </summary>
        public int adID { get; }
        /// <summary>
        /// Init ads ID, we should not include rewardPlayer action here because it should be in show(), the ads is the same but reward varies
        /// </summary>
        public void Init(string adsID, AdPosition adsPosition);
        /// <summary>
        /// Create ads, and no im not care shit about ads sizem only position
        /// use in case:
        /// want to create but not show up [rare] 
        /// want to implement different ID
        /// ...
        /// otherwise should use show instead
        /// </summary>
        public void Create(AdType adType, int adID = 0);
        /// <summary>
        /// Creates the banner view and loads a banner ad.
        /// </summary>
        public void Show(UnityAction rewardPlayer = null);
        /// <summary>
        /// Destroys the ad. Impostant for reward ads
        /// </summary>
        public void DestroyAd();
    }
}
