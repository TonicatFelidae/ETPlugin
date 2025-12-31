using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Shooterkit
{
    public enum ProjectileType
    {
        RoundShot
    }
    public enum FireMode
    {
        SingleShot,
        ThreeShot,
        Automatic,
    }
    public enum ControlType
    {
        FullAutomatic, // hold button and shot
        Click, // click to shot
    }
}
