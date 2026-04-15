using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public static class SafeAreaHelper
    {
        private static float _bottomAdsHeight;
        public static float BottomAdsHeight
        {
            get => _bottomAdsHeight;
            set
            {
                _bottomAdsHeight = value;
                OnBottomAdsHeightChanged?.Invoke();
            }
        }
        public static event Action OnBottomAdsHeightChanged;
    }
}
