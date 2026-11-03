using UnityEngine;
using UnityEngine.Events;

namespace ETClaimEffect
{
    public interface IUIResourceDestinationItem
    {
        float Value { get; }
        Vector3 Position { get; }
        Vector3 IconPosition { get; }
        RectTransform IconRect { get; }
        UIResourceDestinationItem SetValue(float value, bool isTrigerAction = false, bool isAnimate = false);
        UIResourceDestinationItem UpdateEvent(UnityAction<float> onValueChange);
        void ChangeValueClampToCeiling(float toValue, bool isTrigerAction = false, bool isAnimate = false);
        void ChangeValueInstantly(float toValue);
    }
}
