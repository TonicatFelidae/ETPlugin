using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public struct RectMinMax
    {
        public float minX;
        public float minY;
        public float maxX;
        public float maxY;

        public float Width => maxX - minX;  
        public float Height => maxY - minY; 

        public RectMinMax(float minX, float minY, float maxX, float maxY)
        {
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.maxY = maxY;
        }
    }

    public class EnumRectangle : MonoBehaviour
    {
        public Rect GetRectFromMinMax(float minX, float minY, float maxX, float maxY)
        {
            return new Rect(minX, minY, maxX- minX, maxY- minY);
        }
        public Rect GetRectFromMinMax(RectMinMax rectMinMax)
        {
            return new Rect(rectMinMax.minX, rectMinMax.minY, rectMinMax.Width, rectMinMax.Height);
        }
    }
}

