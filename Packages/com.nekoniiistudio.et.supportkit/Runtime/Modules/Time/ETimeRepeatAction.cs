using ET.SupportKit.EMath;
using System;
using UnityEngine.Events;
using UnityEngine;

namespace ET
{
    public class ETimeRepeatAction
    {
        public float CurTime => _curCountTime;
        public bool IsFlowing => _isFlowing;
        private bool _isFlowing = true;
        private bool _useUnscaleTime;
        private float _curCountTime;
        private float _setcountTime;
        private UnityAction _repeatAction;

        public void SetUp(
            UnityAction repeatAction,
            float countTime,
            bool doActionAtStart = false, 
            bool useUnscaledDeltaTime = false)
        {
            _isFlowing = false;
            _repeatAction = repeatAction;
            if (doActionAtStart)
            {
                _repeatAction.Invoke();
            }
            _setcountTime = countTime;
            _useUnscaleTime = useUnscaledDeltaTime;  
            ResetTime();
        }
        public void RestartAndPlay()
        {
            ResetTime();
            _isFlowing = true;
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
        public void Flow()
        {
            if (_isFlowing)
            {
                if (_useUnscaleTime)
                {
                    _curCountTime -= Time.unscaledDeltaTime;
                }
                else
                {
                    _curCountTime -= Time.deltaTime;
                }
                if (_curCountTime <= 0)
                {
                    _repeatAction.Invoke();
                    ResetTime();
                }
            }
        }
        /// <summary>
        /// Set sec of _curCountSec
        /// </summary>
        /// <param name="sec"></param>
        public void SetSec(int sec)
        {
            _curCountTime = sec;
        }
        /// <summary>
        /// Reset sec to _fromCountSec
        /// </summary>
        public void ResetTime()
        {
            _curCountTime = _setcountTime;
        }
    }
}