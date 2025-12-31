using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace ET.UIKit
{
    [Serializable]
    public class SplashScreenMachine
    {
        public SplashScreenItem[] screens;
        public IEnumerator RunSplashScreen(UnityAction onFinished)
        {
            foreach (var screen in screens)
            {
                screen.screen.SetActive(false);
            }
            foreach (var screen in screens)
            {
                screen.screen.SetActive(true);
                yield return new WaitForSeconds(screen.durationSeconds);
            }
            onFinished?.Invoke();
        }
    }
    [Serializable]
    public struct SplashScreenItem
    {
        public GameObject screen;
        public float durationSeconds;
    }
}

