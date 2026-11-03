using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ETClaimEffect
{
    public class ETClaimEffectMain : MonoBehaviour
    {
        public bool debugOn;
        public ETClaimEffectData data;
        public Transform effectStartLocation;
        private UnityAction _onCompletedCallback;
        private UnityAction _onFirstArrivalCallback;

        //public void ClaimResourceWithProtoccol(int protocolID)
        //{
        //    if (protocolID >= 0) // normal
        //    {
        //        ProtocolData protocol = protocolDatas.Find(x => x.protocolID == protocolID);
        //        _popupReward.Play(protocol.data);
        //    }
        //    else // claim all
        //    {
        //        ResourceData[] mergedArray = new ResourceData[0];
        //        foreach (var item in protocolDatas)
        //        {
        //            mergedArray = mergedArray.Concat(item.data).ToArray();
        //        }
        //
        //        _popupReward.Play(mergedArray);
        //    }
        //}

        private Dictionary<int, UIResourceDestinationItem> _destinationCache;

        public void ReInit()
        {
            _destinationCache = null;
        }

        public void RegisterDestination(int destinationID, UIResourceDestinationItem destinationItem)
        {
            if (_destinationCache == null)
            {
                _destinationCache = new Dictionary<int, UIResourceDestinationItem>();
            }
            _destinationCache[destinationID] = destinationItem;
        }

        public void Setup(int destinationID, int value, UnityAction<float> onValueChange)
        {
            var destinationItem = GetDestinationByID(destinationID);
            destinationItem.Setup(value, onValueChange);
        }
        /// <summary>
        /// Set value without triger anything, set start value for play
        /// </summary>
        public void SetValue(int destinationID, int value)
        {
            var destinationItem = GetDestinationByID(destinationID);
            destinationItem.SetValue(value);
        }
        public void ChangeValueInstantly(int destinationID, int value)
        {
            var destinationItem = GetDestinationByID(destinationID);
            destinationItem.ChangeValueInstantly(destinationItem.Value + value);
        }
        /// <summary>
        /// Wire a LIVE upper bound onto a destination's animated relative write, so the reward
        /// count-up (ChangeValueInstantly) can never transiently exceed the authoritative balance
        /// under overlapping flies. Provider is read live per particle arrival.
        /// </summary>
        public void SetCeilingProvider(int destinationID, System.Func<float> provider)
        {
            if (data == null || data.destinationDatas == null) return;
            var destinationItem = GetDestinationByID(destinationID);
            if (destinationItem == null) return;
            destinationItem.CeilingProvider = provider;
        }
        public Vector3 GetDestinationPosition(int resourceID)
        {
            var dest = GetDestinationByResourceID(resourceID);
            return dest.Position;
        }

        private UIResourceDestinationItem GetDestinationByResourceID(int resourceID)
        {
            if (data != null && data.destinationDatas != null)
            {
                for (int i = 0; i < data.destinationDatas.Length; i++)
                {
                    if (data.destinationDatas[i].resourceID == resourceID)
                    {
                        return GetDestinationByID(data.destinationDatas[i].destinationID);
                    }
                }
            }
            return GetDestinationByID(resourceID);
        }

        private UIResourceDestinationItem GetDestinationByID(int ID)
        {
            if (_destinationCache != null && _destinationCache.TryGetValue(ID, out var destination) && destination != null)
            {
                return destination;
            }

            if (data != null && data.destinationDatas != null)
            {
                for (int i = 0; i < data.destinationDatas.Length; i++)
                {
                    if (data.destinationDatas[i].destinationID == ID && data.destinationDatas[i].destinationItem != null)
                    {
                        return data.destinationDatas[i].destinationItem;
                    }
                }
            }

            if (_destinationCache == null)
            {
                _destinationCache = new Dictionary<int, UIResourceDestinationItem>();
            }
            var items = FindObjectsOfType<UIResourceDestinationItem>(true);
            foreach (var item in items)
            {
                if (item != null && !_destinationCache.ContainsKey(item.destinationID))
                {
                    _destinationCache.Add(item.destinationID, item);
                }
            }

            if (_destinationCache.TryGetValue(ID, out destination) && destination != null)
            {
                return destination;
            }

            // Fallback / Refresh: search again
            _destinationCache.Clear();
            var refreshedItems = FindObjectsOfType<UIResourceDestinationItem>(true);
            foreach (var item in refreshedItems)
            {
                if (item != null && !_destinationCache.ContainsKey(item.destinationID))
                {
                    _destinationCache.Add(item.destinationID, item);
                }
            }

            if (_destinationCache.TryGetValue(ID, out destination) && destination != null)
            {
                return destination;
            }

            return null;
        }

        private ClaimResourceData GetClaimResourceData(int ID)
        {
            if (data != null && data.resourceDatas != null)
            {
                for (int i = 0; i < data.resourceDatas.Length; i++)
                {
                    if (data.resourceDatas[i] != null && data.resourceDatas[i].resourceID == ID)
                    {
                        return data.resourceDatas[i];
                    }
                }
            }
            return null;
        }

        #region Play
        private static readonly UnityAction EmptyAction = () => { };
        private Vector3 _startPosition;
        private Vector3 _toPosition;
        private int _destinationID;
        private Dictionary<int, int> _totalResourcesClaim;

        public void StopAllEffects()
        {
            StopAllCoroutines();
        }

        //public void Play(Dictionary<int, int> dat, Vector3 startLocation, Action callback = null)
        //{
        //    StopAllCoroutines();
        //    _startPosition = startLocation;
        //    _isPlaying = true;
        //    _resourceData = dat;
        //    StartCoroutine(PlayResourceAnimations(callback));
        //}
        //public void Play(Dictionary<int, int> dat, Action callback = null)
        //{
        //    StopAllCoroutines();
        //    _isPlaying = true;
        //    _resourceData = dat;
        //    StartCoroutine(PlayResourceAnimations(callback));
        //}
        //public void Play(ResourceData[] dat, Action callback = null)
        //{
        //    StopAllCoroutines();
        //    _isPlaying = true;
        //    _resourceData = dat.ToDictionary(x =>x.resourceID,y=>y.amount);
        //    StartCoroutine(PlayResourceAnimations(callback));
        //}
        public void Play(int resourceID, int amount)
        {
            Play(resourceID, amount, effectStartLocation.position, GetDestinationByResourceID(resourceID).IconPosition);
        }
        public void Play(int resourceID, int amount, Vector3 fromDestination)
        {
            Play(resourceID, amount, fromDestination, GetDestinationByResourceID(resourceID).IconPosition);
        }
        public void Play(int resourceID, int amount, Vector3 fromDestination, Vector3 toPosition)
        {
            Play(resourceID, amount, fromDestination, toPosition, resourceID, null);
        }
        public void Play(int resourceID, int amount, Vector3 fromDestination, int toPositionID)
        {
            var destinationItem = GetDestinationByID(toPositionID);
            if (destinationItem == null)
            {
                Debug.LogError($"[ETClaimEffectMain.Play] GetDestinationByID({toPositionID}) returned null. Cannot play effect.");
                return;
            }
            Play(resourceID, amount, fromDestination, destinationItem.IconPosition, toPositionID, null);
        }
        public void Play(int resourceID, int amount, Vector3 fromDestination, int toPositionID, UnityAction onCompletedCallback, UnityAction onFirstArrivalCallback)
        {
            var destinationItem = GetDestinationByID(toPositionID);
            if (destinationItem == null)
            {
                Debug.LogError($"[ETClaimEffectMain.Play] GetDestinationByID({toPositionID}) returned null. Cannot play effect.");
                return;
            }
            Play(resourceID, amount, fromDestination, destinationItem.IconPosition, toPositionID, onCompletedCallback, onFirstArrivalCallback);
        }
        public void Play(int resourceID, int amount, Vector3 fromDestination, Vector3 toPosition, int destinationID, UnityAction callback)
        {
            Play(resourceID, amount, fromDestination, toPosition, destinationID, callback, null);
        }
        public void Play(int resourceID, int amount, Vector3 fromDestination, Vector3 toPosition, int destinationID, UnityAction onCompletedCallback, UnityAction onFirstArrivalCallback)
        {
            _startPosition = fromDestination;
            _toPosition = toPosition;
            _totalResourcesClaim = new();
            _totalResourcesClaim.Add(resourceID, amount);
            _destinationID = destinationID;
            _onCompletedCallback = onCompletedCallback;
            _onFirstArrivalCallback = onFirstArrivalCallback;

            UnityAction safeCompleted = onCompletedCallback ?? EmptyAction;
            UnityAction safeFirstArrival = onFirstArrivalCallback ?? EmptyAction;
            StartCoroutine(PlayResourceAnimationsRoutine(resourceID, amount, fromDestination, toPosition, destinationID, safeCompleted, safeFirstArrival));
        }

        public void Play(int resourceID, int amount, Transform fromTransform, int toPositionID)
        {
            var destinationItem = GetDestinationByID(toPositionID);
            Play(resourceID, amount, fromTransform, destinationItem.IconPosition, toPositionID, EmptyAction, EmptyAction);
        }

        public void Play(int resourceID, int amount, Transform fromTransform, int toPositionID, UnityAction onCompletedCallback, UnityAction onFirstArrivalCallback)
        {
            var destinationItem = GetDestinationByID(toPositionID);
            Play(resourceID, amount, fromTransform, destinationItem.IconPosition, toPositionID, onCompletedCallback, onFirstArrivalCallback);
        }

        public void Play(int resourceID, int amount, Transform fromTransform, Vector3 toPosition, int destinationID, UnityAction callback)
        {
            Play(resourceID, amount, fromTransform, toPosition, destinationID, callback, null);
        }

        public void Play(int resourceID, int amount, Transform fromTransform, Vector3 toPosition, int destinationID, UnityAction onCompletedCallback, UnityAction onFirstArrivalCallback)
        {
            UnityAction safeCompleted = onCompletedCallback ?? EmptyAction;
            UnityAction safeFirstArrival = onFirstArrivalCallback ?? EmptyAction;
            StartCoroutine(PlayResourceAnimationsRoutine(resourceID, amount, fromTransform, toPosition, destinationID, safeCompleted, safeFirstArrival));
        }

        private IEnumerator PlayResourceAnimationsRoutine(
            int resourceID,
            int amount,
            Transform fromTransform,
            Vector3 toPosition,
            int destinationID,
            UnityAction onCompletedCallback,
            UnityAction onFirstArrivalCallback)
        {
            yield return new WaitForSeconds(data.timeDelayBeforeStart);
            PlayResourceAnimation(resourceID, amount, fromTransform.position, toPosition, destinationID, onCompletedCallback, onFirstArrivalCallback);
        }

        private IEnumerator PlayResourceAnimationsRoutine(
            int resourceID,
            int amount,
            Vector3 fromDestination,
            Vector3 toPosition,
            int destinationID,
            UnityAction onCompletedCallback,
            UnityAction onFirstArrivalCallback)
        {
            yield return new WaitForSeconds(data.timeDelayBeforeStart);
            PlayResourceAnimation(resourceID, amount, fromDestination, toPosition, destinationID, onCompletedCallback, onFirstArrivalCallback);
        }

        IEnumerator PlayResourceAnimations()
        {
            yield return new WaitForSeconds(data.timeDelayBeforeStart);
            foreach (var item in _totalResourcesClaim)
            {
                PlayResourceAnimation(item.Key);
                yield return new WaitForSeconds(data.timeBetweenEachResourceType);
            }
            yield return null;
        }

        private void PlayResourceAnimation(int resourceID)
        {
            PlayResourceAnimation(resourceID, _totalResourcesClaim[resourceID], _startPosition, _toPosition, _destinationID, _onCompletedCallback ?? EmptyAction, _onFirstArrivalCallback ?? EmptyAction);
        }

        private void PlayResourceAnimation(
            int resourceID,
            int amount,
            Vector3 fromDestination,
            Vector3 toPosition,
            int destinationID,
            UnityAction onCompletedCallback,
            UnityAction onFirstArrivalCallback)
        {
            ClaimResourceData claimResourceData = GetClaimResourceData(resourceID);
            if (claimResourceData == null)
            {
                Debug.LogError($"[ETClaimEffectMain.PlayResourceAnimation] GetClaimResourceData({resourceID}) returned null.");
                return;
            }
            int valuePerResourceParticle = claimResourceData.valuePerResourceParticle > 0 ? claimResourceData.valuePerResourceParticle : 1;
            int particleAmount = amount / valuePerResourceParticle;
            particleAmount += amount % valuePerResourceParticle > 0 ? 1 : 0;
            if (debugOn) Debug.Log($"ParticleAmount = {particleAmount}");
            Material mat = claimResourceData.material;
            GameObject go = Instantiate(data.rewardParticlePrefab, transform);
            go.transform.position = fromDestination;
            var rewardParticle = go.GetComponent<PopupRewardParticle>();
            rewardParticle.Init(this, data.rewardParticleSetting);

            UnityAction onComplete = () =>
            {
                StartCoroutine(PlayOnCompleted(onCompletedCallback));
            };

            var destinationItem = GetDestinationByID(destinationID);

            rewardParticle.StartAnim(toPosition, destinationID,
                particleAmount, mat, GetOrder(resourceID),
                amount, resourceID,
                onComplete,
                onFirstArrivalCallback,
                valuePerResourceParticle,
                destinationItem.IconRect);
        }
        public void OnCompletedAction()
        {
            StartCoroutine(PlayOnCompleted());
            _onCompletedCallback?.Invoke();
        }
        private void OnFirstArrivalAction()
        {
            if (_onFirstArrivalCallback == null)
            {
                return;
            }

            UnityAction callback = _onFirstArrivalCallback;
            _onFirstArrivalCallback = null;
            callback.Invoke();
        }
        /// <summary>
        /// Plays a single reward animation without cancelling any in-flight animations.
        /// Use this when multiple reward types need to fly simultaneously.
        /// </summary>
        public void PlayAdditive(int resourceID, int amount, Vector3 fromPosition, int destinationID,
            UnityAction onCompletedCallback)
        {
            var destinationItem = GetDestinationByID(destinationID);
            if (destinationItem == null)
            {
                Debug.LogError($"[ETClaimEffectMain.PlayAdditive] GetDestinationByID({destinationID}) returned null.");
                return;
            }

            Vector3 toPosition = destinationItem.IconPosition;
            ClaimResourceData claimResourceData = GetClaimResourceData(resourceID);
            int valuePerResourceParticle = claimResourceData.valuePerResourceParticle > 0 ? claimResourceData.valuePerResourceParticle : 1;
            int particleAmount = amount / valuePerResourceParticle;
            if (amount % valuePerResourceParticle > 0) particleAmount++;

            Material mat = claimResourceData.material;
            GameObject go = Instantiate(data.rewardParticlePrefab, transform);
            go.transform.position = fromPosition;
            var rewardParticle = go.GetComponent<PopupRewardParticle>();
            rewardParticle.Init(this, data.rewardParticleSetting);

            UnityAction onComplete = () =>
            {
                StartCoroutine(PlayOnCompleted());
                onCompletedCallback?.Invoke();
            };

            rewardParticle.StartAnim(toPosition, destinationID,
                particleAmount, mat, GetOrder(resourceID),
                amount, resourceID,
                onComplete, null,
                valuePerResourceParticle,
                destinationItem.IconRect);
        }

        int GetOrder(int resourceID)
        {
            return 10;
        }
        #endregion

        #region OnCompleted
        public UnityEvent onCompleted = new();
        //private int _maxResourceHit;
        //private int _currentResourceHit;
        //private int CurrentResourceHit
        //{
        //    get => _currentResourceHit;
        //    set
        //    {
        //        _currentResourceHit = value;
        //        CheckOnCompleted();
        //
        //    }
        //
        //}
        //private void CheckOnCompleted()
        //{
        //    if (_currentResourceHit >= _maxResourceHit) StartCoroutine(PlayOnCompleted());
        //}
        public void SetUpOnComplete(UnityAction onCompletedAction)
        {
            onCompleted.AddListener(onCompletedAction);
        }
        private IEnumerator PlayOnCompleted(UnityAction onCompletedCallback)
        {
            yield return new WaitForSeconds(data.onCompletedEventDelay);
            if (debugOn) Debug.Log("HomingEffectCompleted");
            onCompleted.Invoke();
            onCompletedCallback.Invoke();
        }
        private IEnumerator PlayOnCompleted()
        {
            yield return new WaitForSeconds(data.onCompletedEventDelay);
            if (debugOn) Debug.Log("HomingEffectCompleted");
            onCompleted.Invoke();
        }
        #endregion
    }

    [Serializable]
    public class ETClaimEffectData
    {
        public float timeDelayBeforeStart = 0f;
        [Header("Resources")]
        public ClaimResourceData[] resourceDatas;
        public bool manualSetDestinationData = true;
        public UIResourceDestinationData[] destinationDatas;
        [Header("Particle")]
        [FormerlySerializedAs("pp_popupRewardParticle")]
        public GameObject rewardParticlePrefab;
        public PopupRewardParticleSetting rewardParticleSetting;
        [Header("Multiple effect")]
        public float timeBetweenEachResourceType = 0.5f;
        [Header("Oncompleted")]
        public float onCompletedEventDelay;
    }
    [Serializable]
    public struct UIResourceDestinationData
    {
        [FormerlySerializedAs("desticationID")]
        public int destinationID;
        public int resourceID;
        public UIResourceDestinationItem destinationItem;
    }
}
