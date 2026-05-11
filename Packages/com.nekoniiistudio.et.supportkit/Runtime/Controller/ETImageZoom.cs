using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace ET
{
    public class ETImageZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image targetImage; // Assign in Inspector
        public ScrollRect scrollRect;
        private bool isPointerOver = false;

        private float previousDistance = 0f;
        private float minimumZoom;
        private Vector3 touchStart;
        [SerializeField] private RectTransform _parrentRect;

        public float MinimumZoom => minimumZoom;
        private void Start()
        {
            minimumZoom = (float)_parrentRect.rect.height / 2000;
            SetBeginZoom();
            //minimumZoom = 1.11f;
        }
        void Update()
        {
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR || UNITY_STANDALONE
            // Block movement when more than 2 touches are detected
            if (Input.touchCount > 2)
            {
                if (scrollRect != null)
                    scrollRect.enabled = false;
                return;
            }
            else
            {
                if (scrollRect != null)
                    scrollRect.enabled = true;
            }
#endif

            if (!isPointerOver) return;

#if UNITY_EDITOR || UNITY_STANDALONE
            // Zoom using scroll wheel (mouse)
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                Zoom(scroll * 10f);
            }
#elif UNITY_ANDROID || UNITY_IOS
        // Zoom using pinch (touch)

            if (Input.GetMouseButtonDown(0))
            {
                touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            if (Input.touchCount == 2)
            {
            
                scrollRect.enabled = false;
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

                float difference = currentMagnitude - prevMagnitude;

                ZoomPhone(difference * 0.01f);
            }
            else if (Input.GetMouseButton(0))
            {
                //Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                //gameObject.transform.position -= direction;
            }
            //ZoomPhone(Input.GetAxis("Mouse ScrollWheel"));
            //    scrollRect.enabled = true;
#endif
        }

        private void Zoom(float delta)
        {
            if (targetImage == null) return;

            Vector3 scale = targetImage.rectTransform.localScale;
            scale += Vector3.one * delta * 0.01f;
            scale = ClampScale(scale, minimumZoom, 3f);
            targetImage.rectTransform.localScale = scale;
        }

        // [ContextMenu("ZoomPhone")]
        // public void test()
        // {
        //     ZoomPhone(2000f, 2000f);
        // }

        public void ZoomPhone(float increment, float max = 3f)
        {
            RectTransform rt = transform as RectTransform;
            if (rt == null) return;

            float factor = Mathf.Clamp(rt.localScale.x + increment * 0.1f, minimumZoom, max);
            Vector3 newScale = new Vector3(factor, factor, 1f);

            Vector2 screenPos = Vector2.zero;
            bool hasCenter = false;
#if UNITY_EDITOR
            if (Input.GetMouseButton(0))
            {
                screenPos = Input.mousePosition;
                hasCenter = true;
            }
#else
            if (Input.touchCount >= 2)
            {
                screenPos = (Input.GetTouch(0).position + Input.GetTouch(1).position) * 0.5f;
                hasCenter = true;
            }
            else if (Input.touchCount == 1)
            {
                screenPos = Input.GetTouch(0).position;
                hasCenter = true;
            }
#endif

            if (hasCenter)
            {
                RectTransform parent = rt.parent as RectTransform;
                if (parent != null)
                {
                    Vector2 localPoint;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPos, null, out localPoint);
                    Vector3 worldBefore = parent.TransformPoint(localPoint);
                    Vector3 pivotLocal = rt.InverseTransformPoint(worldBefore);

                    rt.localScale = newScale;

                    Vector3 worldAfter = rt.TransformPoint(pivotLocal);
                    Vector3 offset = worldBefore - worldAfter;
                    rt.position += offset;
                    return;
                }
            }

            rt.localScale = newScale;
        }
        private void SetBeginZoom()
        {
            targetImage.rectTransform.localScale = ClampScale(Vector3.zero, minimumZoom, 2.5f);
        }

        private Vector3 ClampScale(Vector3 scale, float min, float max)
        {
            scale.x = Mathf.Clamp(scale.x, min, max);
            scale.y = Mathf.Clamp(scale.y, min, max);
            scale.z = 1f;
            return scale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerEnter == targetImage.gameObject)
                isPointerOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerOver = false;
        }
        public void ResetZoom()
        {
            if (targetImage == null) return;
            targetImage.rectTransform.localScale = new Vector3(minimumZoom, minimumZoom, 1f);
        }
    }
}

