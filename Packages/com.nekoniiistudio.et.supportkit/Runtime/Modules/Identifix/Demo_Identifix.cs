using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ET.Engine;

namespace ET.Demo
{
    public class Demo_Identifix : MonoBehaviour, IIDItem
    {
        private string _id;
        public string ID
        {
            get
            {
                if (string.IsNullOrEmpty(_id)) _id = Identifix.GetID(this);
                return _id;    
            }
        }
    }
}

