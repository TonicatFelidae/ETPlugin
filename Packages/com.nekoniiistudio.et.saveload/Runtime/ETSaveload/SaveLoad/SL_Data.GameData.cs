using ET.Saveload;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// SAVE_GameFile
[Serializable]
public class SAVE_GameData : SAVE_File
{
    //public W w;
    //public SAVE_GameData(WorldInfo w) //:D
    //{
    //    //this.w = w;
    //}
    public override SAVE_MetaData metaData { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}
//[Serializable]
//public class WorldInfo
//{
//    //public Dictionary<string, PlayerGeneral> playerCrews = new();
//    //public Dictionary<string, BuildingData> buildings = new();
//    //public MapData mapData;
//}
