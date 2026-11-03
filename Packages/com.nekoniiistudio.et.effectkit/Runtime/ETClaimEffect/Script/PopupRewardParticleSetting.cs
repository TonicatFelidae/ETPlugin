using UnityEngine;

namespace ETClaimEffect
{
    public enum FlyEase { InBack, InOutBack, OutBack }

    [CreateAssetMenu(fileName = "RewardParticleSetting", menuName = "Data/RewardParticleSetting")]
    public class PopupRewardParticleSetting : ScriptableObject
    {
        [Tooltip("Initial scatter of the burst (ParticleSystem shape radius)")]
        public float emissionRadius = 0.2f;

        [Header("Phase 1 - burst")]
        [Tooltip("How long the pop outward takes. Every particle bursts together, so this reads as one explosion")]
        public float burstDuration = 0.286f;

        [Tooltip("Burst spread, in canvas units (min)")]
        public float burstDistanceMin = 24f;

        [Tooltip("Burst spread, in canvas units (max)")]
        public float burstDistanceMax = 64f;

        [Tooltip("Delay between two consecutive emits during the burst (0 = all pop together). Capped by maxTotalDelay")]
        public float burstPerParticleDelay = 0.02f;

        [Tooltip("Spread the burst evenly around the circle instead of random angles")]
        public bool burstEvenlySpaced = true;

        [Tooltip("Beat the particles hang at the burst point before leaving, so the pop registers")]
        public float burstHoldDuration = 0.05f;

        [Header("Phase 2 - fly to destination")]
        [Tooltip("How long a single particle takes to reach the HUD icon")]
        public float flyDuration = 0.585f;

        [Tooltip("InBack: pull back then dive straight into the icon. InOutBack also overshoots past the icon")]
        public FlyEase flyEase = FlyEase.InBack;

        [Tooltip("Back-ease overshoot ('s'). 1.70158 matches DOTween's default")]
        public float backOvershoot = 1.70158f;

        [Tooltip("Stagger between two consecutive particles — this is what makes the counter tick up")]
        public float perParticleDelay = 0.012f;

        [Tooltip("Hard cap on the total stagger, so a 100-particle reward never drags on")]
        public float maxTotalDelay = 0.3f;

        [Header("Size")]
        [Tooltip("Drive size from code (size-over-lifetime is disabled): 0 → startSize on burst, then → the HUD icon's on-screen size on arrival")]
        public bool matchTargetIconSize = true;

        [Tooltip("Burst size as a multiple of the icon size, rolled per particle (arrival is always 1.0)")]
        public float burstScaleMin = 0.9f;
        public float burstScaleMax = 1.2f;
    }
}
