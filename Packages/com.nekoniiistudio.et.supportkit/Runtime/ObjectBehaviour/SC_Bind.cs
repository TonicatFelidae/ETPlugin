using System;
using UnityEngine;

public class SC_Bind: MonoBehaviour
{
    public ET_objeffect_bind bindSetting;
    public void Update()
    {
        if (bindSetting.on && bindSetting.go)
        {
            transform.position = bindSetting.go.position;
        }
    }
    [Serializable]
    public struct ET_objeffect_bind
    {
        public bool on;
        public Transform go;
    }
}
