using ET.SupportKit.Collection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ET.UIKit
{
    public class ETLayoutRebuilder : MonoBehaviour
    {
        [SerializeField] RectTransform[] _rectTransforms;
        public void RebuildLayoutImmediate()
        {
            for (int i = 0; i < _rectTransforms.Length; i++)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransforms[i]);
            }
        }
        public void AddRectTransform(RectTransform rectTransform)
        {
            _rectTransforms = _rectTransforms.Append(rectTransform);
        }
    }
}