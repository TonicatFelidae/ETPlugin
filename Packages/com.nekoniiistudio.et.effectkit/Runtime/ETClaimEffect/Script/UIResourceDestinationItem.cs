using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace ETClaimEffect
{
    public class UIResourceDestinationItem : MonoBehaviour, IUIResourceDestinationItem
    {
        public int destinationID;
        public Image icon;
        public TextMeshProUGUI tx_amount = null;
        float _value;
        public float Value => _value;
        // Opt-in live upper bound for the ANIMATED relative write (ChangeValueInstantly). Null =>
        // unchanged behavior. Wired from game code (CurrencyRewardHandler) to the authoritative
        // balance so the reward count-up can never transiently exceed it under overlapping flies.
        public Func<float> CeilingProvider;
        private Animator _animator = null;
        public Vector3 Position => gameObject.transform.position;
        public UnityAction<float> OnValueChange { get; set; }
        public Vector3 IconPosition => icon != null ? icon.transform.position : transform.position;
        public RectTransform IconRect => icon != null ? icon.rectTransform : null;
        public Animator animator
        {
            get
            {
                if (_animator == null) _animator = GetComponent<Animator>();
                return _animator;
            }
        }
        public void Setup(float value, UnityAction<float> onValueChange)
        {
            SetValue(value);
            this.OnValueChange = onValueChange;
        }
        public UIResourceDestinationItem SetValue(
            float value,
            bool isTrigerAction = false,
            bool isAnimate = false
            )
        {
            if (tx_amount != null)
            {
                Debug.Log($"[CurrencyDebug] UIResourceDestinationItem.SetValue: {tx_amount.name} old='{tx_amount.text}' new='{value}'");
                tx_amount.text = value.ToString();
            }
            _value = value;
            if (isTrigerAction)
            {
                OnValueChange?.Invoke(_value);
            }
            if (isAnimate)
            {
                AnimateIcon();
            }
            return this;
        }
        /// <summary>
        /// Bring the internal counter in line with an authoritative value WITHOUT touching the
        /// text. For HUDs where another component owns the label (a bound currency item that
        /// tweens it), this keeps the base of the next relative write correct while leaving the
        /// visible number to that owner — an instant write here would fight its tween.
        /// </summary>
        public UIResourceDestinationItem SyncValue(float value)
        {
            _value = value;
            return this;
        }
        public UIResourceDestinationItem UpdateEvent(
            UnityAction<float> onValueChange)
        {
            this.OnValueChange = onValueChange;
            return this;
        }
        /// <summary>
        /// Upper-clamp for the ANIMATED relative write: a per-particle arrival may never push the
        /// displayed counter above the current authoritative balance (the live ceiling). Pure and
        /// static so the overlap invariant is unit-testable without a MonoBehaviour (see
        /// CurrencyHudOvershootClampTests). Upper-bound ONLY — the values fed here are already >= 0
        /// and a floor would mask real state; the spec is a ceiling.
        /// </summary>
        public static float ClampToCeiling(float proposedValue, float ceiling) => Mathf.Min(proposedValue, ceiling);

        public void ChangeValueClampToCeiling(float toValue,
            bool isTrigerAction = false,
            bool isAnimate = false)
        {
            // Live ceiling (opt-in): the reward count-up is a RELATIVE += per particle arrival and
            // many flies overlap during a swipe — a late arrival must never push the shared counter
            // above the authoritative balance (the 19+8 -> 28 -> 27 overshoot). Absolute SetValue
            // snaps stay unclamped. Read live per arrival: the balance climbs mid-swipe, so a
            // snapshot ceiling would undershoot. Placed above the change-guard so an over-ceiling
            // arrival collapses to a no-op (no spurious PopIcon / OnValueChange).
            if (CeilingProvider != null) toValue = ClampToCeiling(toValue, CeilingProvider());
            float current = _value;
            if (current != toValue)
            {
                if (tx_amount != null) tx_amount.text = toValue.ToString();
                _value = toValue;
            }
            if (isTrigerAction)
            {
                OnValueChange?.Invoke(_value);
            }
            if (isAnimate)
            {
                AnimateIcon();
            }
        }

        public void ChangeValueInstantly(float toValue)
        {
            ChangeValueClampToCeiling(toValue, true, true);
        }

        private void AnimateIcon()
        {
            animator.Play("PopIcon");
        }
    }
}
