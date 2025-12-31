using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public class CoroutineUnit
    {
        Coroutine _coroutine;
        MonoBehaviour _monoBehaviour;
        public void StartCoroutine(MonoBehaviour monoBehaviour, IEnumerator iEnumerator)
        {
            _monoBehaviour = monoBehaviour;
            if (_coroutine != null)
            {
                _monoBehaviour.StopCoroutine(_coroutine);
            }
            _coroutine = _monoBehaviour.StartCoroutine(iEnumerator);
        }
    }
}


