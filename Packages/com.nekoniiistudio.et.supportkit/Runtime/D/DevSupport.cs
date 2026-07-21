using UnityEngine;
namespace ET
{
    public class DevSupport : Singleton<DevSupport>
    {
        public bool isDebugOn = false; public bool IsDebugOn => isDebugOn;

        void Awake()
        {
            Debug.Log("[DevSupport]: isDebugOn = " + isDebugOn);
            if (IsDebugOn)
                DontDestroyOnLoad(this);
        }


        public void TurnOnDebug()
        {
            Debug.Log("[DevSupport]: TurnOnDebug");
            isDebugOn = true;
            DontDestroyOnLoad(this);
        }
        public void TurnOffDebug()
        {
            Debug.Log("[DevSupport]: TurnOffDebug");
            isDebugOn = false;
        }

    }
}
