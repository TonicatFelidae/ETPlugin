using UnityEngine;
using UnityEngine.Events;

namespace ET.Monetization
{
    /// <summary>
    /// Automatically conforms a UI RectTransform to prevent Google Mobile Ads banners
    /// (top or bottom) from overlapping essential UI components.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class GoogleAdsSafeArea : MonoBehaviour
    {
        [Tooltip("Adjust top offset when top banner is displayed.")]
        [SerializeField] private bool _conformTop = true;
        [Tooltip("Adjust bottom offset when bottom banner is displayed.")]
        [SerializeField] private bool _conformBottom = true;
        [Tooltip("Optional Canvas reference. If null, automatically resolves in parents.")]
        [SerializeField] private Canvas _targetCanvas;

        public UnityEvent OnSafeAreaChanged = new UnityEvent();

        private RectTransform _rectTransform;
        private Vector2 _originalOffsetMin;
        private Vector2 _originalOffsetMax;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _originalOffsetMin = _rectTransform.offsetMin;
            _originalOffsetMax = _rectTransform.offsetMax;

            if (_targetCanvas == null)
            {
                _targetCanvas = GetComponentInParent<Canvas>();
            }
        }

        private void OnEnable()
        {
            GoogleAdsSafeAreaHelper.OnBottomAdsHeightChanged += ApplySafeArea;
            GoogleAdsSafeAreaHelper.OnTopAdsHeightChanged += ApplySafeArea;
            ApplySafeArea();
        }

        private void OnDisable()
        {
            GoogleAdsSafeAreaHelper.OnBottomAdsHeightChanged -= ApplySafeArea;
            GoogleAdsSafeAreaHelper.OnTopAdsHeightChanged -= ApplySafeArea;
        }

        /// <summary>
        /// Recalculates canvas scale and updates rectTransform offsets.
        /// </summary>
        public void ApplySafeArea()
        {
            if (_rectTransform == null) return;

            float canvasScaleY = 1f;
            if (_targetCanvas != null)
            {
                canvasScaleY = _targetCanvas.transform.localScale.y;
            }
            if (canvasScaleY <= 0.0001f) canvasScaleY = 1f;

            float bottomOffset = GoogleAdsSafeAreaHelper.BottomAdsHeight / canvasScaleY;
            float topOffset = GoogleAdsSafeAreaHelper.TopAdsHeight / canvasScaleY;

            Vector2 newOffsetMin = _originalOffsetMin;
            Vector2 newOffsetMax = _originalOffsetMax;

            if (_conformBottom && bottomOffset > 0f)
            {
                newOffsetMin = new Vector2(_originalOffsetMin.x, _originalOffsetMin.y + bottomOffset);
            }

            if (_conformTop && topOffset > 0f)
            {
                newOffsetMax = new Vector2(_originalOffsetMax.x, _originalOffsetMax.y - topOffset);
            }

            _rectTransform.offsetMin = newOffsetMin;
            _rectTransform.offsetMax = newOffsetMax;

            OnSafeAreaChanged?.Invoke();
        }
    }
}
