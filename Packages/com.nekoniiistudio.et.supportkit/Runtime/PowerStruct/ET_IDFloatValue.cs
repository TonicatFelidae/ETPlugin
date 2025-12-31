using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ET.SupportKit;
using ET.SupportKit.EMath;

namespace ET.PowerStruct
{
    [Serializable]
    public class ET_IDFloatValue<T>
    {
        public T ID;
        public float value;

        public ET_IDFloatValue(T iD, float value)
        {
            ID = iD;
            this.value = value;
        }
    }
}
