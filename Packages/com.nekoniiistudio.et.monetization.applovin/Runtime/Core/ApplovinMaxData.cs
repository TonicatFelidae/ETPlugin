using System;
using UnityEngine;

namespace ET.Monetization
{
    [Serializable]
    public class ApplovinMaxData
    {
        public string ADS_ID_rewarded;
        public string ADS_ID_interstitial;
        public string ADS_ID_banner;
        public string ADS_ID_MREC;
        public bool InitBanner = true;
        public bool InitMREC = false;
    }
}
