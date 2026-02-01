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

        [Header("Progress Bar Start Value")]
        [SerializeField] private float startValue = 0f; // The value at which 0% begins

        public void Init(ProgressBarType progressBarType = ProgressBarType.Fill, float startValue = 0f)
        {
            _progressBarType = progressBarType;
            this.startValue = startValue;
        }

        /// <summary>
        /// Updates the progress bar value. The percent parameter is mapped so that 0% is at startValue.
        /// </summary>
        /// <param name="percent">Progress percentage (0-1), where 0 is startValue.</param>
        /// <param name="messeger">Optional message to display.</param>
        public void UpdateValue(float percent, string messeger)
        {
            // Clamp percent between 0 and 1
            percent = Mathf.Clamp01(percent);

            // Map percent so that 0% is at startValue, 100% is at 1
            float mappedPercent = Mathf.Lerp(startValue, 1f, percent);

            switch (_progressBarType)
            {
                case ProgressBarType.Fill:
                    progressBar.fillAmount = mappedPercent;
                    break;
                case ProgressBarType.RectHorizontal:
                    var originalWidth = _originalRect.sizeDelta.x;
                    progressBar.rectTransform.sizeDelta = new Vector2(originalWidth * mappedPercent, progressBar.rectTransform.sizeDelta.y);
                    break;
                case ProgressBarType.RectVertical:
                    var originalHeight = _originalRect.sizeDelta.y;
                    progressBar.rectTransform.sizeDelta = new Vector2(progressBar.rectTransform.sizeDelta.x, originalHeight * mappedPercent);
                    break;
                case ProgressBarType.Scale:
                    progressBar.transform.localScale = new Vector3(mappedPercent, mappedPercent, 1f);
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
