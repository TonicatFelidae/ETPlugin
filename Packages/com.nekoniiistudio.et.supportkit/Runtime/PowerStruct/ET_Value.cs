using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ET.PowerStruct
{
    [Serializable]
    public struct ET_Value
    {
        public object Value => ValueType switch
        {
            ETValueType.Int => (int)numberValue,
            ETValueType.Float => (float)numberValue,
            ETValueType.String => (string)stringValue,
            _ => (string)stringValue
        };
        [SerializeField] private ETValueType ValueType;
        [SerializeField] private float numberValue;
        [SerializeField] private string stringValue;
    }
    public enum ETValueType
    {
        Int,
        Float,
        String,
    }
}

