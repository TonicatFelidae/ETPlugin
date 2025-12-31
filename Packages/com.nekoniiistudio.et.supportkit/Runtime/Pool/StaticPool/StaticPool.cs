using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ET
{
    [Serializable]
    public class StaticPool
    {
        [SerializeField] private GameObject[] _listObject;
        public GameObject GetObject()
        {
            for (int i = 0; i < _listObject.Length; i++)
            {
                if (_listObject[i].activeSelf == false)
                {
                    _listObject[i].SetActive(true);
                    return _listObject[i];
                }
            }
            return null;
        }
        public void CleanPool()
        {
            for (int i = 0; i < _listObject.Length; i++)
            {
                _listObject[i].SetActive(false);
            }
        }
        public void ReturnToPool(GameObject go)
        {
            go.SetActive(false);
        }
    }
}
