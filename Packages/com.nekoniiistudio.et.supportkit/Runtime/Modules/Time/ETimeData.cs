using System;

namespace ET
{
    [Serializable]
    public class ETimeData
    {
        public float baseSpeed = 1;
        public TimeFrame[] timeFrames;
        public int CountFrame => timeFrames.Length;
    }
    public class ETimeExtendData
    {
        public float[] framePercentages;
        public void Init(TimeFrame[] timeFrames)
        {
            framePercentages = new float[timeFrames.Length];
        }
    }
}