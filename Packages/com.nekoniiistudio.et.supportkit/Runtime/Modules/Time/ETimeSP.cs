using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ET
{
    public static class ETimeSP
    {
        public static ETimeData GenerateSecTickTF(this ETimeData timeSetting)
        {
            if (timeSetting == null) timeSetting = new(); 
            timeSetting.timeFrames = new TimeFrame[2];
            timeSetting.timeFrames[0] = GenerateSecond();
            timeSetting.timeFrames[1] = GenerateTick();
            timeSetting.baseSpeed = 60;
            return timeSetting;
        }
        public static ETimeData GenerateMinuteSecTickTF(this ETimeData timeSetting)
        {
            if (timeSetting == null) timeSetting = new();
            timeSetting.timeFrames = new TimeFrame[3];
            timeSetting.timeFrames[0] = GenerateMinute();
            timeSetting.timeFrames[1] = GenerateSecond();
            timeSetting.timeFrames[2] = GenerateTick();
            timeSetting.baseSpeed = 60;
            return timeSetting;
        }
        public static ETimeData GenerateHourMinuteSecTickTF(this ETimeData timeSetting)
        {
            if (timeSetting == null) timeSetting = new();
            timeSetting.timeFrames = new TimeFrame[4];
            timeSetting.timeFrames[0] = GenerateHour();
            timeSetting.timeFrames[1] = GenerateMinute();
            timeSetting.timeFrames[2] = GenerateSecond();
            timeSetting.timeFrames[3] = GenerateTick();
            timeSetting.baseSpeed = 60;
            return timeSetting;
        }
        public static ETimeData GenerateMinuteSecTF(this ETimeData timeSetting)
        {
            if (timeSetting == null) timeSetting = new();
            timeSetting.timeFrames = new TimeFrame[2];
            timeSetting.timeFrames[0] = GenerateMinute();
            timeSetting.timeFrames[1] = GenerateSecond().SetSpanToOne();
            return timeSetting;
        }
        public static ETimeData GenerateHourMinuteTF(this ETimeData timeSetting)
        {
            if (timeSetting == null) timeSetting = new();
            timeSetting.timeFrames = new TimeFrame[2];
            timeSetting.timeFrames[0] = GenerateHour();
            timeSetting.timeFrames[1] = GenerateMinute();
            return timeSetting;
        }
        public static ETimeData GenerateHourMinuteSecTF(this ETimeData timeSetting)
        {
            if (timeSetting == null) timeSetting = new();
            timeSetting.timeFrames = new TimeFrame[3];
            timeSetting.timeFrames[0] = GenerateHour();
            timeSetting.timeFrames[1] = GenerateMinute();
            timeSetting.timeFrames[2] = GenerateSecond().SetSpanToOne();
            return timeSetting;
        }
        public static TimeFrame GenerateHour()
        {
            TimeFrame ret = new TimeFrame
            {
                namex = "Hour",
                span = 60,
                prefix = ":",
                present = 2,
            };
            return ret;
        }
        public static TimeFrame GenerateMinute()
        {
            TimeFrame ret = new TimeFrame
            {
                namex = "Minute",
                span = 60,
                prefix = ":",
                present = 2,
            };
            return ret;
        }
        public static TimeFrame GenerateSecond()
        {
            TimeFrame ret = new TimeFrame
            {
                namex = "Second",
                span = 60,
                prefix = ":",
                present = 2,
            };
            return ret;
        }
        public static TimeFrame GenerateTick()
        {
            TimeFrame ret = new TimeFrame
            {
                namex = "Tick",
                span = 1,
                prefix = ":",
                present = 2,
            };
            return ret;
        }
        public static TimeFrame SetSpanToOne(this TimeFrame timeFrame)
        {
            timeFrame.span = 1;
            return timeFrame;
        }
    }
}


