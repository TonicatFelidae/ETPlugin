using ET;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace ET
{
    public class IdentifixInt : Singleton<IdentifixInt>
    {
        static IdentifixIntData _identifixIntData;
        public static void Init(IdentifixIntData identifixIntData, string[] types)
        {
            _identifixIntData = identifixIntData;
            for (int i = 0; i < types.Length; i++)
            {
                _identifixIntData.AddDataGroup(types[i]);  
            }
        }
        public static int GetID(string type)
        {
            int ret = _identifixIntData.IDDataDict[type].value;
            _identifixIntData.IncreasingValue(type);
            return ret;
        }
        public static void RecreateData(IdentifixIntData identifixIntData, string[] types)
        {
            identifixIntData.IDData = new();
            Init(identifixIntData, types);
        }
    }

}