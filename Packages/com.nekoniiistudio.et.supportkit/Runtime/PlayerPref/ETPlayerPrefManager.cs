using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace ET.ETPlayerPref
{
    /// <summary>
    /// ETPlayerPrefManager use to manager simple playerPref value, for more advance, should consider other system
    /// </summary>
    [Serializable]
    public class ETPlayerPrefManager : MonoBehaviour
    {
        public List<PlayerPrefInt> intKeys;
        public List<PlayerPrefFloat> floatKeys;
        public List<PlayerPrefString> stringKeys;
        [Header("Options")]
        public bool runInitAtAwake = true;
        private void Awake()
        {
            DontDestroyOnLoad(this);
            if (runInitAtAwake) Init();
        }
        public int Count
        {
            get
            {
                int ret = 0;
                ret += intKeys != null ? intKeys.Count : 0;
                ret += floatKeys != null ? floatKeys.Count : 0;
                ret += stringKeys != null ? stringKeys.Count : 0;
                return ret;
            }
        }
        /// <summary>
        /// Init all key
        /// </summary>
        public void Init()
        {
            int count = 0;
            foreach (PlayerPrefInt item in intKeys)
            {
                count += ET_PlayerPref.TrySetInt(item.key, item.value) ? 1 : 0;
            }
            foreach (PlayerPrefFloat item in floatKeys)
            {
                count += ET_PlayerPref.TrySetFloat(item.key, item.value) ? 1 : 0;
            }
            foreach (PlayerPrefString item in stringKeys)
            {
                count += ET_PlayerPref.TrySetString(item.key, item.value) ? 1 : 0;
            }
            Debug.Log($"[ETPlayerPrefManager] Initiation completed, change {count}/{Count} keys");
        }
        public void DeleteAllKeys()
        {
            PlayerPrefs.DeleteAll();
            intKeys = new();
            floatKeys = new();
            stringKeys = new();
            Debug.Log($"[ETPlayerPrefManager] Deleted All Keys");
        }
        public void ResetToDefaultValue()
        {
            foreach (PlayerPrefInt item in intKeys)
            {
                PlayerPrefs.SetInt(item.key, item.value);
            }
            foreach (PlayerPrefFloat item in floatKeys)
            {
                PlayerPrefs.SetFloat(item.key, item.value);
            }
            foreach (PlayerPrefString item in stringKeys)
            {
                PlayerPrefs.SetString(item.key, item.value);
            }
            Debug.Log($"[ETPlayerPrefManager] Reseted All Keys To Default, change {Count} keys");
        }
    }
    [Serializable]
    public struct PlayerPrefInt
    {
        public string key;
        public int value;

        public PlayerPrefInt(string key, int value)
        {
            this.key = key;
            this.value = value;
        }
    }
    [Serializable]
    public struct PlayerPrefString
    {
        public string key;
        public string value;

        public PlayerPrefString(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }
    [Serializable]
    public struct PlayerPrefFloat
    {
        public string key;
        public float value;

        public PlayerPrefFloat(string key, float value)
        {
            this.key = key;
            this.value = value;
        }
    }
}