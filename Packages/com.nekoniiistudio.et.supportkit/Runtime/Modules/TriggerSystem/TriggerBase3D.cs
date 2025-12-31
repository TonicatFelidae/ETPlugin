using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace ET.ETEvent
{
    /// <summary>
    /// Obsolete code
    /// </summary>

    public class TriggerBase3D : MonoBehaviour
    {
        protected IEventManager _eventManager;
        /// <summary>
        /// Assign IEventManager and register
        /// </summary>
        /// <param name="eventManager"></param>
        public void Init(IEventManager eventManager)
        {
            _eventManager = eventManager;
            //_eventManager.Register(this);
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                OnPlayerEnter(this.gameObject);
            }
        }
        private void OnTriggerStay(Collider other)
        {
            if (other.tag == "Player")
            {
                OnPlayerStay(this.gameObject);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            
        }
        public virtual void OnPlayerEnter(GameObject player)
        {
        }
        public virtual void OnPlayerStay(GameObject player)
        {
        }
    }
    public interface IEventManager
    {
        void Register(TriggerBase3D triggerBase);
        UnityEvent EventInvoke(TriggerBase3D triggerBase);
    }
}
