using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Engine
{
    public static class ETag
    {
        public static string GetTag(ETagEnum enumValue)
        {
            return Enum.GetName(typeof(ETagEnum), enumValue);
        }
    }
    public enum ETagEnum
    {
        Player,
        Enemy,
        PlayerBuilding,
        PlayerObject,
        EnemyBuilding,
        EnemyObject,
    }

}
