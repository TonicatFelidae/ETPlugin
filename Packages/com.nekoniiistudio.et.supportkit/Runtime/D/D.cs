using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ET.SupportKit;
using ET.Engine;
using System.Linq;

namespace ET
{
    public class D_Demo
    {
        List<int> newList = new();
        int[] newListw = new int[5] { 1, 1, 1, 1, 1 };
        public void TryMethod()
        {
            D.LogList(newList);
        }

    }
         
    public static class D
    {
        /*
         * Colorful debug for system 
         * 
         * 
         */
        static class DColor
        {
            public static string InstallSetting = "db14ff"; // Install setting
            public static string Install = "eeb1fa"; // Install system
            public static string InstallONG = "d1b0ff"; // Install system
            public static string FindError = "#eaeda6";
            public static string NotifyData = "3dbd39";
            public static string CuteNoff_PiggyPink = "ffdde4";
            public static string FileSystem_Yellow = "#f1f5ab";
            public static string ChainConditionWarning_LightBlueGray = "f5feff";
            public static string Confirm_LightGreen = "0dba21";
            public static string DataVerify = "#12941f";
            public static string TypeLog = "#4EC9B0";

        }
        public static class Sys
        {
            public static void InstallSetting(string tx = "")
            {
                if (!string.IsNullOrEmpty(tx))
                {
                    Color color = ET_Color.Get(DColor.InstallSetting);
                    Log($"ETEngine: [Imported] {tx}", color);
                }
            }
            public static void InstallSystem(string tx = "")
            {
                if (!string.IsNullOrEmpty(tx))
                {
                    Color color = ET_Color.Get(DColor.Install);
                    Log($"ETEngine: [Installed] {tx}", color);
                }
            }
            public static void InstallSystemONG(string tx = "")
            {
                if (!string.IsNullOrEmpty(tx))
                {
                    Color color = ET_Color.Get(DColor.InstallONG);
                    Log($"ETEngine: [Installed] {tx}", color);
                }
            }
            public static void InstallSystemError(string tx = "")
            {
                if (!string.IsNullOrEmpty(tx))
                {
                    Color color = ET_Color.Get(DColor.Install);
                    Log($"ETEngine: [ERROR] Installed {tx}", color);
                }
            }
            public static void DataVerify(string tx = "")
            {
                if (!string.IsNullOrEmpty(tx))
                {
                    Color color = ET_Color.Get(DColor.DataVerify);
                    Log($"Check: [ERROR] Installed {tx} Accessable!", color);
                }
            }
            /// <summary>
            /// Show findind asset error
            /// </summary>
            /// <param name="tx"></param>
            /// <param name="searchIn"></param>
            public static void FindError(string tx = "", string searchIn = "")
            {
                if (!string.IsNullOrEmpty(tx))
                {
                    Color color = ET_Color.Get(DColor.FindError);
                    if (searchIn!="")
                    {
                        Log($"Can't find {tx} in {searchIn}", color);
                    }
                    else
                    {
                        Log($"Can't find {tx}", color);
                    }
                }
            }

            public static void NotifyData(string tx = "")
            {
                if (!string.IsNullOrEmpty(tx))
                {
                    Color color = ET_Color.Get(DColor.NotifyData);
                    Log($"{tx}", color);
                }
            }

            public static void File(string tx = "")
            {
                if (!string.IsNullOrEmpty(tx))
                {
                    Color color = ET_Color.Get(DColor.FileSystem_Yellow);
                    Log($"{tx}", color);
                }
            }
            
            public static void ChainConditionWarning(string tx = "")
            {
                if (!string.IsNullOrEmpty(tx))
                {
                    Color color = ET_Color.Get(DColor.ChainConditionWarning_LightBlueGray);
                    LogWarning($"{tx}", color);
                }
            }
            public static void Confirm(string tx = "")
            {
                if (!string.IsNullOrEmpty(tx))
                {
                    Color color = ET_Color.Get(DColor.Confirm_LightGreen);
                    Log($"{tx}", color);
                }
            }
        }// basic
        public static class Not
        {
            public static void CuteNot(string tx = "")
            {
                if (tx!=null)
                {
                    Color color = ET_Color.Get(DColor.CuteNoff_PiggyPink);
                    Log($"Heyyyy! I am here! oik! oik! {tx}", color);
                }
            }
        }
        public static void Log(string tx = "")
        {
            
            Debug.Log(tx);
        }
        public static void LogList(IEnumerable self, string tx = "Array log : ")
        {
            string ret = tx;
            foreach (var item in self)
            {
                ret += item+"|";
            }
            Debug.Log(ret);
        }
        /// <summary>
        /// Log lists inside lists
        /// </summary>
        /// <param name="self"></param>
        /// <param name="tx"></param>
        public static void LogLists(IEnumerable<IEnumerable> self, string tx = "Array log : ")
        {
            string ret = tx;
            foreach (var item in self)
            {
                foreach (var itemx in item)
                {
                    ret += itemx + "|";
                }
                ret += "////";
            }
            Debug.Log(ret);
        }
        public static void Log(string tx, Color color)
        {
            Debug.Log(string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(color.r * 255f), (byte)(color.g * 255f), (byte)(color.b * 255f), tx));
        }
        public static void LogWarning(string tx = "")
        {
            Debug.LogWarning(tx);
        }
        public static void LogWarning(string tx, Color color)
        {
            Debug.LogWarning(string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(color.r * 255f), (byte)(color.g * 255f), (byte)(color.b * 255f), tx));
        }
        public static void LogErrorEditor(string tx = "")
        {
            Debug.LogError($"[EDITOR] {tx}");
        }
        public static void LogError(string tx = "")
        {
            Debug.LogError(tx);
        }
        public static void LogError(string tx, Color color)
        {
            Debug.LogError(string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(color.r * 255f), (byte)(color.g * 255f), (byte)(color.b * 255f), tx));
        }
        #region Self logger
        /// <summary>
        /// Log out note with type name indication. 
        /// exam : [Type] have something happen
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="tx"></param>
        public static void Log<T>(this T type, string tx) where T : class
        {
            Debug.Log(GetStringWithColorFormat(ET_Color.Get(D.DColor.TypeLog) ,$"[{type.GetType()}] {tx}"));
        }
        public static void LogWarning<T>(this T type, string tx) where T : class
        {
            Debug.LogWarning(GetStringWithColorFormat(ET_Color.Get(D.DColor.TypeLog), $"[{type.GetType()}] {tx}"));
        }
        public static void LogError<T>(this T type, string tx) where T : class
        {
            Debug.LogError(GetStringWithColorFormat(ET_Color.Get(D.DColor.TypeLog), $"[{type.GetType()}] {tx}"));
        }
        #endregion
        public static string GetStringWithColorFormat(Color color, string tx)
        {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(color.r * 255f), (byte)(color.g * 255f), (byte)(color.b * 255f), tx);
        }
        
    }

}

