using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{
    public class Identifix : Singleton<Identifix>
    {
        static Dictionary<string, int> _IDData = new Dictionary<string, int>();
        static int curID = -1;
        /// <summary>
        /// Get info form a group of IIDItem, then choose the suitable ID 
        /// Exam of use: you have 10 quest that already have their  ID, now you create a new quest that required a unique ID
        /// Result: 000123 or prefix000123
        /// </summary>
        /// <param name="iDItems"></param>
        /// <returns></returns>
        public static string GetID(List<string> iDItems, string prefixID = null)
        {
            List<string> IDs = iDItems;
            string ret = "";
            string prefix = string.IsNullOrEmpty(prefixID) ? "" : prefixID;
            do
            {
                ret = prefix + ConstructID_Random6Digit();
            }
            while (IDs.Contains(ret));
            return ret;
        }
        /// <summary>
        /// Get info form a group of IIDItem, then choose the suitable ID 
        /// Exam of use: you have 10 quest that already have their  ID, now you create a new quest that required a unique ID
        /// Result: 000123 or prefix000123
        /// </summary>
        /// <param name="iDItems"></param>
        /// <returns></returns>
        public static string GetID<T>(List<T> iDItems, string prefixID = null) where T : IIDItem
        {
            List<string> IDs = iDItems.Select(x => x.ID).ToList();
            string ret = "";
            string prefix = string.IsNullOrEmpty(prefixID) ? "" : prefixID;
            do
            {
                ret = prefix + ConstructID_Random6Digit();
            }
            while (IDs.Contains(ret));      
            return ret;
        }
        /// <summary>
        /// Get info form a group of IIDItem, then choose the suitable ID 
        /// Exam of use: you have 10 quest that already have their  ID, now you create a new quest that required a unique ID
        /// Result: 000123 or prefix000123
        /// </summary>
        /// <param name="iDItems"></param>
        /// <returns></returns>
        public static string GetID<T>(Dictionary<string, T> iDItemDict, string prefixID = null) where T: IIDItem
        {
            List<string> IDs = iDItemDict.Select(x => x.Value.ID).ToList();
            string ret = "";
            string prefix = string.IsNullOrEmpty(prefixID) ? "" : prefixID;
            do
            {
                ret = prefix + ConstructID_Random6Digit();
            }
            while (IDs.Contains(ret));
            return ret;
        }
        /// <summary>
        /// Get specific ID for this item.
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="defaultID"></param>
        /// <returns></returns>
        public static string GetID<T>(T sample, string prefixID = null) where T : IIDItem
        {
            string ret = prefixID;
            if (string.IsNullOrEmpty(ret))
            {
                ret = sample.GetType().Name;
                if (_IDData.ContainsKey(ret)) return ConstructID(ret);
                else
                {
                    _IDData.Add(ret, 0);
                    return ConstructID(ret);
                }
            }
            else
            {
                if (_IDData.ContainsKey(ret)) return ConstructID(ret);
                else
                {
                    _IDData.Add(ret, 0);
                    return ConstructID(ret);
                }
            }
        }
        static string ConstructID(string prefixID)
        {
            return prefixID;
        }
        //public static string Process(IIDItem sample)
        //{
        //    string curID = sample.ID;
        //    if (string.IsNullOrEmpty(curID))
        //    {
        //        curID = sample.GetType().Name;
        //        if (_IDData.ContainsKey(curID)) return ConstructID(curID);
        //        else
        //        {
        //            _IDData.Add(curID, 0);
        //            return ConstructID(curID);
        //        }
        //    }
        //    else
        //    {
        //        if (_IDData.ContainsKey(curID)) return ConstructID(curID);
        //        else
        //        {
        //            _IDData.Add(curID, 0);
        //            return ConstructID(curID);
        //        }
        //    }
        //    
        //}
        //static string ConstructID(string sample)
        //{
        //    string ret;
        //    ret = $"{sample}_{_IDData[sample]}";
        //    _IDData[sample] += 1;
        //    return ret;
        //}
        /// <summary>
        /// Construct ascending ID. Best use for dictionary that when you dont need to know about that ID.
        /// </summary>
        /// <returns></returns>
        public static int ConstructID()
        {
            curID += 1;
            return curID;
        }
        /// <summary>
        /// 000001 123456 999999
        /// </summary>
        /// <returns></returns>
        private static string ConstructID_Random6Digit()
        {
            int randomNum = Random.Range(0,1000000);
            return randomNum.ToString("D6");
        }
    }
}

