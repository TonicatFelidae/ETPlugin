using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Windows;

namespace ET.SupportKit.Collection
{
    public static class ET_CollectionIdentifix
    {
        /// <summary>
        /// ET Returns a new dictionary, get error mean you not worthy to use this code. 
        /// </summary>
        public static List<string> ToListID_IItemID<T>(this IEnumerable<T> self)
        {
            var result = new List<string>();
            foreach (var e in self)
            {
                result.Add(((IIDItem)e).ID);
            }
            return result;
        }
        /// <summary>
        /// ET Returns a new dictionary, get error mean you not worthy to use this code. 
        /// </summary>
        public static Dictionary<string, T> ToDictionary_IItemID<T>(this IEnumerable<T> self)
        {
            var result = new Dictionary<string, T>();
            foreach (var e in self)
            {
                result.Add(((IIDItem)e).ID, e);
            }
            return result;
        }
        public static Dictionary<string, IIDItem> ToDictionary_IItemID_Interface(this IEnumerable<IIDItem> self)
        {
            var result = new Dictionary<string, IIDItem>();
            if (self != null && self.Count() > 0)
                foreach (var e in self)
                {
                    result.Add(e.ID, e);
                }
            return result;
        }
        /// <summary>
        /// Separate the same key with 6 digits Identifix
        /// </summary>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <param name="self"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        //public static Dictionary<TA, TB> Merge_SeparateSameKeyWithIdentifix<TA, TB>(this Dictionary<TA, TB> self, Dictionary<TA, TB> target)
        //{
        //    Dictionary<TA, TB> ret = target;
        //    foreach (var item in self)
        //    {
        //        if (ret.ContainsKey(item.Key))
        //        {
        //            bool endsWithSixNumbers = CheckIfEndsWithSixNumbers(item.Key.ToString(), out string number);
        //            if (endsWithSixNumbers)
        //            {
        //
        //            }
        //            else
        //            {
        //                ret.Add(item.Key , item.Value);
        //            }
        //        }
        //        else
        //        {
        //            ret.Add(item.Key, item.Value);
        //        }
        //    }
        //    return ret;
        //}
        //public static bool CheckIfEndsWithSixNumbers(string input, out string number)
        //{
        //    number = null;
        //    string pattern = @"\d{6}$"; // Regular expression pattern to match six numbers at the end of the string
        //
        //    Match match = Regex.Match(input, pattern);
        //
        //    if (match.Success)
        //    {
        //        number = match.Value;
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
    }
       

}
