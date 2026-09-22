using System;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Monetization
{
    /// <summary>
    /// Visual overlay simulator for testing Google Mobile Ads in the Unity Editor or mock environments.
    /// Follows ET Coding Rules for lazy component retrieval.
    /// </summary>
    public class GoogleMobileAdsDummy : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Image _background;
        private Text _label;

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                    if (_rectTransform == null)
                    {
                        _rectTransform = gameObject.AddComponent<RectTransform>();
                    }
                }
                return _rectTransform;
            }
        }

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                {
                    _canvasGroup = GetComponent<CanvasGroup>();
                    if (_canvasGroup == null)
                    {
                        _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                    }
                }
                return _canvasGroup;
            }
        }

        public void ShowBannerMock(Canvas parentCanvas, int bannerWidth = 320, int bannerHeight = 50, GoogleBannerPosition position = GoogleBannerPosition.Bottom)
        {
            float dpi = Screen.dpi > 0 ? Screen.dpi : 160f;
            float density = dpi / 160f;

            float pixelWidth = bannerWidth * density;
            float pixelHeight = bannerHeight * density;

            float scaleX = parentCanvas != null ? parentCanvas.transform.localScale.x : 1f;
            if (scaleX <= 0.0001f) scaleX = 1f;

            float canvasWidth = pixelWidth * scaleX;
            float canvasHeight = pixelHeight * scaleX;

            RectTransform.sizeDelta = new Vector2(canvasWidth, canvasHeight);

            // Anchor according to position
            if (position == GoogleBannerPosition.Top || position == GoogleBannerPosition.TopLeft || position == GoogleBannerPosition.TopRight)
            {
                RectTransform.anchorMin = new Vector2(0.5f, 1f);
                RectTransform.anchorMax = new Vector2(0.5f, 1f);
                RectTransform.pivot = new Vector2(0.5f, 1f);
                RectTransform.anchoredPosition = Vector2.zero;
                GoogleAdsSafeAreaHelper.TopAdsHeight = pixelHeight;
            }
            else
            {
                RectTransform.anchorMin = new Vector2(0.5f, 0f);
                RectTransform.anchorMax = new Vector2(0.5f, 0f);
                RectTransform.pivot = new Vector2(0.5f, 0f);
                RectTransform.anchoredPosition = Vector2.zero;
                GoogleAdsSafeAreaHelper.BottomAdsHeight = pixelHeight;
            }

            gameObject.SetActive(true);
        }

        public void HideMock()
        {
            gameObject.SetActive(false);
            GoogleAdsSafeAreaHelper.Reset();
        }
    }
}
