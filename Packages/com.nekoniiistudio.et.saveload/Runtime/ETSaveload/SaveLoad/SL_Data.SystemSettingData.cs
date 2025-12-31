using ET.Saveload;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// SAVE_SystemSettig
[Serializable]
public class SAVE_SystemSettingData : SAVE_File
{
    public SoundData soundData;

    public override SAVE_MetaData metaData { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override void ResetAll()
    {
        ResetSoundData();
    }
    //sound
    public void ResetSoundData()
    {
        soundData = new SoundData();
        soundData.BMVol = 70;
        soundData.EMVol = 70;
        soundData.UMVol = 70;
        soundData.MAVol = 100;
    }
    [Serializable]
    public struct SoundData
    {
        public int BMVol; //(0,100)
        public int EMVol; //(0,100)
        public int UMVol; //(0,100)
        public int MAVol; //(0,100)
    }
}
