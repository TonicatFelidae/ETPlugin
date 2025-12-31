using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace ET.SupportKit.Mutable
{
    public static class ValueChangeChecker
    {
        public static T SetValue<T>(this T value1, T value2, out bool isDifferent)
        {
            isDifferent = false;
            if (!value1.Equals(value2))
            {
                isDifferent = true;
                return value2;
            }
            return value1;
        }
        /// <summary>
        /// Set value and invoke a action if value different from the base value. Action invoke after set value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="actionIfDifferrent"></param>
        /// <returns></returns>
        public static void SetValue<T>(ref T valueBase, T valueSet, UnityAction actionIfDifferrent) 
        {
            if (!valueBase.Equals(valueSet))
            {
                valueBase = valueSet;
                actionIfDifferrent.Invoke();
            }
        }

        public static Dictionary<TA, TB> ForceAddDetectDifferent<TA, TB>(this Dictionary<TA, TB> self, TA key, TB value, out bool isDifferent, UnityAction action = null)
        {
            isDifferent = false;
            if (self == null) self = new Dictionary<TA, TB>();
            if (self.ContainsKey(key))
            {
                if (!self[key].Equals(value))
                {
                    self[key] = value;
                    isDifferent = true;
                }
            }
            else
            {
                self.Add(key, value);
                isDifferent = true;
            }
            return self;
        }

        public static Dictionary<TA, TB> ForceAddDetectDifferent<TA, TB>(this Dictionary<TA, TB> self, TA key, TB value, UnityAction actionIfDifferrent, UnityAction action = null)
        {
            if (self == null) self = new Dictionary<TA, TB>();
            if (self.ContainsKey(key))
            {
                if (!self[key].Equals(value))
                {
                    self[key] = value;
                    actionIfDifferrent.Invoke();
                }
            }
            else
            {
                self.Add(key, value);
                actionIfDifferrent.Invoke();
            }
            return self;
        }
        public static bool EqualWithAnyOf<T>(this T obj, params T[] args)
        {
            return args.Contains(obj);
        }
    }
}
