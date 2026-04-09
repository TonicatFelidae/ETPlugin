using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ET.SupportKit;
using ET.SupportKit.EMath;

namespace ET.PowerStruct
{
    [Serializable]
    public class SimpleChanceGroup<T>
    {
        public SimpleChanceItem<T>[] items;
        private bool initialized = false;
        private void Init()
        {
            int total = 0;
            bool isAllZero = true;
            foreach (var item in items)
            {
                total += item.chance;
                if (item.chance != 0) isAllZero = false;
            }
            if (total == 0)
            {
                if (isAllZero)
                {
                    float startChance = 0;
                    float chance = 1 / (float)total;
                    for (int i = 0; i < items.Length; i++)
                    {
                        items[i].chanceInPercent.x = startChance;
                        items[i].chanceInPercent.y = startChance + chance;
                    }
                }
            }
            else
            {
                float startChance = 0;
                for (int i = 0; i < items.Length; i++)
                {
                    float chance = (float)items[i].chance / (float)total;
                    items[i].chanceInPercent.x = startChance;
                    items[i].chanceInPercent.y = startChance + chance;
                    startChance += chance;
                }
            }
            initialized = true;
        }
        public T Get(int i) => items[i].item;
        public int GetChanceOf(T item)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].item != null && items[i].item.Equals(item))
                {
                    return items[i].chance;
                }
            }
            return 0;
        }
        public T GetRandom()
        {
            if (!initialized) Init();
            float random = UnityEngine.Random.Range(0f, 1f);
            foreach (var item in items)
            {
                if (random.IsBetweenRange(item.chanceInPercent.x, item.chanceInPercent.y))
                {
                    return item.item;
                }
            }
            return items[0].item;

        }
        /// <summary>
        /// Setup and also Init
        /// </summary>
        /// <param name="values"></param>
        /// <param name="chance"></param>
        public void SetUpItems(T[] values, int[] chance)
        {
            if (values.Length != chance.Length)
            {
                Debug.LogError("Values and chance not equal");
                return;
            }
            items = new SimpleChanceItem<T>[values.Length];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = new SimpleChanceItem<T>(values[i], chance[i]);
            }
            Init();
            InvalidateCache();
        }
        /// <summary>
        /// Gets the count of items in the group.
        /// </summary>
        public int Count => items?.Length ?? 0;

        private List<T> _cachedList;
        private Dictionary<string, T> _cachedDict;

        private void InvalidateCache() { _cachedList = null; _cachedDict = null; }

        public List<T> GetList()
        {
            if (_cachedList == null)
            {
                _cachedList = new List<T>(items?.Length ?? 0);
                if (items != null)
                {
                    foreach (var item in items)
                        _cachedList.Add(item.item);
                }
            }
            return _cachedList;
        }
        public Dictionary<string, T> GetDictionary()
        {
            if (!typeof(IIDItem).IsAssignableFrom(typeof(T)))
            {
                Debug.LogError($"GetDictionary requires T to implement IIDItem, but T is {typeof(T).Name}");
                return null;
            }
            if (_cachedDict == null)
            {
                _cachedDict = new Dictionary<string, T>(items?.Length ?? 0);
                if (items != null)
                {
                    foreach (var item in items)
                        _cachedDict[((IIDItem)item.item).ID] = item.item;
                }
            }
            return _cachedDict;
        }
    }
    [Serializable]
    public struct SimpleChanceItem<T>
    {
        public T item;
        public int chance;
        [NonSerialized] public Vector2 chanceInPercent;

        public SimpleChanceItem(T item, int chance)
        {
            this.item = item;
            this.chance = chance;
            chanceInPercent = Vector2.zero;
        }
    }
}
