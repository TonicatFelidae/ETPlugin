using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET.ETPlayerPref
{
    //[Lesser]
    internal static class ET_PlayerPref
    {
        /// <summary>
        /// Try set int key, if success return true
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TrySetInt(string key, int value)
        {
            bool d = PlayerPrefs.HasKey(key);
            if (!d)
            {
                PlayerPrefs.SetInt(key, value);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Try set string key, if success return true
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TrySetString(string key, string value)
        {
            bool d = PlayerPrefs.HasKey(key);
            if (!d)
            {
                PlayerPrefs.SetString(key, value);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Try set float key, if success return true
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TrySetFloat(string key, float value)
        {
            bool d = PlayerPrefs.HasKey(key);
            if (!d)
            {
                PlayerPrefs.SetFloat(key, value);
                return true;
            }
            return false;
        }
    }
}