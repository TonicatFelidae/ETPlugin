using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace ET.UIKit
{
    /// <summary>
    /// Powerful ET adapter for player pref
    /// </summary>
    public class EUPlayerPrefs : MonoBehaviour
    {
        public string key;
        public UnityEvent<string, object> onValueChange = new();

        void Start()
        {
            if (GetComponent<Button>()) GetComponent<Button>().onClick.AddListener(OnToggleBut);
            if (GetComponent<Toggle>()) GetComponent<Toggle>().onValueChanged.AddListener(OnBoolChange);
            if (GetComponent<Slider>()) GetComponent<Slider>().onValueChanged.AddListener(OnFLoatChange);
            if (GetComponent<TMP_Dropdown>()) GetComponent<TMP_Dropdown>().onValueChanged.AddListener(OnIntChange);
            if (GetComponent<TMP_InputField>()) GetComponent<TMP_InputField>().onValueChanged.AddListener(OnStringChange);
        }
        void OnEnable()
        {
            if (GetComponent<Toggle>()) GetComponent<Toggle>().SetIsOnWithoutNotify(PlayerPrefs.GetInt(key) == 1 ? true : false);
            if (GetComponent<Slider>()) GetComponent<Slider>().SetValueWithoutNotify(PlayerPrefs.GetFloat(key));
            if (GetComponent<TMP_Dropdown>()) GetComponent<TMP_Dropdown>().SetValueWithoutNotify(PlayerPrefs.GetInt(key));
            if (GetComponent<TMP_InputField>()) GetComponent<TMP_InputField>().SetTextWithoutNotify(PlayerPrefs.GetString(key));
        }
        void OnToggleBut()
        {
            int value = 0;
            if (PlayerPrefs.GetInt(key) == 1) value = 0;
            else value = 1;
            PlayerPrefs.SetInt(key, value);
            onValueChange?.Invoke(key, value);
        }
        void OnBoolChange(bool val)
        {
            int value = 0;
            if (val == false) value = 0;
            else value = 1;
            PlayerPrefs.SetInt(key, value);
            onValueChange?.Invoke(key, value);
        }
        void OnFLoatChange(float val)
        {
            PlayerPrefs.SetFloat(key, val);
            onValueChange?.Invoke(key, val);
        }

        void OnIntChange(int val)
        {
            PlayerPrefs.SetInt(key, val);
            onValueChange?.Invoke(key, val);
        }
        void OnStringChange(string val)
        {
            PlayerPrefs.SetString(key, val);
            onValueChange?.Invoke(key, val);
        }
    }

}