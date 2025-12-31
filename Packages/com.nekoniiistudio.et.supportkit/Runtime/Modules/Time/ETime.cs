using ET.SupportKit;
using ET.SupportKit.EMath;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ET
{
    /// <summary>
    /// Type A class, ETime
    /// 
    /// Current issue: 
    /// - Coutner not manager by ditionary but int number
    /// </summary>
    public class ETime : IETime
    {
        public bool ticka;
        public bool tick; // tick sec that affect by speed // MAIN count
        public bool tickh; // should use different count method
        public bool tickq; // should use different count method // will need
        public bool tick2;
        public bool tick5;
        public bool tick10;
        public bool tick60;
        public bool tick120;
        public int seca;
        public int sec;
        //
        private float _timeSpeed = 1;
        private float _timeScale = 1;
        /// <summary>
        /// local scale // best use when time follwo faster but all other action is normal 
        /// not so performace friendly
        /// </summary>
        public float TimeSpeed => _timeSpeed * _timeScale; 
        public ETimeData timeCoreData;
        [HideInInspector] public ETimeExtendData extendData = new();
        public bool IsFlowing => _isFlowing;
        private bool _isFlowing = true;

        #region Logic Fields

        private float[] _count_tick_s = new float[4] { 1, 1, 0.5f, 0.25f};
        private int[] _count_tick_l = new int[5] { 2,5,10,60,120 };
        private float _count_delay = 0;
        #endregion
        private int _totalFramesSpan;
        //counter
        private ETimeCounterManager _counterManager = new();
        //case
        /// <summary>
        /// Time data accord to time frame 1/32/24
        /// </summary>
        public int[] Time // hour/min/sec
        {
            get
            {
                return GetTime();
            }
        }
        public int[] Timea
        {
            get
            {
                return GetTime(ETimeType.Absolute);
            }
        }
        #region Event
        public UnityEvent onTickA = new();
        public UnityEvent onTick = new();
        public UnityEvent onCountDownEnd = new();
        #endregion
        #region Init
        public ETime(ETimeData timeCore, float timeScale = 1)
        {
            this.timeCoreData = timeCore;
            CalculateTotalSpan();
            _timeSpeed = timeCore.baseSpeed;
            _timeScale = timeScale;
            extendData.Init(timeCore.timeFrames);
        }
        public ETime AddToTimeManager(ITimeManager timeManager)
        {
            timeManager.Add(this);
            return this;
        }
        #endregion
        #region Actions
        public void Resume()
        {
            _isFlowing = true;
        }
        public void Pause()
        {
            _isFlowing = false;
        }
        public void Stop()
        {
            Pause();
            ResetTime();
        }
        public void ResetTime()
        {
            StartAt(0);
        }
        public void Delay(float delayTime)
        {
            _count_delay = delayTime;
        }
        public ETime StartAt(int second)
        {
            sec = second;
            return this;
        }
        public ETime StartAt(int[] time)
        {
            sec = GetSec(time);
            return this;
        }
        #endregion
        #region Counter
        public void AddCounter(ETimeCounterData counterData)
        {
            _counterManager.Add(this, counterData);
        }
        public int GetCounterSec(int index) => _counterManager.counters[index].Sec;
        public void SetCounterSec(int index, int sec)
        {
            _counterManager.SetSec(index, sec);
        }
        public void ResetAllCounter() => _counterManager.ResetAll();
        public void ResetCounter(int index) => _counterManager.ResetCounter(index);
        public void ResumeCounter(int index) => _counterManager.ResumeCounter(index);
        public void PauseCounter(int index) => _counterManager.PauseCounter(index);
        public void StopAllCounter() => _counterManager.StopAll();
        public void StopCounter(int index) => _counterManager.StopCounter(index);
        public bool IsCounterFlowing(int index) => _counterManager.IsCounterFlowing(index);
        #endregion
        #region Flow
        public void Flow()
        {
            if (_isFlowing)
            {
                float modSpeed = UnityEngine.Time.deltaTime * TimeSpeed;
                if (_count_delay <= 0)
                {
                    _count_tick_s[0] -= UnityEngine.Time.deltaTime;
                    _count_tick_s[1] -= modSpeed;
                    _count_tick_s[2] -= modSpeed;
                    _count_tick_s[3] -= modSpeed;
                }
                else if (_count_delay > 0)
                {
                    _count_delay -= UnityEngine.Time.deltaTime;
                }
                TickCount(ref ticka, ref _count_tick_s[0], 1f);
                TickCount(ref tick, ref _count_tick_s[1], 1f);
                TickCount(ref tickh, ref _count_tick_s[2], 0.5f);
                TickCount(ref tickq, ref _count_tick_s[3], 0.25f);
                TickCount_DependTick(ref tick2, ref _count_tick_l[0], 2);
                TickCount_DependTick(ref tick5, ref _count_tick_l[1], 5);
                TickCount_DependTick(ref tick10, ref _count_tick_l[2], 10);
                TickCount_DependTick(ref tick60, ref _count_tick_l[3], 60);
                TickCount_DependTick(ref tick120, ref _count_tick_l[4], 120);
                if (ticka)  // Main
                {
                    seca += 1;
                    onTickA?.Invoke();
                }
                if (tick)
                {
                    sec += 1; // Main
                    onTick?.Invoke();
                    CalculateFramePercentage();
                    _counterManager.FlowTick();
                }
            }

        }
        private void TickCount(ref bool tickType, ref float count_tickType, float range)
        {

            if (count_tickType <= 0)
            {
                count_tickType = range;
                tickType = true;
            }
            else
            {
                tickType = false;
            }
        }
        private void TickCount_DependTick(ref bool tickType, ref int count_tickType, int range)
        {
            if (tick) count_tickType -= 1;
            if (count_tickType <= 0)
            {
                count_tickType = range;
                tickType = true;
            }
            else
            {
                tickType = false;
            }
        }
        #endregion
        #region Convertion
        public int[] GetTime(ETimeType eTimeStringType = ETimeType.Normal, int counterIndex = 0)
        {
            switch (eTimeStringType)
            {
                case ETimeType.Normal:
                    return GetTime(sec);
                case ETimeType.Absolute:
                    return GetTime(seca);
                case ETimeType.Counter:
                    return GetTime(_counterManager.GetSec(counterIndex));
                default:
                    return GetTime(sec);
            }
        }
        public int[] GetTime(int inputSec)
        {
            int[] ret = new int[timeCoreData.CountFrame];
            int curspan = _totalFramesSpan;
            int cursec = inputSec;
            for (int i = 0; i < timeCoreData.CountFrame; i++)
            {
                ret[i] = cursec / curspan;
                cursec = cursec % curspan;
                curspan /= timeCoreData.timeFrames[i].span;
            }
            return ret;
        }
        public float[] GetDegrees(ETimeType eTimeStringType = ETimeType.Normal, int zeroSpan = 12, int counterIndex = 0)
        {
            switch (eTimeStringType)
            {
                case ETimeType.Normal:
                    return GetDegrees(GetTime(sec), zeroSpan);
                case ETimeType.Absolute:
                    return GetDegrees(GetTime(seca), zeroSpan);
                case ETimeType.Counter:
                    return GetDegrees(GetTime(_counterManager.GetSec(counterIndex)), zeroSpan);
                default:
                    return GetDegrees(GetTime(sec), zeroSpan);
            }
        }
        private float[] GetDegrees(int[] secs, int zeroSpan = 12)
        {
            float[] ret = new float[timeCoreData.CountFrame];
            for (int i = 0; i < secs.Length; i++)
            {
                int span = i == 0 ? zeroSpan : timeCoreData.timeFrames[i - 1].span;
                float fraction = (float)secs[i] / (float)span;
                float radians = fraction * 2 * Mathf.PI;
                ret[i] = radians * Mathf.Rad2Deg;
            }
            return ret;
        }
        #endregion
        #region SP
        private void CalculateTotalSpan()
        {
            _totalFramesSpan = 1;
            for (int i = 0; i < timeCoreData.CountFrame; i++)
            {
                _totalFramesSpan *= timeCoreData.timeFrames[i].span;
            }
        }
        #endregion

        public string GetTimeString(ETimeType eTimeStringType = ETimeType.Normal, bool showFirstPrefix = false)
        {
            return GetTimeString(0, timeCoreData.CountFrame, -1, eTimeStringType, showFirstPrefix);
        }
        public string GetCounterTimeString(int counterIndex = 0, ETimeType eTimeStringType = ETimeType.Counter, bool showFirstPrefix = false)
        {
            return GetTimeString(0, timeCoreData.CountFrame, -1, eTimeStringType, showFirstPrefix, counterIndex);
        }
        public string GetTimeString(int fromFrameIncluded, int toFrameExcluded, ETimeType eTimeStringType = ETimeType.Normal, bool showFirstPrefix = false)
        {
            return GetTimeString(fromFrameIncluded, toFrameExcluded, -1, eTimeStringType, showFirstPrefix);
        }
        public string GetCounterTimeString(int fromFrameIncluded, int toFrameExcluded, int counterIndex = 0, ETimeType eTimeStringType = ETimeType.Counter, bool showFirstPrefix = false)
        {
            return GetTimeString(fromFrameIncluded, toFrameExcluded, -1, eTimeStringType, showFirstPrefix, counterIndex);
        }
        public string GetTimeString(int inputSec, bool showFirstPrefix = false)
        {
            return GetTimeString(0, timeCoreData.CountFrame, inputSec, ETimeType.Normal, showFirstPrefix);
        }
        public string GetTimeString(int fromFrameIncluded, int toFrameExcluded, int inputSec, bool showFirstPrefix = false)
        {
            return GetTimeString(fromFrameIncluded, toFrameExcluded, inputSec, ETimeType.Normal, showFirstPrefix);
        }
        public string GetTimeString(
            int fromFrameIncluded, 
            int toFrameExcluded, 
            int inputSec = -1, 
            ETimeType eTimeStringType = ETimeType.Normal, 
            bool showFirstPrefix = false,
            int counterIndex = 0   
            )
        {
            string ret = "";
            int[] curTime;
            if(inputSec < 0)
            {
                switch (eTimeStringType)
                {
                    case ETimeType.Normal:
                        curTime = Time;
                        break;
                    case ETimeType.Absolute:
                        curTime = Timea;
                        break;
                    case ETimeType.Counter:
                        curTime = GetTime(ETimeType.Counter, counterIndex);
                        break;
                    default:
                        curTime = Time;
                        break;
                }
            }
            else
            {
                curTime = GetTime(inputSec);
            }
            for (int i = fromFrameIncluded; i < toFrameExcluded; i++)
            {
                string addString;
                if (i == fromFrameIncluded && !showFirstPrefix)
                {
                    addString = curTime[i].ToString(GetNumPresent(timeCoreData.timeFrames[i].present));
                }
                else
                {
                    addString = timeCoreData.timeFrames[i].prefix + curTime[i].ToString(GetNumPresent(timeCoreData.timeFrames[i].present));
                }
                if (timeCoreData.timeFrames[i].useEcho)
                {
                    ret += $"<size={timeCoreData.timeFrames[i].echo}%>{addString}<size=100%>";
                }
                else
                {
                    ret += addString;
                }

            }
            return ret;
        }

        private string GetNumPresent(int nc)
        {
            return "D" + nc;
        }
        public int GetSec(int[] time) 
        {
            int ret = 0;
            int mul = 1;
            for (int i = timeCoreData.CountFrame - 1; i >=0; i--)
            {
                mul *= timeCoreData.timeFrames[i].span;
                ret += time[i] * mul;
            }
            return ret;
        }
        /// <summary>
        /// GetTotalSecAndSpan Input 20/30/12 |> 60/60/1 Input: 1 Output: 30*60 + 12*1
        /// </summary>
        /// <param name="frameIndex"></param>
        /// <returns></returns>
        public void GetTotalSecAndSpanOfFrame(int frameIndex, out int totalSec, out int totalSpan)
        {
            totalSec = 0;
            totalSpan = 1;
            for (int i = timeCoreData.CountFrame - 1; i >= frameIndex; i--)
            {
                totalSpan *= timeCoreData.timeFrames[i].span;
                totalSec += Time[i] * totalSpan;
            }
        }
        public float GetPercentageOfFrame(int frameIndex)
        {
            int totalSec = 0;
            int totalSpan = 1;
            for (int i = timeCoreData.CountFrame - 1; i >= frameIndex; i--)
            {
                totalSpan *= timeCoreData.timeFrames[i].span;
                if (i!= frameIndex) totalSec += Time[i] * totalSpan;
            }
            return (float)totalSec / totalSpan;
        }
        /// <summary>
        /// GetTotalSpan
        /// </summary>
        /// <param name="frameIndex"></param>
        /// <returns></returns>
        public int GetSpanOfFrame(int frameIndex)
        {
            int ret = 0;
            int mul = 1;
            for (int i = timeCoreData.CountFrame - 1; i >= frameIndex; i--)
            {
                ret = Time[i] * mul;
                mul *= timeCoreData.timeFrames[i].span;
            }
            return ret;
        }
        public int GetSecFromCounterOfIndex(int index) => _counterManager.GetSec(index);
        public int GetMinFromCounterOfIndex(int index) => Mathf.CeilToInt((float)_counterManager.GetSec(index)/60);

        #region Extend Data
        public void CalculateFramePercentage()// 30/30/30/1 |> 60/60/60/1 => 0.5/0.5/0.5/1
        {
            for (int i = 0; i < extendData.framePercentages.Length; i++)
            {
                if (i == extendData.framePercentages.Length - 1) extendData.framePercentages[i] = 1;
                else
                {
                    extendData.framePercentages[i] = GetPercentageOfFrame(i);
                }
            }
        }
        #endregion


        public enum ETimeType
        {
            Normal,
            Absolute,
            Counter,
        }
    }
    public class ETimeDelay
    {
        public int Sec => _curCountDownSec;
        private bool _isCountDownOn;
        private int _maxCountDownSec;
        private int _curCountDownSec;
        private UnityEvent _onCountDownEnd = new();
        public void SetUp(int sec, bool pauseTimeOnCountDownEnd, UnityAction onCountDownEnd, UnityAction pauseTimeAction)
        {
            _maxCountDownSec = sec;
            _isCountDownOn = true;
            _onCountDownEnd.AddListener(onCountDownEnd);
            if (pauseTimeOnCountDownEnd)
                _onCountDownEnd.AddListener(pauseTimeAction);
            Reset();
        }
        public void FlowTick()
        {
            if (_isCountDownOn)
            {
                _curCountDownSec -= 1;
                if (_curCountDownSec <= 0)
                {
                    _onCountDownEnd?.Invoke();
                }
            }
        }
        public void Reset()
        {
            if (_isCountDownOn)
            {
                _curCountDownSec = _maxCountDownSec;
            }
        }
    }
    public class ETimeCounterData
    {
        internal int fromSec;
        internal int toSec = 0;
        internal UnityAction onCounterEnd;
        internal bool pauseOnEnd = false;
        internal bool resetOnEnd = false;
        internal int randomFromSecIncluded;
        internal int randomToSecExcluded;
        public ETimeCounterData(int fromSec, int toSec = 0)
        {
            this.fromSec = fromSec;
            this.toSec = toSec;
        }
        public ETimeCounterData AddEvent(UnityAction onCounterEnd)
        {
            this.onCounterEnd = onCounterEnd;
            return this;    
        }
        public ETimeCounterData PauseOnEnd()
        {
            pauseOnEnd = true;
            return this;
        }
        public ETimeCounterData ResetOnEnd(int randomFromSecIncluded = 0, int randomToSecExcluded = 0)
        {
            this.randomFromSecIncluded = randomFromSecIncluded;
            this.randomToSecExcluded = randomToSecExcluded;
            resetOnEnd = true;
            return this;
        }
    }
    internal class ETimeCounter
    {
        public int Sec => _curCountSec;
        public bool IsFlowing => _isFlowing;
        private bool _isFlowing = true;
        private int _fromCountSec;
        private int _toCountSec;
        private int _curCountSec;
        private UnityEvent _onCounterEnd = new();
        private ETime _eTime;
        private int _direction;
        //reset
        private int randomFromSecIncluded = 0;
        private int randomToSecExcluded = 0;

        public void SetUp(ETime eTime, ETimeCounterData counterData)
        {
            _eTime = eTime;
            _fromCountSec = counterData.fromSec;
            _toCountSec = counterData.toSec;
            _isFlowing = true;
            _onCounterEnd = new();
            if (counterData.onCounterEnd != null) _onCounterEnd.AddListener(counterData.onCounterEnd);
            if (counterData.pauseOnEnd) _onCounterEnd.AddListener(_eTime.Pause);
            //reset on end
            if (counterData.resetOnEnd) ResetOnEndSetUp(counterData.randomFromSecIncluded, counterData.randomToSecExcluded);
            //
            _direction = (_toCountSec - _fromCountSec).Normalize();
            if (_direction == 0)
            {
                this.LogError("Direction == 0, counter function turn off");
                _isFlowing = false;  
            }
            ResetTime();
        }
        public void Resume()
        {
            _isFlowing = true;
        }
        public void Pause()
        {
            _isFlowing = false;
        }
        public void Stop()
        {
            Pause();
            ResetTime();
        }
        public void FlowTick()
        {
            if (_isFlowing)
            {
                _curCountSec += _direction;
                if (_direction == 1)
                {
                    if (_curCountSec >= _toCountSec)
                    {
                        _onCounterEnd?.Invoke();
                    }
                }
                else
                {
                    if (_curCountSec <= _toCountSec)
                    {
                        _onCounterEnd?.Invoke();
                    }
                }
            }
        }
        /// <summary>
        /// Set sec of _curCountSec
        /// </summary>
        /// <param name="sec"></param>
        public void SetSec(int sec)
        {
            _curCountSec = sec;
        }
        /// <summary>
        /// Reset sec to _fromCountSec
        /// </summary>
        public void ResetTime()
        {
            _curCountSec = _fromCountSec;
        }
        public void ResetRandom()
        {
            _curCountSec = UnityEngine.Random.Range(randomFromSecIncluded, randomToSecExcluded);
        }
        private void ResetOnEndSetUp(int randomFrom, int randomTo)
        {
            this.randomFromSecIncluded = randomFrom;
            this.randomToSecExcluded = randomTo;
            if (randomFromSecIncluded == randomToSecExcluded && randomFromSecIncluded == 0)
            {
                //no random
                _onCounterEnd.AddListener(ResetTime);
            }
            else
            {
                //random
                _onCounterEnd.AddListener(ResetRandom);
            }
        }
    }
    internal class ETimeCounterManager
    {
        internal List<ETimeCounter> counters = new();
        public void FlowTick()
        {
            foreach (var item in counters)
            {
                item.FlowTick();
            }
        }
        public void Add(ETime eTime,ETimeCounterData counterData)
        {
            ETimeCounter counter = new ETimeCounter();
            counter.SetUp(eTime, counterData);
            counters.Add(counter);
        }
        public int GetSec(int index)
        {
            return counters[index].Sec;
        }
        public void SetSec(int index, int sec)
        {
            counters[index].SetSec(sec);
        }



        public void ResetCounter(int index) => counters[index].ResetTime();
        public void ResetAll()
        {
            foreach (var item in counters)
            {
                item.ResetTime();
            }
        }
        public void ResumeCounter(int index) => counters[index].Resume();
        public void PauseCounter(int index) => counters[index].Pause();
        public void StopCounter(int index) => counters[index].Stop();
        public bool IsCounterFlowing(int index) => counters[index].IsFlowing;
        public void StopAll()
        {
            foreach (var item in counters)
            {
                item.Stop();
            }
        }
    }
}