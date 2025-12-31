using ET.SupportKit.Collection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ET
{
    [Serializable]
    public class IdentifixIntData
    {
        public List<IdentifixIntDataItem> IDData = new();
        private Dictionary<string, IdentifixIntDataItem> _IDDataDict;
        public Dictionary<string, IdentifixIntDataItem> IDDataDict
        {
            get
            {
                if (_IDDataDict == null || _IDDataDict.Count == 0)
                {
                    _IDDataDict = new();
                    for (int i = 0; i < IDData.Count; i++)
                    {
                        _IDDataDict.TryAdd(IDData[i].type, IDData[i]);
                    }
                }
                return _IDDataDict;
            }
        }
        public void AddDataGroup(string type)
        {
            IDData.Add(new(type, 0));
        }
        public void IncreasingValue(string type)
        {
            IDDataDict[type].value += 1;
        }
    }
    [Serializable]
    public class IdentifixIntDataItem
    {
        public string type;
        public int value;

        public IdentifixIntDataItem(string type, int value)
        {
            this.type = type;
            this.value = value;
        }
    }
}

