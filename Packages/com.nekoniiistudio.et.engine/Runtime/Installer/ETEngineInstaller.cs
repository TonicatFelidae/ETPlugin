using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace ET.Engine.Installer
{
    /// <summary>
    /// Type 0 installer class : EngineInstaller
    /// Ver 240219
    /// </summary>
    public abstract class ETEngineInstaller : MonoInstaller
    {
        private LateInstaller _lateInstaller = new();
        public override void InstallBindings()
        {
            SingleBinding();
            SingleInitiation();
            _lateInstaller.Install();
        }
        /// <summary>
        /// Quickly install signal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected void InstallSignal<T>()
        {
            Container.DeclareSignal<T>().OptionalSubscriber();
        }
        /// <summary>
        /// Bind single class functions in here. Can also install buildin installer
        /// </summary>
        public abstract void SingleBinding();
        /// <summary>
        /// Use class init in here (best avoid)
        /// </summary>
        public abstract void SingleInitiation();
        /// <summary>
        /// Subcribe install acts to late installer, they will invoke after all basic installer completed
        /// </summary>
        /// <param name="installAct"></param>
        protected void SubcribeLateInstaller(UnityAction installAct)
        {
            _lateInstaller.Add(installAct);
        }
        public class LateInstaller
        {
            private List<UnityAction> _prefabInstallers = new();
            public void Add(UnityAction installAct)
            {
                _prefabInstallers.Add(installAct);
            }
            public void Install()
            {
                foreach (UnityAction action in _prefabInstallers)
                {
                    action.Invoke();
                }
            }
        }
    }
}
