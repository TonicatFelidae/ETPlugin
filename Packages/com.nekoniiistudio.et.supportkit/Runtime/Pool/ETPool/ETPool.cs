using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ET
{
    /// <summary>
    /// Add singerton will break patern, DO NOT ADD
    /// </summary>
    /// <summary>
    /// Simple pool system that handle turn on and off MonoBehaviour pool item gameobject => object focus, singleton for class case
    /// PreLoad ahead x items. 
    /// Limited singel type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ETPool<T> where T : Component
    {
        List<T> _objects;
        Transform _container;
        T _pp_poolObject;
        int _initialSize = 5; // PreLoad ahead x items.
        public ETPool(Transform container, T pp_poolObject, int initialSize = 5)
        {
            _container = container;
            _pp_poolObject = pp_poolObject;
            _initialSize = initialSize;
            AddObjectsToPool();
        }
        public T GetObject()
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                if (_objects[i] == null) _objects.Remove(_objects[i]);
                else if (_objects[i].gameObject.activeSelf == false)
                {
                    _objects[i].gameObject.SetActive(true);
                    return _objects[i];
                }
            }
            return AddObjectsToPoolAndGetLast();
        }
        public void CleanPool()
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                _objects[i].gameObject.SetActive(false);
            }
        }
        public void ReturnToPool(T go)
        {
            go.gameObject.SetActive(false);
        }
        void AddObjectsToPool()
        {
            if (_objects == null) _objects = new();
            for (int i = 0; i < _initialSize; i++)
            {
                T newGoItem = GameObject.Instantiate<T>(_pp_poolObject, _container);
                newGoItem.gameObject.SetActive(false);
                _objects.Add(newGoItem);
            }
        }
        T AddObjectsToPoolAndGetLast()
        {
            if (_objects == null) _objects = new();
            for (int i = 0; i < _initialSize; i++)
            {
                T newGoItem = GameObject.Instantiate<T>(_pp_poolObject, _container);
                newGoItem.gameObject.SetActive(false);
                _objects.Add(newGoItem);
                if (i == _initialSize - 1)
                {
                    newGoItem.gameObject.SetActive(true);
                    return newGoItem;
                }
            }
            return null;
        }
    }
}