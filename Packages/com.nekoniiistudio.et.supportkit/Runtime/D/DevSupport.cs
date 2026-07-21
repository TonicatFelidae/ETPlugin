using UnityEngine;
namespace ET
{
    public class DevSupport : Singleton<DevSupport>
    {
        public bool isDebugOn = false;

        void Awake()
        {
            Debug.Log("[DevSupport]: isDebugOn = " + isDebugOn);
            if (isDebugOn)
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
