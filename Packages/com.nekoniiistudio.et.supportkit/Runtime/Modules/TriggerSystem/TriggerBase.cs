using UnityEngine;

namespace ET.ETEvent
{
    /// <summary>
    /// For other obejct than player or similar object in world
    /// </summary>
    public class TriggerBase : MonoBehaviour
    {
        public virtual void OnPlayerEnter(TriggerPlayer vehicleBase)
        {
        }
        public virtual void OnPlayerStay(TriggerPlayer vehicleBase)
        {
        }
        public virtual void OnPlayerExit(TriggerPlayer vehicleBase)
        {
        }
    }
}
