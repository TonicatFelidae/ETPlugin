using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ET.SupportKit.UI
{
    public static class ET_UISupportKit
    {
        public static bool IsTouchOverUI(Image uiImage, Touch touch)
        {
            RectTransform uiRect = uiImage.rectTransform;

            Vector2 touchPosition = touch.position;
            Vector2 localTouchPosition;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(uiRect, touchPosition, null, out localTouchPosition))
            {
                return uiRect.rect.Contains(localTouchPosition);
            }

            return false;
        }
    }

}