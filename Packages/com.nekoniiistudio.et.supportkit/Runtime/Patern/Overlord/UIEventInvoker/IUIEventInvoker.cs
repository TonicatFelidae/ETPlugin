using UnityEngine.Events;

namespace ET.UIKit
{
    public interface IUIEventInvoker
    {
        public UnityEvent<UIEvent> OnEventInvoker { get; set; }
    }
    public enum UIEvent
    {
        SizeChange,
        Rotation,
        Flip,
        Disable,
        Enable,
    }
}
