using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET.UIKit.EUExpandableSystem
{
    /// <summary>
    /// DO NOT SET THIS AS SCRIPTABVLE OBJECT
    ///
    /// </summary>
    [Serializable]
    public class EUE_stringListData
    {
        public List<string> values = new();
        public EUExpandableListItemUI bindUI { get; set; }
        public List<EUE_stringListData> items = new();


        public string Value => values[0];
        public EUE_stringListData(string value)
        {
            values = new() { value };
            items = new();
        }
        public EUE_stringListData(string value, string itemValue0)
        {
            values = new() { value};
            items = new() { new(itemValue0) };
        }
        public EUE_stringListData(string value, string itemValue0, string itemValue1)
        {
            values = new() { value };
            items = new() { new(itemValue0), new(itemValue1) };
        }
        public EUE_stringListData(string value, string itemValue0, string itemValue1, string itemValue2)
        {
            values = new() { value };
            items = new() { new(itemValue0), new(itemValue1), new(itemValue2) };
        }
        public EUE_stringListData(string value, string itemValue0, string itemValue1, string itemValue2, string itemValue3)
        {
            values = new() { value };
            items = new() { new(itemValue0), new(itemValue1), new(itemValue2), new(itemValue3) };
        }
    }
}
