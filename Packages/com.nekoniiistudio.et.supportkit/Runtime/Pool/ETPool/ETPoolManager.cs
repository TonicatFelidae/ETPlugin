using System;
using System.Collections.Generic;
using UnityEngine;
// TO ET (do not return singerton)

namespace ET
{
    /// <summary>
    /// ETPoolManager bind in multi scene, using zenject or singleton container must be undestroyable
    /// </summary>
    public class ETPoolManager
    {
        //using string for ID instead of type because item may have same type
        private Dictionary<string, object> poolDictionary = new Dictionary<string, object>();
        private Transform autoContainer;

        public void CreatePool<T>(string ID, Transform container, T prefab, int initialSize = 5) where T : Component
        {
            var pool = new ETPool<T>(GetContainer(container), prefab, initialSize);
            poolDictionary[ID] = pool;
        }
        //get and creaste
        public ETPool<T> GetPool<T>(string ID) where T : Component
        {
            if (poolDictionary.TryGetValue(ID, out var pool))
            {
                return pool as ETPool<T>;
            }
            else
            {
                throw new Exception($"Pool for id {ID} does not exist.");
            }
        }
        public ETPool<T> GetPool<T>(string ID, Transform container, T prefab, int initialSize = 5) where T : Component
        {
            if (poolDictionary.TryGetValue(ID, out var pool))
            {
                return pool as ETPool<T>;
            }
            else
            {
                CreatePool(ID, GetContainer(container), prefab, initialSize);
                return GetPool<T>(ID);
            }
        }
        public T GetObjectFromPool<T>(string ID, int initialSize = 5) where T : Component => GetObjectFromPool(ID, null, Resources.Load<T>(ID), initialSize);
        public T GetObjectFromPool<T>(string ID, Transform container, GameObject prefab, int initialSize = 5) where T : Component => GetObjectFromPool(ID, container, prefab.GetComponent<T>(), initialSize);
        public T GetObjectFromPool<T>(string ID, Transform container, T prefab, int initialSize = 5) where T : Component
        {
            var pool = GetPool<T>(ID, GetContainer(container), prefab, initialSize);
            return pool.GetObject();
        }
        public T GetObjectFromPool<T>(string ID) where T : Component
        {
            return GetPool<T>(ID).GetObject();
        }
        public void ReturnObjectToPool<T>(T go) where T : Component
        {
            go.gameObject.SetActive(false);
        }
        //SP
        private Transform GetContainer(Transform container = null)
        {
            if (container == null)
            {
                if (autoContainer == null)
                {
                    GameObject go = new GameObject();
                    autoContainer = go.transform;
                    go.AddComponent<SC_DontDestroyOnLoad>();
                }
                autoContainer.name = "PoolContainer";
                return autoContainer;
            }
            return container;
        }
    }

}