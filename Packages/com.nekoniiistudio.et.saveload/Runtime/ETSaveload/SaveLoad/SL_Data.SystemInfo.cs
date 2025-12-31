using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ET.Saveload;

public class SL_Data_SystemInfo : MonoBehaviour
{
    public class SAVE_SystemInfo
    {

    }
    [Serializable]
    public class SAVE_Test : SAVE_File
    {
        public QuickData dat;

        public override SAVE_MetaData metaData { get ; set ; }
    }
    [Serializable]
    public struct QuickData
    {
        public string xxx;
        //public Vector3 v0;
        //public Vector2 v1;
        //public int[] v2;
        //public List<int> v3;
        //public Dictionary<string, string> v4;
    }
}
