using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ET.SupportKit
{
    public class EAction : Singleton<EAction>
    {
        public void InvokeCorotine(UnityAction action, float time)
        {
            StartCoroutine(IEInvoke(action, time));
        }
        private IEnumerator IEInvoke(UnityAction action, float time)
        {
            yield return new WaitForSeconds(time);
            action?.Invoke();
        }

    }
}
