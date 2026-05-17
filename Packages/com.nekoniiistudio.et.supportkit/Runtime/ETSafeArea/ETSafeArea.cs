using UnityEngine;
using System;

namespace ET
{

    [RequireComponent(typeof(RectTransform))]
    public class ETSafeArea : MonoBehaviour
    {
        [Flags]
        public enum SafeAreaOptions
        {
            None = 0,
            Top = 1 << 0,
            Bottom = 1 << 1,
            All = Top | Bottom
        }

        [SerializeField]
        private SafeAreaOptions safeAreaOptions = SafeAreaOptions.All;

        private RectTransform _rect;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            ApplySafeArea();
            SafeAreaHelper.OnBottomAdsHeightChanged += OnBottomAdsHeightChanged;
        }

        private void OnDisable()
        {
            SafeAreaHelper.OnBottomAdsHeightChanged -= OnBottomAdsHeightChanged;
        }

        private void OnBottomAdsHeightChanged()
        {
            ApplySafeArea();
        }

        [ContextMenu("ApplySafeArea")]
        private void ApplySafeArea()
        {
            Rect safe = Screen.safeArea;
            Debug.Log($"Screen.safeArea: {safe.y} {SafeAreaHelper.BottomAdsHeight}");
            safe.y = Mathf.Max(safe.y, SafeAreaHelper.BottomAdsHeight);
            float pixelsPerUnit = (float)Screen.height / safe.height;
            float bottomOverlap = 0;
            if (SafeAreaHelper.BottomAdsHeight > Screen.safeArea.y)
                bottomOverlap = SafeAreaHelper.BottomAdsHeight - Screen.safeArea.y;
            safe.height -= Mathf.Max(0, bottomOverlap);

            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            Vector2 anchorMin = safe.position;
            Vector2 anchorMax = safe.position + safe.size;
            anchorMin.x /= screenSize.x;
            anchorMax.x /= screenSize.x;
            anchorMin.y /= screenSize.y;
            anchorMax.y /= screenSize.y;

            if ((safeAreaOptions & SafeAreaOptions.Bottom) == 0)
                anchorMin.y = 0f;
            if ((safeAreaOptions & SafeAreaOptions.Top) == 0)
                anchorMax.y = 1f;

            _rect.anchorMin = anchorMin;
            _rect.anchorMax = anchorMax;
            _rect.anchoredPosition = Vector2.zero;
            _rect.sizeDelta = Vector2.zero;
        }
    }

}
