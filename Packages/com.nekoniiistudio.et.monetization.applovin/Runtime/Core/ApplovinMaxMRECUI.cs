#if ENABLE_MAX
using System.Collections;
using UnityEngine;
namespace ET.Monetization
{
    public class ApplovinMaxMRECUI : MonoBehaviour
    {
        [SerializeField] private ApplovinDummyAds _applovinDummyAds;
        private bool _adsShowed;
        private Canvas _parrentCanvas;
        private ApplovinMaxManager _applovinMaxManager;
        private bool isInitted = false;
        public void Init(ApplovinMaxManager applovinMaxManager, Canvas parrentCanvas)
        {
            if (!isInitted)
            {
                _parrentCanvas = parrentCanvas;
                _applovinMaxManager = applovinMaxManager;
                isInitted = true;
                // _applovinDummyAds.Hide();
            }
        }
        public void ShowAds()
        {
            ShowPlaceholder();
            ShowNative();
        }

        // The placeholder is a plain UI rect inside the popup, so it can animate with
        // the popup; the native MREC view sits above Unity and cannot, so show it
        // only once the popup has settled (DidPushEnter).
        public void ShowPlaceholder()
        {
#if UNITY_EDITOR
            _applovinDummyAds.Show(_parrentCanvas, 300, 250, transparent: false);
#else
            _applovinDummyAds.Show(_parrentCanvas, 300, 250, transparent: true);
#endif
        }

        public void ShowNative()
        {
            StartCoroutine(DelayShowAds(0f));
        }
        public void HideAds()
        {
            StopAllCoroutines();
            // Always hide ads regardless of flag state to prevent race condition
            // where coroutine is stopped before _adsShowed is set to true
            _applovinDummyAds.Hide();
            _applovinMaxManager.HideMRecAds();
            _adsShowed = false;
        }
        IEnumerator DelayShowAds(float delay)
        {
            if (!_adsShowed)
            {
                if (delay > 0f)
                {
                    yield return new WaitForSeconds(delay);
                }
                else
                {
                    yield return new WaitForEndOfFrame();
                }
                _adsShowed = true;
                if (!_applovinDummyAds.gameObject.activeSelf)
                {
                    ShowPlaceholder();
                    yield return new WaitForEndOfFrame();
                }
                var position = _applovinDummyAds.GetAdsPosition(_parrentCanvas);

                // Use MaxSdkUtils density for both X and Y dp calculations
                const float mrecWidthDp = 300f;
                float density = MaxSdkUtils.GetScreenDensity();
                float centeredXDp = (Screen.width / density * 0.5f) - (mrecWidthDp * 0.5f);
                float centeredXPixels = centeredXDp * density;
                float yDp = position.Item1.y / density;

                var centeredUnityPos = new Vector2(centeredXPixels, position.Item1.y);
                var centeredApplovinPos = new Vector2(centeredXDp, yDp);
                _applovinMaxManager.ShowMRecAds(centeredUnityPos, centeredApplovinPos);
            }
        }
    }
}
#endif
