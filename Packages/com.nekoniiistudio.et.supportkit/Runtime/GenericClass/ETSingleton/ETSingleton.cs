using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        
        public static T Instance
        {
            get
            {
                if (Singleton<T>._instance == null)
                {
                    Singleton<T>._instance = (UnityEngine.Object.FindFirstObjectByType(typeof(T),FindObjectsInactive.Include) as T);
                    if (Singleton<T>._instance == null)
                    {
                        Singleton<T>._instance = new GameObject().AddComponent<T>();
                        Singleton<T>._instance.gameObject.name = Singleton<T>._instance.GetType().Name;
                    }
                }
                return Singleton<T>._instance;
            }
        }

        public void Reset()
        {
            Singleton<T>._instance = (T)((object)null);
        }

        public static bool Exists()
        {
            return Singleton<T>._instance != null;
        }
    }

}

