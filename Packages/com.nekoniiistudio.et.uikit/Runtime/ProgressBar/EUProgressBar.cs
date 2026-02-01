using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace ET.UIKit
{
    public class EUProgressBar : MonoBehaviour
    {
        public Image progressBar;
        public TextMeshProUGUI loadingText;
        [Header("Option")]
        [SerializeField] private RectTransform _originalRect;
        [SerializeField] private ProgressBarType _progressBarType = ProgressBarType.Fill;
        public void Init(ProgressBarType progressBarType = ProgressBarType.Fill)
        {
            _progressBarType = progressBarType;
        }
        public void UpdateValue(float percent, string messeger)
        {
            switch (_progressBarType)
            {
                case ProgressBarType.Fill:
                    progressBar.fillAmount = percent;
                    break;
                case ProgressBarType.RectHorizontal:
                    var originalWidth = _originalRect.sizeDelta.x;
                    progressBar.rectTransform.sizeDelta = new Vector2(originalWidth * percent, progressBar.rectTransform.sizeDelta.y);
                    break;
                case ProgressBarType.RectVertical:
                    var originalHeight = _originalRect.sizeDelta.y;
                    progressBar.rectTransform.sizeDelta = new Vector2(progressBar.rectTransform.sizeDelta.x, originalHeight * percent);
                    break;
                case ProgressBarType.Scale:
                    progressBar.transform.localScale = new Vector3(percent, percent, 1f);
                    break;
                default:
                    break;
            }
            if (messeger != null) loadingText.text = messeger;
        }
        public void UpdateText(string messeger)
        {
            if (messeger != null) loadingText.text = messeger;
        }
        public enum SecondaryInfomationType
        {
            None,
            Percent,
            Progress,
            Loading,
        }
        public enum ProgressBarType
        {
            Fill,
            RectHorizontal,
            RectVertical,
            Scale,
        }
    }
}
