using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ET.UIKit
{
    public static class ETDropdown
    {
        public static void GenerateEnumDropdown<TEnum>(TMP_Dropdown dropdown) where TEnum : Enum
        {
            if (!typeof(TEnum).IsEnum)
            {
                Debug.LogError("TEnum must be an enum type.");
                return;
            }
            List<string> enumOptions = new List<string>();
            foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
            {
                enumOptions.Add(value.ToString());
            }
            dropdown.AddOptions(enumOptions);
        }
    }
}