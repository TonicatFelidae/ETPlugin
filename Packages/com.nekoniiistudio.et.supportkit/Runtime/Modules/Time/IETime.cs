using System;
using UnityEngine;

namespace ET
{
    public interface IETime
    {
        public void Flow();
    }


    [Serializable]
    public struct TimeFrame // separate // from bigger span to smaller span
    {
        public string namex; // name that dispay
        public string prefix; // suffix / : :: / #
        [Range(1, 8)]
        public int present; // 1, 00, 001, 03, 004
        public int span; // span of time
                         //option
        public bool useEcho;
        [Range(20, 100)]
        public int echo;
    }
}