using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ET.Extension
{
    public class RequireInterfaceAttribute : PropertyAttribute
    {
        public System.Type requiredType { get; set; }
        public RequireInterfaceAttribute(System.Type type)
        {
            this.requiredType = type;
        }
    }

}