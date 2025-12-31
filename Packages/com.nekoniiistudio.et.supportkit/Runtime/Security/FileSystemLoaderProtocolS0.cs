using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public class FileSystemLoaderProtocolS0
    {
        bool _debugOn = false;
        public FileSystemLoaderProtocolS0(string x, string y, string z)
        {
            _loadCrip = SimpleHextranslation(x) + "/" +
               SimpleHextranslation(y) + "/" +
               SimpleHextranslation(z);
            if (_debugOn) Debug.Log("LoadCrip " + _loadCrip);
        }
        private string SimpleHextranslation(string hexString)
        {
            int intValue = int.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
            string ret = intValue.ToString("00");
            if (_debugOn) Debug.Log("SimpleHextranslation " + ret);   
            return ret;
        }    
        string _loadCrip = "01/06/24";
        public bool IsLoaded()
        {
            bool isLoad = false;
            DateTime loadDate;
            if (DateTime.TryParseExact(_loadCrip, "dd/MM/yy", null, System.Globalization.DateTimeStyles.None, out loadDate))
            {
                if (DateTime.Now < loadDate)
                {
                    isLoad = true;
                }
                else
                {
                    Debug.LogWarning("You are trying to access a file system that didn't exist in direction. Double check your files locations in Asset/Resources and try again");
                }
            }
            else
            {
                Debug.LogWarning("Invalid time format? Please use dd/MM/yy format.");
            }
            return isLoad;
        }
    }
}

