using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

namespace ET.UIKit.ZenjectUIScreen
{
    /// <summary>
    /// Show noff
    /// Show cost in middle with icon and cost
    /// Show no yes but
    /// </summary>
    public class PUUsingMoney : PopupUIScreen
    {
        public TextMeshProUGUI noffText;
        public TextMeshProUGUI costText;
        public GameObject[] itemIcons;
        public Button[] buttons;
        public TextMeshProUGUI[] butTexts;
        public Color[] costTestColors;
        public RectTransform costBox; // to force up date transform
        /// <summary>
        /// Noff
        /// Cost string
        /// ItemIconID string int paste able
        /// 0Action
        /// 1Action
        /// 0Text
        /// 1Text
        /// </summary>
        /// <param name="paras"></param>
        public override void Load(params object[] paras)
        {
            noffText.text = paras[0].ToString();
            costText.text = paras[1].ToString();
            costText.color = costTestColors[int.Parse(paras[2].ToString())];
            OnlyShowIconIndex(int.Parse(paras[2].ToString()));
            for (int i = 0; i < buttons.Length; i++)
            {
                int index = i;
                buttons[index].interactable = true;
                buttons[index].onClick.RemoveAllListeners();
                buttons[index].onClick.AddListener((UnityAction)paras[index + 3]);
                buttons[index].onClick.AddListener(() =>
                {

                    buttons[index].interactable = false;
                    _popupUI.ClosePopupUI();
                });
            }
            for (int i = 0; i < butTexts.Length; i++)
            {
                int index = i;
                try
                {
                    butTexts[index].text = paras[index + 3 + buttons.Length].ToString();
                }
                catch { }
            }
            base.Load(paras);
            costBox.ForceUpdateRectTransforms();
            StartCoroutine(LateUpdateRect());
            LayoutRebuilder.ForceRebuildLayoutImmediate(costBox);
        }
        void OnlyShowIconIndex(int index)
        {
            for (int i = 0; i < itemIcons.Length; i++)
            {
                itemIcons[i].SetActive(index == i);
            }
        }
        IEnumerator LateUpdateRect()
        {
            yield return new WaitForSeconds(0.1f);
            costBox.ForceUpdateRectTransforms();
        }
    }
}

