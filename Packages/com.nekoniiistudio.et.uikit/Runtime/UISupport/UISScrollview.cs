using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ET.SupportKit;

namespace ET.UIKit
{
    [RequireComponent(typeof(ScrollRect), typeof(RectTransform))]
    public class UISScrollView : MonoBehaviour
    {
        [Header("Limit content child count settings")]
        [Tooltip("Maximum number of child items allowed in the content area.")]
        [SerializeField] private bool _autoLimitContent;
        [SerializeField] private int _limitContent = 0;

        [Header("Auto scroll settings for TMP content")]
        [Tooltip("Automatically scroll content when TMP text height exceeds the view.")]
        [SerializeField] private bool _isAutoScrollTMP;
        [Tooltip("Whether to reset scroll position to top after reaching the end.")]
        [SerializeField] private bool _resetToTopAfterScroll = false;
        [Tooltip("Delay before auto-scrolling begins, in seconds.")]
        [SerializeField] private float _scrollDelay = 1;
        [Tooltip("Scroll speed used when auto-scrolling TMP content.")]
        [SerializeField] private float _scrollSpeed = 20;
        [Header("REFERENCES")]
        private RectTransform _content;
        private RectTransform _viewRect;

        private void Start()
        {
            _viewRect = GetComponent<RectTransform>();
            // Ensure we have the content RectTransform from the ScrollRect
            var scrollRect = GetComponent<ScrollRect>();
            if (scrollRect != null)
            {
                _content = scrollRect.content;
            }

            if (_isAutoScrollTMP && _content != null) AutoScrollContent();

        }

        private void FixedUpdate()
        {
            if (_autoLimitContent && _limitContent > 0 && _content != null && _content.childCount > _limitContent)
            {
                Destroy(_content.GetChild(0).gameObject);
            }
        }
        public void AutoScrollContent() => StartCoroutine(ScrollContent());
        private IEnumerator ScrollContent()
        {
            // Run continuously: wait, then scroll if content is taller than view
            while (true)
            {
                yield return new WaitForSeconds(_scrollDelay);

                if (_content == null || _viewRect == null)
                    yield break;

                float contentHeight = _content.rect.height;
                float viewHeight = _viewRect.rect.height;

                if (contentHeight <= viewHeight)
                    continue;

                float maxY = contentHeight - viewHeight;

                // Scroll by increasing the anchoredPosition.y until the max is reached
                while (_content.anchoredPosition.y < maxY)
                {
                    Vector2 ap = _content.anchoredPosition;
                    ap.y = Mathf.Min(maxY, ap.y + _scrollSpeed * Time.deltaTime);
                    _content.anchoredPosition = ap;
                    yield return null;
                }

                // Optionally reset to top and repeat
                if (_resetToTopAfterScroll)
                {
                    _content.anchoredPosition = Vector2.zero;
                }
            }
        }
        public void FocusOnTopContent()
        {
            if (_content != null)
            {
                RectTransform contentRect = _content.GetComponent<RectTransform>();
                contentRect.anchoredPosition = Vector2.zero;
            }
        }
    }
}
