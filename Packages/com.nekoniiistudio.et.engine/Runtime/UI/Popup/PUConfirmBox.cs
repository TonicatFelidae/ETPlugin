using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

namespace ET.UIKit.ZenjectUIScreen
{
    public class PUConfirmBox : PopupUIScreen
    {
        public TextMeshProUGUI noffText;
        public Button[] buttons;
        /// <summary>
        /// Noff
        /// Confirm 
        /// Cancle 
        /// </summary>
        /// <param name="paras"></param>
        public override void Load(params object[] paras)
        {
            noffText.text = paras[0].ToString();
            UnityAction act0 = (UnityAction)paras[1];
            for (int i = 0; i < buttons.Length; i++)
            {
                int index = i;
                buttons[index].interactable = true;
                buttons[index].onClick.RemoveAllListeners();
                buttons[index].onClick.AddListener((UnityAction)paras[index + 1]);
                buttons[index].onClick.AddListener(() =>
                {

                    buttons[index].interactable = false;
                    _popupUI.ClosePopupUI();
                });
            }
            base.Load(paras);
        }
    }
}

