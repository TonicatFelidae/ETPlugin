using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace ET.Module.VersionControlSystem
{
    internal class VersionControl
    {
        private string _versionURL = null;
        private VersionInfo _versionInfo;
        internal VersionInfo VersionInformation => _versionInfo;
        private int _connectTryingTime = 10;
        private string _VersionURL
        {
            get
            {
                if (_versionURL == null)
                {
                    _versionURL = GenLink();
                }
                return _versionURL;
            }
        }
        private string GenLink()
        {
            string me = "tonicatfelidae";
            string very = "github";
            string cute = "io";
            string dotx = ".";
            return "https://" + me + dotx + very + dotx + cute + "/version_" + Application.productName + ".json";
        }
        internal IEnumerator GetVersionInfo(UnityAction onSuccess, UnityAction onFailed)
        {
            UnityWebRequest request = UnityWebRequest.Get(_VersionURL);
            bool _isSuccess = false;
            int _count = 0;
            while (
                _isSuccess == false && 
                _count < _connectTryingTime
                // && request.result != UnityWebRequest.Result.InProgress
                )
            {
                request = UnityWebRequest.Get(_VersionURL);
                var asyncOperation = request.SendWebRequest();
                //request = UnityWebRequest.Get(_VersionURL);
                while ( !asyncOperation.isDone ) 
                {
                    yield return null;
                }
                Debug.Log("Pass1");
                if (request.result != UnityWebRequest.Result.Success)
                {
                    _count += 1;
                    Debug.Log("Error code: 100");
                    request.Abort();
                    yield return new WaitForSeconds(1);
                }
                else
                {
                    _isSuccess = true;
                    string versionData = request.downloadHandler.text;
                    _versionInfo = JsonUtility.FromJson<VersionInfo>(versionData);
                    request.Abort();
                    Debug.Log("Passf");
                }
            }
            if (_isSuccess) 
            {
                onSuccess?.Invoke();
            }
            else
            {
                onFailed?.Invoke();    
            }
        }
        
    }
    [Serializable]
    public class VersionInfo
    {
        public string namex;
        public string currentVersion;
        public string latestVersion;
        public string nextVersion;
        public string nextVestionReleaseDay;
        public string stores;
        public string releaseNotes;
    }
}

