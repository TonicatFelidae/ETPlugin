using UnityEngine;
using UnityEngine.UI;
namespace ET.UIKit
{
    public enum ImageFitMode
    {
        None,        // No override – leave image as-is
        Stretch,     // Fill the container, ignoring aspect ratio
        FitCenter,   // Contain: scale to fit inside, preserve aspect (letterbox/pillarbox)
        CropCenter,  // Cover: scale to fill, crop centered
        CropTop,     // Cover: scale to fill, crop aligned to top edge
        CropBottom,  // Cover: scale to fill, crop aligned to bottom edge
        CropLeft,    // Cover: scale to fill, crop aligned to left edge
        CropRight    // Cover: scale to fill, crop aligned to right edge
    }

    [RequireComponent(typeof(Image))]
    public class SC_ImageFixer : MonoBehaviour
    {
        [SerializeField] private ImageFitMode fitMode = ImageFitMode.CropCenter;

        private Image _image;
        private Sprite _lastSprite;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void Start()
        {
            ApplyFitMode();
            _lastSprite = _image != null ? _image.sprite : null;
        }

        private void LateUpdate()
        {
            if (_image == null) return;
            Sprite current = _image.sprite;
            if (current != _lastSprite)
            {
                _lastSprite = current;
                ApplyFitMode();
            }
        }

        private void ApplyFitMode()
        {
            if (_image == null)
                _image = GetComponent<Image>();

            if (_image == null) return;

            if (fitMode == ImageFitMode.None)
            {
                _image.overrideSprite = null;
                return;
            }

            if (fitMode == ImageFitMode.Stretch)
            {
                _image.overrideSprite = null;
                _image.preserveAspect = false;
                return;
            }

            if (fitMode == ImageFitMode.FitCenter)
            {
                _image.overrideSprite = null;
                _image.preserveAspect = true;
                return;
            }

            // --- Crop modes (Cover) ---
            if (_image.sprite == null) return;

            RectTransform rt = GetComponent<RectTransform>();
            float containerW = rt.rect.width;
            float containerH = rt.rect.height;

            if (containerW <= 0 || containerH <= 0) return;

            Sprite original = _image.sprite;
            Rect spriteRect = original.rect; // pixel rect in texture (handles atlas sprites)

            float spriteW = spriteRect.width;
            float spriteH = spriteRect.height;

            if (spriteW <= 0 || spriteH <= 0) return;

            // Cover: scale so both dimensions are >= container, preserve aspect ratio
            float scale = Mathf.Max(containerW / spriteW, containerH / spriteH);

            float cropW = containerW / scale;
            float cropH = containerH / scale;

            float cropX, cropY;
            switch (fitMode)
            {
                case ImageFitMode.CropTop:
                    cropX = spriteRect.x + (spriteW - cropW) * 0.5f;
                    cropY = spriteRect.y + (spriteH - cropH); // aligned to top (max Y)
                    break;
                case ImageFitMode.CropBottom:
                    cropX = spriteRect.x + (spriteW - cropW) * 0.5f;
                    cropY = spriteRect.y;                     // aligned to bottom (min Y)
                    break;
                case ImageFitMode.CropLeft:
                    cropX = spriteRect.x;                     // aligned to left (min X)
                    cropY = spriteRect.y + (spriteH - cropH) * 0.5f;
                    break;
                case ImageFitMode.CropRight:
                    cropX = spriteRect.x + (spriteW - cropW); // aligned to right (max X)
                    cropY = spriteRect.y + (spriteH - cropH) * 0.5f;
                    break;
                default: // CropCenter
                    cropX = spriteRect.x + (spriteW - cropW) * 0.5f;
                    cropY = spriteRect.y + (spriteH - cropH) * 0.5f;
                    break;
            }

            _image.overrideSprite = Sprite.Create(
                original.texture,
                new Rect(cropX, cropY, cropW, cropH),
                new Vector2(0.5f, 0.5f),
                original.pixelsPerUnit
            );
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_image == null)
                _image = GetComponent<Image>();

            if (_image == null) return;

            if (fitMode == ImageFitMode.None)
            {
                _image.overrideSprite = null;
                return;
            }

            ApplyFitMode();
        }
#endif
    }
}