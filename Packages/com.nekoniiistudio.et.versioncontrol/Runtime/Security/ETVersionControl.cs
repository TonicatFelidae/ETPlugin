using ET.Module.TsakaFieldSystem;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace ET.Module.VersionControlSystem
{
    public class ETVersionControl
    {
        //ETSimpleSaveLoad _etSaveLoad = new();
        private int _connectTryingSec  = 10;
        private VersionControl versionControl = new();
        private TsakaField tsakaField = null;   
        internal VersionInfo VersionInfo => versionControl.VersionInformation;
        private bool _enableDebugLog;
        private const string lightRay = "IsCutest";
        private UnityAction<VersionInfo> _onGotInfo;
        private UnityAction _onGotInfoFailed;
        private UnityAction _onPassBarrier;
        private UnityAction _onNotPassBarrier;

        /// <summary>
        /// Enable debuglog to log out data receiving, enable some events
        /// </summary>
        /// <param name="enableDebugLog"></param>
        public void Setup(
            UnityAction<VersionInfo> onGotInfo,
            UnityAction onGotInfoFailed,
            bool enableDebugLog = false,
             int connectTryingSec = 10
            ) => Setup(onGotInfo, onGotInfoFailed , null,null, enableDebugLog, connectTryingSec);
        /// <summary>
        /// Enable debuglog to log out data receiving, enable some events
        /// </summary>
        /// <param name="enableDebugLog"></param>
        public void Setup(
            UnityAction<VersionInfo> onGotInfo,
            UnityAction onGotInfoFailed,
            UnityAction onPassBarrier = null,
            UnityAction onNotPassBarrier = null,
            bool enableDebugLog = false,
            int connectTryingSec = 10)
        {
            _onGotInfo = onGotInfo; 
            _onGotInfoFailed = onGotInfoFailed;
            _enableDebugLog = enableDebugLog;
            _onPassBarrier = onPassBarrier;
            _onNotPassBarrier = onNotPassBarrier;
            _connectTryingSec = connectTryingSec;   
        }
        private ETMagicLightPoint Body
        {
            get
            {
                if(_body == null)
                {
                    GameObject go = new GameObject("ETMagicLightPoint");
                    _body = go.AddComponent<ETMagicLightPoint>();
                    _body.lightPoint = lightRay;
                    Debug.Log("Pass");
                    _body.StartCoroutine(
                        versionControl.GetVersionInfo(() => CastShield(go)
                        , () => CastShield(go)));
                }
                return _body;   
            }
        }
        private void CastShield(GameObject go)
        {
            tsakaField = new();
            tsakaField.Cast(go, null, _onPassBarrier, _onNotPassBarrier, 10);
        }
        private ETMagicLightPoint _body;
        /// <summary>
        /// Get VersionInfo and use it in game
        /// </summary>
        /// <param name="onGotInfo"></param>
        /// <param name="onFailedGotInfo"></param>
        public void RunVersionInfoProtocol()
        {
            Body.StartCoroutine(RunCheckVersionInfoProtocolCorontire());
        }
        /// <summary>
        /// This protocol is for get info that already load in game, increase security
        /// This protocol DO NOT request from html
        /// This protocol DO NOT SAVE FILE
        /// If cant not get new version info it will load old version info
        /// </summary>
        /// <param name="onGotInfo"></param>
        /// <param name="onFailedGotInfo"></param>
        /// <returns></returns>
        private IEnumerator RunCheckVersionInfoProtocolCorontire()

        {
            int countTry = 0;
            while (VersionInfo == null && countTry < _connectTryingSec)
            {
                countTry += 1;
                yield return new WaitForSeconds(1);
            }
            if (VersionInfo != null)
            {
                if(_enableDebugLog)
                {
                    Debug.Log(
                        $"///VersionControlLog///" + "\n" +
                        $"----UNITY----" + "\n" +
                        $"currentVersion {Application.version}" + "\n" +
                        $"-----LOG-----" + "\n" +
                        $"currentVersion {VersionInfo.currentVersion}" + "\n" +
                        $"nextVersion {VersionInfo.nextVersion}" + "\n" +
                        $"nextVestionReleaseDay {VersionInfo.nextVestionReleaseDay}" + "\n" +
                        $"namex {VersionInfo.namex}" + "\n" +
                        $"stores {VersionInfo.stores}" + "\n" +
                        $"releaseNotes {VersionInfo.releaseNotes}" + "\n" +
                        $"-----END-----");

                }
                _onGotInfo?.Invoke(VersionInfo); 
            }
            else
            {
                _onGotInfoFailed?.Invoke();
            }
        }
    }
}