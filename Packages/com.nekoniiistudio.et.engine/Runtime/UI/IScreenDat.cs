using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET.UIKit.ZenjectUIScreen
{
    public interface IScreenDat
    {
        Dictionary<string, UIScreen> ScreenDict { get; set; }
    }
}
