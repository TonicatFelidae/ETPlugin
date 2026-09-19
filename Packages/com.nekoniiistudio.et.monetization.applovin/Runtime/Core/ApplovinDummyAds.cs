using UnityEngine;
using UnityEngine.UI;
namespace ET.Monetization
{
    public class ApplovinDummyAds: MonoBehaviour
    {
        /// <summary>
        /// DEPEND ON X ASIX ONLY !!! Look at canvas scale width/height = 0
        /// </summary>
        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>(); 
                }
                return _rectTransform;
            }
        }
        private RectTransform _rectTransform;
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                {
                    _canvasGroup = GetComponent<CanvasGroup>();
                }
                return _canvasGroup;
            }
        }
        private CanvasGroup _canvasGroup;
        /// <summary>
        /// transparent can help with layout alignment
        /// </summary>
        /// <param name="originSizeX"></param>
        /// <param name="originSizeY"></param>
        /// <param name="transparent"></param>
        public void Show(Canvas parentCanvas,
            int originSizeX, int originSizeY, 
            bool transparent = false)
        {
            (float dpi, float density) = GetDPIAndDensity();
            // Max gave wrong destiny cal on editor, prefer use  dpi / 160f =>
            //density = MaxSdkUtils.GetScreenDensity();
            //Debug.Log($"dpi = {dpi} density = {density}");

            float pixelWidth = originSizeX * density;
            float pixelHeight = originSizeY * density;

            (float scaleX, float scaleY) = GetCanvasScale(parentCanvas);

            float canvasWidth = pixelWidth * scaleX;
            float canvasHeight = canvasWidth * originSizeY / originSizeX;
            //float canvasHeight = pixelHeight * scaleY;

#if UNITY_IOS && !UNITY_EDITOR
            // Add 10px padding top and bottom on iOS
            float paddingPixels = 20f * density; // 10px top + 10px bottom
            canvasHeight += paddingPixels * scaleX;
#endif

            RectTransform.sizeDelta = new Vector2(canvasWidth, canvasHeight);
            ApplyTransparent(transparent);
            gameObject.SetActive(true);
        }
        public (float, float) GetDPIAndDensity()
        {
            float dpi = Screen.dpi;
            if (dpi == 0)
            {
                dpi = 160f;
            }
            float density = dpi / 160f;
            Debug.Log($"dpi = {dpi} density = {density}");
            return (dpi, density);
        }
        public (float,float) GetCanvasScale(Canvas parentCanvas)
        {
            CanvasScaler scaler = parentCanvas.GetComponent<CanvasScaler>();
            Vector2 referenceResolution = scaler.referenceResolution;

            float scaleX = referenceResolution.x / Screen.width;
            float scaleY = referenceResolution.y / Screen.height;

            return (scaleX, scaleY);    
        }
        /// <summary>
        /// The mrec pos pivot at top left
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetPosition(float x, float y)
        {
            // Convert top-left screen pixel (AppLovin) to bottom-left screen pixel (Unity)
            float screenY = Screen.height - y;

            // Convert screen position to world position
            Vector2 screenPos = new Vector2(x, screenY);

            Vector3 worldPos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(RectTransform, screenPos, null, out worldPos);

            // Apply world position to RectTransform
            RectTransform.position = worldPos;
        }
        /// <summary>
        /// The mrec pos pivot at top left, so get position from topleft with positive
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public (Vector2, Vector2) GetAdsPosition(Canvas parentCanvas)
        {
            // Acquire the world position of the RectTransform's corners and grab the top left
            var corners = new Vector3[4];
            RectTransform.GetWorldCorners(corners);
            var topLeftPosition = corners[1];

            // Use the correct camera based on canvas render mode (null for overlay canvas)
            Camera camera = parentCanvas != null ? parentCanvas.worldCamera : null;

            // Convert the top-left corner from world space to screen space (pixels)
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(camera, topLeftPosition);

            // Convert Unity's pixel coordinates to dp for AppLovin
            (float dpi, float density) = GetDPIAndDensity();

            // AppLovin SDK uses SAFE AREA coordinate system per official docs:
            // - Position (0,0) = top-left of safe area
            // - Bottom-right = (safeAreaWidth, safeAreaHeight)
            // Unity screen coords: origin at bottom-left, Y increases up
            // AppLovin coords: origin at top-left of SAFE AREA, Y increases down

            Rect safeArea = Screen.safeArea;

            // X: offset from left edge of safe area
            float xDp = (screenPos.x - safeArea.x) / density;

            // Y: offset from top edge of safe area
            // safeArea.yMax is the Y coordinate of the TOP of safe area in Unity's bottom-left system
            float yDp = (safeArea.yMax - screenPos.y) / density;

            xDp = Mathf.Max(0, xDp);
            yDp = Mathf.Max(0, yDp);

            // Convert dp back to pixels for dummy ads in the editor
            float unityX = xDp * density;
            float unityY = yDp * density;

            Debug.Log($"MREC position: dp=({xDp},{yDp}) screenPos=({screenPos.x},{screenPos.y}) safeArea=({safeArea.x},{safeArea.y},{safeArea.width},{safeArea.height})");
            return (new Vector2(unityX, unityY), new Vector2(xDp, yDp));
        }

        public void ApplyTransparent(bool transparent = false)
        {
            float alpha = transparent ? 0 : 1;
            CanvasGroup.alpha = alpha;  
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }
        public Rect GetRecLayout() => RectTransform.rect;
    }
}