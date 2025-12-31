using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ET.UIKit;
using JetBrains.Annotations;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.UI;

public class EUToggleGroup_SingleSwitchOn_Demo : MonoBehaviour
{
    public EUToggleGroup_SingleSwitchOn singleSwitchOn;
    // Start is called before the first frame update
    void Start()
    {
        singleSwitchOn.onValueChange.AddListener(SingleSwitchOn);
        singleSwitchOn.SetCurrentActiveIndexWithoutNotify (2);
    }
    public void SingleSwitchOn(UnityEngine.UI.Toggle toggle)
    {

        Debug.Log(toggle.name);
        Debug.Log(toggle.transform.GetSiblingIndex());
        Debug.Log(singleSwitchOn.GetActiveName());
        Debug.Log(singleSwitchOn.GetActiveIndex());
        Debug.Log(singleSwitchOn.GetActiveToggle().name);
        Debug.Log(singleSwitchOn.GetActiveToggle().transform.GetSiblingIndex());
    }
}
