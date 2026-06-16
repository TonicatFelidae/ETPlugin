using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ET.SupportKit;

namespace ET.UIKit
{
    [RequireComponent(typeof(ScrollRect))]
    public class UISScrollView : MonoBehaviour
    {
        [Header("Scroll view fixer")]
        [Tooltip("Enable fixed content height for GridLayout content.")]
        [SerializeField] private bool _isFixContentHeight;
        [Tooltip("Maximum number of child items allowed in the content area.")]
        [SerializeField] private int _limitContent = 0;
        [Tooltip("Enable fixed text content height for TextMeshPro content.")]
        [SerializeField] private bool _isFixContentHeightTMP;

        [Header("Auto scroll settings for TMP content")]
        [Tooltip("Automatically scroll content when TMP text height exceeds the view.")]
        [SerializeField] private bool _isAutoScrollTMP;
        [Tooltip("Delay before auto-scrolling begins, in seconds.")]
        [SerializeField] private float _scrollDelay;
        [Tooltip("Scroll speed used when auto-scrolling TMP content.")]
        [SerializeField] private float _scrollSpeed;

        private Transform _content;
        private float _curSpaceY;
        private float _spaceY;
        private float _viewH = 60f;
        private float _viewScrollH;

        private void Start()
        {
            _curSpaceY = _spaceY;
            _content = transform.Find("Viewport").Find("Content").transform;
        }

        private void OnEnable()
        {
            _curSpaceY = _spaceY = 0;
        }

        private void FixedUpdate()
        {
            if (_limitContent > 0 && _content != null && _content.childCount > _limitContent)
            {
                Destroy(_content.GetChild(0).gameObject);
            }
        }

        private void Update()
        {
            if (_content == null)
            {
                return;
            }

            if (_isFixContentHeightTMP && _content.childCount > 0)
            {
                _spaceY = _content.GetChild(0).GetComponent<TextMeshProUGUI>().textBounds.size.y;
                if (_spaceY != _curSpaceY)
                {
                    RectTransform contentRect = _content.GetComponent<RectTransform>();
                    contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, _spaceY);
                    contentRect.localPosition = Vector2.zero;

                    if (_isAutoScrollTMP)
                    {
                        _viewScrollH = _spaceY - _viewH;
                        if (_viewScrollH > 0)
                        {
                            StartCoroutine(ScrollContent());
                        }
                    }

                    _curSpaceY = _spaceY;
                }
            }

            if (_isFixContentHeight)
            {
                GridLayoutGroup gridLayout = _content.GetComponent<GridLayoutGroup>();
                RectTransform contentRect = _content.GetComponent<RectTransform>();

                float inSpaceY = gridLayout.spacing.y;
                float inSizeY = gridLayout.cellSize.y;
                float padUp = gridLayout.padding.top;
                float padDown = gridLayout.padding.bottom;

                Vector2 sizeDelta = contentRect.sizeDelta;
                int childCount = ET_Transform.active_child_count(_content);
                sizeDelta.y = inSizeY * childCount + (childCount - 1) * inSpaceY + padUp + padDown;

                contentRect.sizeDelta = sizeDelta;
            }
        }

        private IEnumerator ScrollContent()
        {
            yield return new WaitForSeconds(_scrollDelay);

            RectTransform contentRect = _content.GetComponent<RectTransform>();
            while (contentRect.localPosition.y < _viewScrollH)
            {
                float curY = contentRect.localPosition.y;
                contentRect.localPosition = new Vector2(0, curY + _scrollSpeed);
                yield return new WaitForEndOfFrame();
            }

            yield return StartCoroutine(ScrollContent());
        }
    }
}
