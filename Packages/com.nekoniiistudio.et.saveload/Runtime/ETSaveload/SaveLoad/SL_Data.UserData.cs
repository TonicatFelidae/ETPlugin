using ET.Saveload;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// SAVE_UserFile
[Serializable]
public class SAVE_UserData : SAVE_File
{
    public override SAVE_MetaData metaData { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}
