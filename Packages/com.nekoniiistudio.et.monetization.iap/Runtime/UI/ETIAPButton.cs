using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ET.Monetization
{
    /// <summary>
    /// Reactive UI component for displaying localized IAP product metadata, ownership visual state,
    /// and handling user purchase / restore interactions via Inspector Touch callbacks.
    /// </summary>
    public class ETIAPButton : MonoBehaviour
    {
        [Header("Product Configuration")]
        [SerializeField] private string _productId;
        [SerializeField] private string _placement = "shop";

        [Header("UI Text References")]
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private bool _autoUpdateLocalizedPrice = true;
        [SerializeField] private bool _autoUpdateLocalizedTitle = false;
        [SerializeField] private bool _autoUpdateLocalizedDescription = false;
        [SerializeField] private string _fallbackPriceText = "$0.99";

        [Header("Visual State Configuration")]
        [SerializeField] private OwnedState _ownedState;
        [SerializeField] private GameObject _loadingIndicator;

        [Header("Events")]
        [SerializeField] private UnityEvent<UnityIAPPurchaseResult> _onPurchaseSucceeded;
        [SerializeField] private UnityEvent<UnityIAPPurchaseFailure> _onPurchaseFailed;
        [SerializeField] private UnityEvent<bool> _onOwnershipStateChanged;

        private UnityIAPManager _manager;
        private Color _initialImageColor;
        private bool _hasCachedInitialColor;

        public string ProductId
        {
            get => _productId;
            set
            {
                _productId = value;
                UpdateUI();
            }
        }

        public string Placement
        {
            get => _placement;
            set => _placement = value;
        }

        private UnityIAPManager ActiveManager => _manager ?? UnityIAPManager.DefaultInstance;

        public void BindManager(UnityIAPManager manager)
        {
            UnsubscribeEvents();
            _manager = manager;
            if (isActiveAndEnabled)
            {
                SubscribeEvents();
                UpdateUI();
            }
        }

        private void OnEnable()
        {
            CacheInitialVisuals();
            SubscribeEvents();
            UpdateUI();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        /// <summary>
        /// Drag-and-drop this handler into the Button's OnClick() event list in the Unity Inspector
        /// to initiate a purchase for the configured product.
        /// </summary>
        public void TouchPurchaseButton()
        {
            var manager = ActiveManager;
            if (manager == null)
            {
                throw new InvalidOperationException($"[ETIAPButton] Cannot purchase '{_productId}': No UnityIAPManager instance is bound or initialized.");
            }

            SetLoadingState(true);
            manager.Purchase(
                _productId,
                onSuccess: result =>
                {
                    SetLoadingState(false);
                    UpdateUI();
                    _onPurchaseSucceeded?.Invoke(result);
                },
                onFailed: failure =>
                {
                    SetLoadingState(false);
                    UpdateUI();
                    _onPurchaseFailed?.Invoke(failure);
                },
                placement: _placement
            );
        }

        /// <summary>
        /// Drag-and-drop this handler into a Restore Button's OnClick() event list in the Unity Inspector
        /// to trigger store transaction restoration.
        /// </summary>
        public void TouchRestoreButton()
        {
            var manager = ActiveManager;
            if (manager == null)
            {
                throw new InvalidOperationException("[ETIAPButton] Cannot restore purchases: No UnityIAPManager instance is bound or initialized.");
            }

            SetLoadingState(true);
            manager.RestorePurchases((success, _) =>
            {
                SetLoadingState(false);
                UpdateUI();
            });
        }

        /// <summary>
        /// Refreshes the localized price, title, description, and owned visual state.
        /// </summary>
        public void UpdateUI()
        {
            if (string.IsNullOrEmpty(_productId)) return;

            var manager = ActiveManager;
            if (manager == null)
            {
                ApplyFallbackText();
                return;
            }

            var meta = manager.GetProductMetadata(_productId);

            if (_autoUpdateLocalizedPrice && _costText != null)
            {
                _costText.text = !string.IsNullOrEmpty(meta.LocalizedPriceString)
                    ? meta.LocalizedPriceString
                    : _fallbackPriceText;
            }

            if (_autoUpdateLocalizedTitle && _titleText != null && !string.IsNullOrEmpty(meta.LocalizedTitle))
            {
                _titleText.text = meta.LocalizedTitle;
            }

            if (_autoUpdateLocalizedDescription && _descriptionText != null && !string.IsNullOrEmpty(meta.LocalizedDescription))
            {
                _descriptionText.text = meta.LocalizedDescription;
            }

            SetOwning(meta.IsOwned);
        }

        /// <summary>
        /// Applies the owned or unowned visual state to the configured UI elements.
        /// </summary>
        public void SetOwning(bool owned)
        {
            CacheInitialVisuals();

            if (_ownedState.changeColorImage != null)
            {
                _ownedState.changeColorImage.color = owned ? _ownedState.color : _initialImageColor;
            }

            if (_ownedState.enableObject != null)
            {
                _ownedState.enableObject.SetActive(owned);
            }

            if (_ownedState.disableObject != null)
            {
                _ownedState.disableObject.SetActive(!owned);
            }

            if (_ownedState.disableButton != null)
            {
                _ownedState.disableButton.interactable = !owned;
            }

            if (owned && !string.IsNullOrEmpty(_ownedState.ownedCostTextOverride) && _costText != null)
            {
                _costText.text = _ownedState.ownedCostTextOverride;
            }

            _onOwnershipStateChanged?.Invoke(owned);
        }

        private void SetLoadingState(bool isLoading)
        {
            if (_loadingIndicator != null)
            {
                _loadingIndicator.SetActive(isLoading);
            }
        }

        private void ApplyFallbackText()
        {
            if (_autoUpdateLocalizedPrice && _costText != null && !string.IsNullOrEmpty(_fallbackPriceText))
            {
                _costText.text = _fallbackPriceText;
            }
        }

        private void CacheInitialVisuals()
        {
            if (_hasCachedInitialColor) return;
            if (_ownedState.changeColorImage != null)
            {
                _initialImageColor = _ownedState.changeColorImage.color;
                _hasCachedInitialColor = true;
            }
        }

        private void SubscribeEvents()
        {
            var manager = ActiveManager;
            if (manager == null) return;

            manager.OnInitialized += HandleInitialized;
            manager.OnProductsFetched += HandleProductsFetched;
            manager.OnOwnershipChanged += HandleOwnershipChanged;
        }

        private void UnsubscribeEvents()
        {
            var manager = ActiveManager;
            if (manager == null) return;

            manager.OnInitialized -= HandleInitialized;
            manager.OnProductsFetched -= HandleProductsFetched;
            manager.OnOwnershipChanged -= HandleOwnershipChanged;
        }

        private void HandleInitialized() => UpdateUI();

        private void HandleProductsFetched(IReadOnlyList<UnityIAPProductMetadata> _) => UpdateUI();

        private void HandleOwnershipChanged(string changedProductId, bool isOwned)
        {
            if (string.Equals(changedProductId, _productId, StringComparison.Ordinal))
            {
                UpdateUI();
            }
        }

        [Serializable]
        public struct OwnedState
        {
            public Color color;
            public Image changeColorImage;
            public GameObject enableObject;
            public GameObject disableObject;
            public Button disableButton;
            public string ownedCostTextOverride;
        }
    }
}
