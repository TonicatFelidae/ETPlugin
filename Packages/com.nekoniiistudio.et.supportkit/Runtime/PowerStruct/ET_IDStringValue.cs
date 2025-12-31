using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ET.SupportKit;
using ET.SupportKit.EMath;

namespace ET.PowerStruct
{
    [Serializable]
    public class ET_IDStringValue<T>
    {
        public T ID;
        public string value;

        public ET_IDStringValue(T iD, string value)
        {
            ID = iD;
            this.value = value;
        }
    }
}
