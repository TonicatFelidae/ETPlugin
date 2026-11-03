using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI.Extensions.CasualGame;

namespace ETClaimEffect
{
	/// <summary>
	/// Drives the reward particles in two explicit phases:
	///   Phase 1 (burst)  — particles pop outward from the spawn point onto a ring (EaseOutCubic),
	///                      optionally one after another (burstPerParticleDelay).
	///   Phase 2 (fly)    — after a short hold, particles leave one by one (that stagger is what
	///                      ticks the counter up) and tween from the burst point to the HUD icon
	///                      on an InOutBack curve — overshoot past the icon, settle.
	/// With matchTargetIconSize the size is driven here as well: 0 → prefab startSize during the
	/// burst, then → the icon's on-screen size so the last frame lands exactly on the HUD icon.
	/// Positions are written per frame, so the ParticleSystem's own integration must not add
	/// anything (start speed 0, velocity zeroed, no force/velocity modules on the prefab).
	/// </summary>
	public class PopupRewardParticle : MonoBehaviour
	{
		[SerializeField] private ParticleSystem _particleSystem;
		[SerializeField]
		[FormerlySerializedAs("targetPosition")]
		private Transform _targetTransform;

		public const int MAX_PARTICLES = 100;

		/// <summary>Safety margin so a stalled frame can never strand the onComplete callback.</summary>
		private const float WatchdogGrace = 1f;

		private PopupRewardParticleSetting _setting;
		private ParticleSystem.Particle[] _particles;
		private int _particleCount;
		private int _arrivedCount;
		private int _valuePerParticle;
		private int _maxValue;
		private int _destinationID;
		private ETClaimEffectMain _main;
		private UnityAction _onComplete;
		private UnityAction _onFirstArrival;
		private bool _firstArrivalInvoked;
		private Transform _targetIcon;

		// Per-particle flight data, indexed the same as _particles. Arrived particles are PARKED
		// (size 0) rather than killed: GetParticles() swap-removes dead particles, which would
		// shift every index after the hole and hand the wrong flight path to the survivors.
		private Vector3[] _origins;
		private Vector3[] _burstPoints;
		private float[] _burstDelays;
		private float[] _startDelays;
		private float[] _burstSizes;
		private float[] _burstScales;
		private bool[] _arrived;
		private float _elapsed;
		private float _totalDuration;

		private bool DriveSize => _setting != null && _setting.matchTargetIconSize;

		public void Init(ETClaimEffectMain main, PopupRewardParticleSetting setting)
		{
			_main = main;
			_setting = setting;
			_particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			var mainModule = _particleSystem.main;
			// We own the positions from here on — any emission speed would fight the tween.
			mainModule.startSpeed = 0f;
			var shape = _particleSystem.shape;
			shape.radius = _setting.emissionRadius;
			if (DriveSize)
			{
				var sizeOverLifetime = _particleSystem.sizeOverLifetime;
				sizeOverLifetime.enabled = false;
			}
		}

		public void StartAnim(
			Vector3 destinationPosition, int destinationID, int particleAmount,
			Material mat, int order, int maxValue, int resourceType, UnityAction onCompletedAction,
			UnityAction onFirstArrivalAction,
			int coinCost,
			Transform targetIcon = null)
		{
			_destinationID = destinationID;
			_targetIcon = targetIcon;

			if (!gameObject.activeSelf) gameObject.SetActive(true);

			var psRenderer = GetComponent<ParticleSystemRenderer>();
			if (psRenderer)
			{
				if (mat != null) psRenderer.material = mat;
				psRenderer.sortingOrder = order;
			}
			var uiPS = GetComponent<UIParticleSystem>();
			if (uiPS)
			{
				if (mat != null) uiPS.material = mat;
			}

			// Cap particle count to MAX_PARTICLES for performance, each particle represents multiple coins
			int cappedParticleAmount = Mathf.Min(particleAmount, MAX_PARTICLES);
			int coinsPerParticle = Mathf.CeilToInt((float)particleAmount / cappedParticleAmount);

			_particleCount = cappedParticleAmount;
			_arrivedCount = 0;
			_maxValue = maxValue;
			_onComplete = onCompletedAction;
			_onFirstArrival = onFirstArrivalAction;
			_firstArrivalInvoked = false;
			_valuePerParticle = coinsPerParticle * coinCost;
			PlayAnimationAtPosition(destinationPosition);
		}

		/// <summary>
		/// Per-arrival HUD delta: how much this particle adds to the counter, and the remaining
		/// pool (<paramref name="maxValue"/>) after it. Extracted as a pure function so the value
		/// accounting is unit-testable independently of particle timing
		/// (see RewardParticleAccountingTests). Invariant: summing the return over all arrivals
		/// must equal the reward amount — it must never feed a negative delta into the HUD.
		/// </summary>
		public static int NextChangeValue(ref int maxValue, int valuePerParticle)
		{
			// Clamp so a particle never delivers a negative delta and the pool never drops below 0:
			// once the pool empties, remaining arrivals deliver 0 (was: negative underflow into the
			// unclamped HUD writer, pulling the counter hundreds below the true balance).
			int changeValue = Mathf.Clamp(maxValue, 0, valuePerParticle);
			maxValue = Mathf.Max(0, maxValue - valuePerParticle);
			return changeValue;
		}

		/// <summary>Per-particle stagger, capped so the total never exceeds maxTotal.</summary>
		public static float Stagger(float perParticle, int particleCount, float maxTotal)
		{
			if (particleCount <= 1 || perParticle <= 0f) return 0f;
			return Mathf.Min(perParticle, maxTotal / (particleCount - 1));
		}

		/// <summary>
		/// ParticleSystem startSize that renders as wide as the icon: UIParticleSystem builds a
		/// quad of width GetCurrentSize in the system's local space, so divide by its scale.
		/// </summary>
		public static float TargetParticleSize(float iconWorldWidth, float systemLossyScale)
		{
			return systemLossyScale > 0f ? iconWorldWidth / systemLossyScale : iconWorldWidth;
		}

		/// <summary>Decelerating spread for phase 1.</summary>
		public static float EaseOutCubic(float t)
		{
			t = Mathf.Clamp01(t);
			float inv = 1f - t;
			return 1f - inv * inv * inv;
		}

		/// <summary>Penner InBack: anticipation pull-back, then accelerate into the target. No end overshoot.</summary>
		public static float EaseInBack(float t, float overshoot)
		{
			return t * t * ((overshoot + 1f) * t - overshoot);
		}

		public static float EaseOutBack(float t, float overshoot)
		{
			t -= 1f;
			return t * t * ((overshoot + 1f) * t + overshoot) + 1f;
		}

		public static float FlyEaseValue(FlyEase ease, float t, float overshoot)
		{
			switch (ease)
			{
				case FlyEase.InBack: return EaseInBack(t, overshoot);
				case FlyEase.OutBack: return EaseOutBack(t, overshoot);
				default: return EaseInOutBack(t, overshoot);
			}
		}

		/// <summary>
		/// Penner InOutBack — the same curve as DOTween's Ease.InOutBack. Deliberately returns
		/// values outside [0,1] (that overshoot IS the flourish), so callers must interpolate
		/// with LerpUnclamped.
		/// </summary>
		public static float EaseInOutBack(float t, float overshoot)
		{
			float s = overshoot * 1.525f;
			t *= 2f;
			if (t < 1f) return 0.5f * (t * t * ((s + 1f) * t - s));
			t -= 2f;
			return 0.5f * (t * t * ((s + 1f) * t + s) + 2f);
		}

		private void PlayAnimationAtPosition(Vector3 endPos)
		{
			_targetTransform.position = endPos;
			PlayAnimation_Init();
		}

		private void PlayAnimation_Init()
		{
			var main = _particleSystem.main;
			_particles = new ParticleSystem.Particle[_particleCount];
			main.maxParticles = _particleCount;

			int lastIndex = Mathf.Max(0, _particleCount - 1);
			float burstStagger = Stagger(_setting.burstPerParticleDelay, _particleCount, _setting.maxTotalDelay);
			float flyStagger = Stagger(_setting.perParticleDelay, _particleCount, _setting.maxTotalDelay);
			_elapsed = 0f;
			_totalDuration = burstStagger * lastIndex + _setting.burstDuration + _setting.burstHoldDuration
				+ flyStagger * lastIndex + _setting.flyDuration;
			// Color-over-lifetime is authored across the whole life, so the lifetime has to match
			// the flight. The tail keeps particles alive while parked at the destination.
			main.startLifetime = _totalDuration + WatchdogGrace;

			_particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			_particleSystem.Emit(_particleCount);
			_particleSystem.Simulate(0f, true, false);

			int emitted = _particleSystem.GetParticles(_particles);
			_origins = new Vector3[_particleCount];
			_burstPoints = new Vector3[_particleCount];
			_burstDelays = new float[_particleCount];
			_startDelays = new float[_particleCount];
			_burstSizes = new float[_particleCount];
			_burstScales = new float[_particleCount];
			_arrived = new bool[_particleCount];

			float angleOffset = Random.Range(0f, Mathf.PI * 2f);
			for (int i = 0; i < emitted; i++)
			{
				Vector3 origin = ToWorldSpace(_particles[i].position);
				float angle = _setting.burstEvenlySpaced
					? angleOffset + i * (Mathf.PI * 2f / emitted)
					: Random.Range(0f, Mathf.PI * 2f);
				float distance = Random.Range(_setting.burstDistanceMin, _setting.burstDistanceMax);
				_origins[i] = origin;
				_burstPoints[i] = origin + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * distance;
				_burstDelays[i] = i * burstStagger;
				_startDelays[i] = i * flyStagger;
				_burstSizes[i] = _particles[i].startSize;
				_burstScales[i] = Random.Range(_setting.burstScaleMin, _setting.burstScaleMax);
				_particles[i].velocity = Vector3.zero;
				if (DriveSize) _particles[i].startSize = 0f;
			}
			// A capped emit (maxParticles) can hand back fewer particles than requested — count the
			// missing ones as already arrived so the completion check can still be reached.
			_arrivedCount = _particleCount - emitted;
			if (emitted > 0) _particleSystem.SetParticles(_particles, emitted);

			StartCoroutine(PlayAnimation());
		}

		private IEnumerator PlayAnimation()
		{
			while (_arrivedCount < _particleCount)
			{
				int particleLength = _particleSystem.GetParticles(_particles);

				// Safety: if the particles expired without arriving, break out
				if (particleLength == 0)
					break;

				// Unscaled: UIParticleSystem simulates on unscaled delta, and rewards play over
				// loading pages/popups that may have parked timeScale.
				_elapsed += Time.unscaledDeltaTime;
				Vector3 target = _targetTransform.position;
				bool driveSize = DriveSize;
				float iconSize = driveSize ? IconParticleSize() : -1f;

				int count = Mathf.Min(particleLength, _particleCount);
				for (int i = 0; i < count; i++)
				{
					if (_arrived[i])
					{
						// Arrived last frame at icon size; park it now.
						if (driveSize) _particles[i].startSize = 0f;
						continue;
					}

					float t = _elapsed - _burstDelays[i];
					float endSize = iconSize >= 0f ? iconSize : _burstSizes[i];
					float burstSize = driveSize ? endSize * _burstScales[i] : _burstSizes[i];
					Vector3 worldPos;
					float size;
					if (t <= 0f)
					{
						worldPos = _origins[i];
						size = 0f;
					}
					else if (t < _setting.burstDuration)
					{
						float e = EaseOutCubic(t / _setting.burstDuration);
						worldPos = Vector3.Lerp(_origins[i], _burstPoints[i], e);
						size = burstSize * e;
					}
					else
					{
						float flightTime = t - _setting.burstDuration
							- _setting.burstHoldDuration - _startDelays[i];
						if (flightTime <= 0f)
						{
							worldPos = _burstPoints[i];
							size = burstSize;
						}
						else
						{
							float f = _setting.flyDuration > 0f
								? flightTime / _setting.flyDuration
								: 1f;
							if (f >= 1f)
							{
								Arrive(i, target, endSize);
								continue;
							}
							// LerpUnclamped: back eases leave [0,1] on purpose.
							worldPos = Vector3.LerpUnclamped(_burstPoints[i], target,
								FlyEaseValue(_setting.flyEase, f, _setting.backOvershoot));
							size = Mathf.Lerp(burstSize, endSize, f);
						}
					}

					_particles[i].position = ToSimulationSpace(worldPos);
					_particles[i].velocity = Vector3.zero;
					if (driveSize) _particles[i].startSize = size;
				}
				_particleSystem.SetParticles(_particles, particleLength);

				// The completion callback gates an awaited loading page (HomeScreen/GachaScreen),
				// so it must fire even if a particle somehow never lands.
				if (_elapsed > _totalDuration + WatchdogGrace) break;

				yield return null;
			}
			DestroyEffect();
			yield return null;
		}

		private float IconParticleSize()
		{
			if (_targetIcon == null) return -1f;
			float iconWorldWidth = _targetIcon is RectTransform rt
				? rt.rect.width * rt.lossyScale.x
				: _targetIcon.lossyScale.x;
			return TargetParticleSize(iconWorldWidth, _particleSystem.transform.lossyScale.x);
		}

		private void Arrive(int index, Vector3 target, float endSize)
		{
			_arrived[index] = true;
			_arrivedCount++;
			// Land at the icon's size for this frame; the loop parks it (size 0) next frame.
			// Never kill — see the _origins comment: a death compacts the array.
			_particles[index].position = ToSimulationSpace(target);
			_particles[index].velocity = Vector3.zero;
			_particles[index].startSize = DriveSize ? endSize : 0f;

			int changeValue = NextChangeValue(ref _maxValue, _valuePerParticle);
			_main.ChangeValueInstantly(_destinationID, changeValue);

			if (!_firstArrivalInvoked)
			{
				_firstArrivalInvoked = true;
				_onFirstArrival?.Invoke();
			}
		}

		private Vector3 ToWorldSpace(Vector3 simulationPosition) =>
			_particleSystem.main.simulationSpace == ParticleSystemSimulationSpace.Local
				? _particleSystem.transform.TransformPoint(simulationPosition)
				: simulationPosition;

		private Vector3 ToSimulationSpace(Vector3 worldPosition) =>
			_particleSystem.main.simulationSpace == ParticleSystemSimulationSpace.Local
				? _particleSystem.transform.InverseTransformPoint(worldPosition)
				: worldPosition;

		public void DestroyEffect()
		{
			_onComplete?.Invoke();
			Destroy(gameObject);
		}
	}
}
