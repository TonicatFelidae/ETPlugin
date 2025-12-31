using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ET.UIKit
{
    public class EUButton : MonoBehaviour
    {
        public Button button;
        public void SetInteractable(bool interactable)
        {
            button.interactable = interactable;
            OnButtonInteractableChange(button.interactable);
        }
        public virtual void OnButtonInteractableChange(bool interactable)
        {

        }
    }
}