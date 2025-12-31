using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;

namespace ET.Extension
{
    [Serializable]
    public class RandomIndex
    {
        /// <summary>
        /// Exampple 
        /// 15, => 75%
        /// 3, => 15%
        /// 2, => 10%
        /// </summary>
        public List<float> indexRandomData = new();
        [HideInInspector] public List<float> indexRandomValue = new();
        [Tooltip("Using to sellect index from group, for example: origin have in dex 0,1,2,3,4,5,; now you only want to get random from 4,5 you should create 4,5 in this index group")]
        public List<IndexGroup> indexGroup = new List<IndexGroup>();  
        public void Init()
        {
            float t = 0;
            if (indexRandomData.Count ==0 )
            {
                Debug.LogError("No data exist");
                return;
            }
            foreach (var item in indexRandomData) 
            {
                t += item;
            }
            if (t <= 0)
            {
                Debug.LogError("Data all zero");
                return;
            }
            float v = 0;
            for (var i = 0; i < indexRandomData.Count; i++)
            {
                indexRandomValue.Add((float)indexRandomData[i] / (float)t + v);
                v = indexRandomValue[i];
            }
        }
        public int GetRandomIndex()
        {
            if (indexRandomValue == null || indexRandomValue.Count == 0)
            {
                Init();
            }
            float value = UnityEngine.Random.Range(0f, 1f);
            int ret = 0;
            for (var i = 0;i < indexRandomValue.Count;i++)
            {
                if (value <= indexRandomValue[i])
                {
                    ret = i;
                    break;
                }
            }
            return ret;
        }
        [Serializable]
        public struct IndexGroup
        {
            public List<int> indexs; 
            [HideInInspector] public List<float> indexRandomValue;
        }

    }
}
