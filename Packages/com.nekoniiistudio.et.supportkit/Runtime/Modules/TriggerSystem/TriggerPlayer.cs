using UnityEngine;

namespace ET.ETEvent
{
    /// <summary>
    /// For player or similar object in world
    /// </summary>
    public class TriggerPlayer : MonoBehaviour
    {
        TriggerBase _triggerBase;

        #region Event Catcher
        private void OnTriggerEnter(Collider other)
        {
            _triggerBase = other.GetComponent<TriggerBase>();
            if (_triggerBase != null) { _triggerBase.OnPlayerEnter(this); }
        }
        private void OnTriggerExit(Collider other)
        {
            _triggerBase = other.GetComponent<TriggerBase>();
            if (_triggerBase != null) { _triggerBase.OnPlayerExit(this); }
        }
        private void OnTriggerStay(Collider other)
        {
            _triggerBase = other.GetComponent<TriggerBase>();
            if (_triggerBase != null) { _triggerBase.OnPlayerStay(this); }
        }
        #endregion
    }

}