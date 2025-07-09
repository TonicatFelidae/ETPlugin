using ET.UIKit.ZenjectUIScreen;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Engine
{
    public interface IFactoryManager<U>
    {
        public U CurItem { get; }
        public T Load<T>() where T : U;
        public T Get<T>() where T : U;
        public T Unload<T>() where T : U;
        public T UpdateNewState<T>() where T : U;
        public Dictionary<string, U> ItemDict { get; set; }
    }
}