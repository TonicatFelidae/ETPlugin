using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ET.UIKit.ZenjectUIScreen
{
    public class PUImageConfirmBox : PopupUIScreen
    {
        public UnityAction onConfirm;
        public UnityAction onCancel;
        public Button[] buttons;
        public TextMeshProUGUI noffText;
        public Image image;
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
                buttons[i].onClick.RemoveAllListeners();
                buttons[i].onClick.AddListener((UnityAction)paras[i + 2]);
            }
            image.sprite = (Sprite)paras[1];
            base.Load(paras);
        }
    }

}
