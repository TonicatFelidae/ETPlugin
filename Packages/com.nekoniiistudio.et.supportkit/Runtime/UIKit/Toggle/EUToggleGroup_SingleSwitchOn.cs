using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ET.UIKit
{
    /// <summary>
    /// EUToggleGroup_SingleSwitchOn work along with ToggleGroup
    /// detect event when single toggle in toggle group on and return the name
    /// </summary>
    [RequireComponent(typeof(ToggleGroup))]
    public class EUToggleGroup_SingleSwitchOn : MonoBehaviour
    {
        ToggleGroup _toggleGroup;
        protected Toggle[] _toggles;
        public UnityEvent<Toggle> onValueChange;

        public int Value
        {
            get
            {
                return GetActiveIndex();
            }
            set
            {
                SetCurrentActiveIndex(value);
            }
        }

        private void Awake()
        {
            _toggleGroup = GetComponent<ToggleGroup>();
            _toggleGroup.allowSwitchOff = false;
            if (_toggleGroup != null)
            {
                _toggles = _toggleGroup.GetComponentsInChildren<Toggle>();
                // Subscribe to the onValueChanged event for each toggle in the ToggleGroup
                foreach (Toggle toggle in _toggles)
                {
                    toggle.onValueChanged.AddListener(OnToggleValueChanged_ForGroupNotice);
                }
            }
        }
        /// <summary>
        /// Get current name of active toggle, base on index from top to down in oject child tree
        /// Return name of object
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetActiveName()
        {
            Toggle toggle = GetActiveToggle();
            if (toggle == null) { return null; }
            else return toggle.transform.name;
        }
        /// <summary>
        /// Get current name of active toggle, base on index from top to down in oject child tree
        /// Return index of object in child list
        /// </summary>
        /// <returns></returns>
        public int GetActiveIndex()
        {
            Toggle toggle = GetActiveToggle();
            if (toggle == null) { return -1; }
            else return toggle.transform.GetSiblingIndex();
        }
        /// <summary>S
        /// Get current name of active toggle, base on index from top to down in oject child tree
        /// Return toggle
        /// </summary>
        /// <returns></returns>
        public Toggle GetActiveToggle()
        {
            try
            {

                return _toggleGroup.GetFirstActiveToggle();
            }
            catch (System.Exception e)
            {
                this.LogError("Criticle error, toggle out of array: " + e.Message);
                return null;
            }
        }

        public void SetCurrentActiveIndex(int index)
        {
            try
            {
                if (_toggleGroup != null)
                {
                    _toggles = _toggleGroup.GetComponentsInChildren<Toggle>();
                    // Subscribe to the onValueChanged event for each toggle in the ToggleGroup
                    for (int i = 0; i < _toggles.Length; i++)
                    {
                        Toggle curToggle = _toggles[i];
                        if ( i == index)
                        {
                            curToggle.isOn = true;
                        }
                    } 
                }
            }
            catch (Exception e)
            {
                this.LogError("Criticle error, cant set idex, may because index out of range: " + e.Message);
            }
        }
        public void SetCurrentActiveIndexWithoutNotify(int index)
        {
            try
            {
                if (_toggleGroup != null)
                {
                    _toggles = _toggleGroup.GetComponentsInChildren<Toggle>();
                    // Subscribe to the onValueChanged event for each toggle in the ToggleGroup
                    for (int i = 0; i < _toggles.Length; i++)
                    {
                        Toggle curToggle = _toggles[i];
                        if (i == index)
                        {
                            curToggle.SetIsOnWithoutNotify(true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.LogError("Criticle error, cant set idex, may because index out of range: "+e.Message);
            }
        }
        private void OnToggleValueChanged_ForGroupNotice(bool isOn)
        {
            if (isOn)
            {
                onValueChange.Invoke(GetActiveToggle());
            }
        }
    }
}