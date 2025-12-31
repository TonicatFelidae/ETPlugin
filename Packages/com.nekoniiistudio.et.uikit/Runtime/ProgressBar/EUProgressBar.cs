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
        //[Header("Options")]
        //public SecondaryInfomationType loadingTextInfomationType;
        public TextMeshProUGUI loadingText;
        public void UpdateValue(float percent, string messeger)
        {
            progressBar.fillAmount = percent;
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
    }
}
