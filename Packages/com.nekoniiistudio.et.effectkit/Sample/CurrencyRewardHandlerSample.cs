using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ETClaimEffect
{
    #region Standalone Mock Classes
    /// <summary>
    /// Standalone mock for Economy service.
    /// </summary>
    public class EconomySP
    {
        public int Coins { get; set; }
        public int Tickets { get; set; }
    }

    /// <summary>
    /// Standalone mock for reward visual event.
    /// </summary>
    public class CurrencyRewardVisualEvent
    {
        public int CoinReward { get; set; }
        public int TicketReward { get; set; }
        public bool HasWorldPosition { get; set; }
        public Vector3 WorldPosition { get; set; }
        public System.Action OnCompleted { get; set; }
    }

    /// <summary>
    /// Standalone mock for planning currency animation steps.
    /// </summary>
    public static class CurrencyRewardVisualPlanner
    {
        public const int CoinsDestinationId = 0;
        public const int TicketsDestinationId = 1;

        public struct AnimationStep
        {
            public int destinationId;
            public int startValue;
            public int amount;
        }

        public static List<AnimationStep> Plan(int preCoins, int preTickets, int shownCoins, int shownTickets)
        {
            var steps = new List<AnimationStep>();
            if (shownCoins > 0)
            {
                steps.Add(new AnimationStep
                {
                    destinationId = CoinsDestinationId,
                    startValue = preCoins,
                    amount = shownCoins
                });
            }
            if (shownTickets > 0)
            {
                steps.Add(new AnimationStep
                {
                    destinationId = TicketsDestinationId,
                    startValue = preTickets,
                    amount = shownTickets
                });
            }
            return steps;
        }
    }

    /// <summary>
    /// Standalone mock for SoundGeneral.
    /// </summary>
    public class SoundGeneral : MonoBehaviour
    {
        private static SoundGeneral _instance;
        public static SoundGeneral Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SoundGeneral>();
                    if (_instance == null)
                    {
                        var go = new GameObject("SoundGeneralSample");
                        _instance = go.AddComponent<SoundGeneral>();
                    }
                }
                return _instance;
            }
        }

        public void PlayEF3D(int soundId)
        {
            Debug.Log($"[SoundGeneralSample] Play sound effect with ID: {soundId}");
        }
    }
    #endregion

    /// <summary>
    /// Standalone sample implementation of CurrencyRewardHandler.
    /// </summary>
    public class CurrencyRewardHandlerSample
    {
        private readonly ETClaimEffectMain _claimEffect;
        private readonly EconomySP _economyService;

        public CurrencyRewardHandlerSample(ETClaimEffectMain claimEffect, EconomySP economyService)
        {
            _claimEffect = claimEffect;
            _economyService = economyService;
            // Live upper-ceiling: an overlapping late +1 arrival can never push the animated counter
            // above the authoritative balance (read live per arrival — the balance climbs during a
            // swipe, so a snapshot would undershoot). Absolute SetValue snaps stay unclamped, so the
            // completion snap still lands the final value exactly on the balance.
            _claimEffect.SetCeilingProvider(CurrencyRewardVisualPlanner.CoinsDestinationId, () => _economyService.Coins);
            _claimEffect.SetCeilingProvider(CurrencyRewardVisualPlanner.TicketsDestinationId, () => _economyService.Tickets);
        }

        public void PlayClaimEffect(
            int rewardType,
            int amount,
            Vector3? customStartPosition = null,
            Action onCompleted = null)
        {
            UnityAction onCompletedAction = onCompleted != null ? new UnityAction(onCompleted) : null;

            if (customStartPosition.HasValue)
            {
                _claimEffect.Play(
                    (int)rewardType,
                    amount,
                    customStartPosition.Value,
                    (int)rewardType,
                    onCompletedAction,
                    null);
            }
            else if (onCompletedAction != null)
            {
                _claimEffect.Play(
                    (int)rewardType,
                    amount,
                    _claimEffect.effectStartLocation.position,
                    (int)rewardType,
                    onCompletedAction,
                    null);
            }
            else
            {
                _claimEffect.Play((int)rewardType, amount);
            }

            SoundGeneral.Instance.PlayEF3D(8);
        }

        /// <summary>
        /// Handles the CurrencyRewardVisualEvent by planning animation steps and playing them.
        /// </summary>
        /// <param name="evt">The reward visual event.</param>
        /// <param name="setInitialValues">
        /// If true, sets the display values for coins/tickets before the animation loop.
        /// MainScreen uses true (to sync display before animation);
        /// PlayScreen uses false (values are set per-step).
        /// </param>
        /// <summary>
        /// Balance must already be applied to EconomySP before calling this method.
        /// The handler computes pre-reward values by subtracting the reward from the current balance.
        /// </summary>
        public void OnCurrencyRewardVisual(CurrencyRewardVisualEvent evt, bool setInitialValues)
        {
            // Clamp the ANIMATED reward to the authoritative balance so the counter can
            // never end ABOVE _economyService.Coins/Tickets. Balance is applied before this
            // fires, so the display end value = pre + shown = balance. Two guards:
            //   - lower: Max(0, …) — the raw start must not go negative (93 - 150 = -57 leaked
            //     onto the HUD; bug No.7).
            //   - upper: Min(reward, balance) — when the fired reward exceeds the applied delta
            //     (e.g. collect-drops clamped the ticket delta to pending, or an optimistic
            //     AddTickets hasn't been corrected yet), the display would otherwise land on the
            //     reward and over-count. PlayScreen never re-syncs down, so it would persist.
            int shownCoins = Mathf.Clamp(evt.CoinReward, 0, Mathf.Max(0, _economyService.Coins));
            int shownTickets = Mathf.Clamp(evt.TicketReward, 0, Mathf.Max(0, _economyService.Tickets));
            // The clamp already guarantees pre >= 0 while EconomySP's setters clamp the
            // balance at 0 — the local floor keeps that invariant even if the setter
            // clamp is ever relaxed (defense in depth, not reachable today).
            int preCoins = Mathf.Max(0, _economyService.Coins - shownCoins);
            int preTickets = Mathf.Max(0, _economyService.Tickets - shownTickets);

            if (setInitialValues)
            {
                _claimEffect.SetValue(CurrencyRewardVisualPlanner.CoinsDestinationId, preCoins);
                _claimEffect.SetValue(CurrencyRewardVisualPlanner.TicketsDestinationId, preTickets);
            }

            var steps = CurrencyRewardVisualPlanner.Plan(
                preCoins, preTickets, shownCoins, shownTickets
            );

            Vector3? claimStartPos = null;
            if (evt.HasWorldPosition && Camera.main != null)
            {
                claimStartPos = Camera.main.WorldToScreenPoint(evt.WorldPosition);
            }

            Vector3 startPos = claimStartPos ?? _claimEffect.effectStartLocation.position;

            int activeAnimations = steps.Count;
            if (activeAnimations == 0)
            {
                evt.OnCompleted?.Invoke();
                return;
            }

            foreach (var step in steps)
            {
                int destId = step.destinationId;
                _claimEffect.SetValue(destId, step.startValue);
                // On completion, snap the counter to the authoritative balance. The balance is
                // already applied (see summary above), so this corrects any residual left by the
                // particle animation — over/under-delivery when not all particles arrive, or the
                // legacy >100 underflow — instead of resting on a wrong value until the next
                // OnCurrencyChangedEvent.
                _claimEffect.PlayAdditive(destId, step.amount, startPos, destId,
                    () => {
                        _claimEffect.SetValue(
                            destId,
                            destId == CurrencyRewardVisualPlanner.CoinsDestinationId
                                ? _economyService.Coins
                                : _economyService.Tickets);
                        activeAnimations--;
                        if (activeAnimations == 0)
                        {
                            evt.OnCompleted?.Invoke();
                        }
                    });
                SoundGeneral.Instance.PlayEF3D(8);
            }
        }

        public void UpdateEconomyDisplay()
        {
            _claimEffect.SetValue(0, _economyService.Coins);
            _claimEffect.SetValue(1, _economyService.Tickets);
        }
    }
}
