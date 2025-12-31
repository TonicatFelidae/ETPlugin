using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace ET.SupportKit.SaveLoad
{

    /* ET SupportKit Save load
     * Role : provide type convention to make it Serializable
     * 
     * 
     * 
     */
    public static class ET_SaveLoad_SP
    {
        public static Vector2S ToVector2S(this UnityEngine.Vector2 vector2) => new Vector2S(vector2.x, vector2.y);
        public static UnityEngine.Vector2 ToVector2(this Vector2S vector2s) => new UnityEngine.Vector2(vector2s.x, vector2s.y);
        public static Vector3S ToVector3S(this UnityEngine.Vector3 vector3) => new Vector3S(vector3.x, vector3.y, vector3.z);
        public static UnityEngine.Vector3 ToVector3(this Vector3S vector3s) => new UnityEngine.Vector3(vector3s.x, vector3s.y, vector3s.z);
        public static ColorS ToColorS(this Color color) => new ColorS(color.r,color.b, color.g,color.a);
        public static Color ToColor(this ColorS colorS) => new Color(colorS.r, colorS.b, colorS.g, colorS.a);
    }
    [Serializable]
    public struct Vector2S
    {
        public float x;
        public float y;

        public Vector2S(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }
    [Serializable]
    public struct Vector3S
    {
        public float x;
        public float y;
        public float z;

        public Vector3S(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    [Serializable]
    public struct ColorS
    {
        public float r;
        public float b;
        public float g;
        public float a;

        public ColorS(float r, float b, float g, float a)
        {
            this.r = r;
            this.b = b;
            this.g = g;
            this.a = a;
        }
    }
}
