using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ET.SupportKit;
using ET.SupportKit.EMath;

namespace ET.PowerStruct
{
    [Serializable]
    public class ET_IDIntValue<T>
    {
        public T ID;
        public int value;

        public ET_IDIntValue(T iD, int value)
        {
            ID = iD;
            this.value = value;
        }
    }
}
