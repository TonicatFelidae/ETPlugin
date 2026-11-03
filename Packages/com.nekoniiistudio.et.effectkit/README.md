# ET Effect Kit

tags: #unity, #effect, #plugin, #claim-effect, #ui-particles

## References
- [ET Plugin Structure](Plugin%20Structure.md)
- [ET Class Architecture](ET%20Class%20Architecture.md)

---

## Overview
**ET Effect Kit** (`com.nekoniiistudio.et.effectkit`) provides high-performance, polished UI particle effects for currency, gem, and item claim animations. It features two-phase particle physics (burst spread followed by back-eased homing trajectory to HUD destination icons), animated counter syncing, and multi-reward playback.

## Features
- **Two-Phase Particle Movement**:
  - *Phase 1 (Burst)*: Decelerating outward burst (`EaseOutCubic`) evenly distributed across a ring.
  - *Phase 2 (Fly)*: Back-easing flight paths (`InBack`, `InOutBack`, `OutBack`) with customizable overshoot.
- **Dynamic Icon Sizing**: Particles smoothly scale to match HUD destination icon dimensions on arrival.
- **Counter Synchronization**: Pure-function clamped step counter (`NextChangeValue`) preventing counter underflow.
- **Concurrent Playback**: Supports multiple simultaneous claim effects (`Play`, `PlayAdditive`) without interrupting in-flight particles.
- **Lag & Resume Spike Protection**: `ClampSimulationDelta` in `UIParticleSystem` protects against massive frame spikes after fullscreen ads or pause resumption.

## Package Structure
```text
com.nekoniiistudio.et.effectkit/
├── package.json
├── README.md
├── Runtime/
│   └── ETClaimEffect/
│       └── Script/
│           ├── ETClaimEffectMain.cs
│           ├── PopupRewardParticle.cs
│           ├── PopupRewardParticleSetting.cs
│           ├── ClaimResourceData.cs
│           ├── UIResourceDestinationItem.cs
│           ├── IUIResourceDestinationItem.cs
│           ├── SupportExtension.cs
│           ├── UIParticleSystem.cs
│           └── ETClaimEffect.asmdef
├── Editor/
│   └── ETClaimEffect/
│       └── Script/
│           ├── ETClaimEffectMainEditor.cs
│           └── ETClaimEffect.Editor.asmdef
└── Sample/
    ├── ClaimEffectScene.unity
    ├── ClaimEffectExample.cs
    ├── CurrencyRewardHandlerSample.cs
    ├── resourceAnimationSimpleAsset.png
    ├── PopIcon.anim
    ├── Res1.controller
    ├── Readme.txt
    ├── BaseRewardParticle.prefab
    ├── BaseRewardParticle_Overlay.prefab
    ├── Prefabs/
    │   └── ClaimEffectExample.prefab
    ├── Data/
    │   ├── ClaimResource_1.asset ... ClaimResource_5.asset
    │   └── RewardParticleSetting.asset
    ├── Material/
    └── Texture/
```

## Quick Start
1. Attach `ETClaimEffectMain` to a UI manager or canvas object.
2. Configure resource mappings and particle prefabs in `ETClaimEffectData`.
3. Call `ETClaimEffectMain.Play(resourceID, amount)` or `ETClaimEffectMain.PlayAdditive(...)`.
