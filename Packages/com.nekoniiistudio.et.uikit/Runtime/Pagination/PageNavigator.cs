using ET.SupportKit;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ET.UIKit.PaginationSystem
{
    public class PageNavigator : MonoBehaviour
    {
        public int itemPerPage = 10;

        [SerializeField] private EUButton _butLeft;
        [SerializeField] private EUButton _butRight;
        [SerializeField] private PageIndicator _pageIndicator;

        [HideInInspector] public UnityEvent onButLeftClick = new();
        [HideInInspector] public UnityEvent onButRightClick = new();

        private int _curMaxItemCount = 0;
        #region CurPage
        private int _curPage = 0;
        public int CurPage => _curPage;
        #endregion
        #region MaxPage
        private int _maxPage = 0;
        public int MaxPage
        {
            get => _maxPage;
            set
            {
                if (_maxPage!= value)
                {
                    _maxPage = value;
                    if (_curPage >= _maxPage)
                    {
                        _butLeft.button.onClick.Invoke();
                    }
                    else
                    {
                        UpdateState();
                    }
                }
            }
        }
        #endregion

        #region CurPageFromNumber
        private int _curPageFromNumber;
        public int CurPageFromNumber => _curPageFromNumber;
        #endregion
        #region CurPageToNumber
        public int _curPageToNumber;
        public int CurPageToNumber => _curPageToNumber;
        #endregion
        public void Setup(UnityAction onButLeftClick, UnityAction onButRightClick)
        {
            this.onButLeftClick.AddListener(onButLeftClick);
            this.onButRightClick.AddListener(onButRightClick);
        }
        private void Awake()
        {
            _butLeft.button.onClick.AddListener(() =>
            {
                ChangePage(-1);
                onButLeftClick.Invoke();
                UpdateState();
            });
            _butRight.button.onClick.AddListener(() =>
            {
                ChangePage(1);
                onButRightClick.Invoke();
                UpdateState();
            });
        }
        public void UpdateData(int maxItemCount)
        {
            _curMaxItemCount = maxItemCount;
            MaxPage = (_curMaxItemCount + itemPerPage - 1) / itemPerPage;
        }
        private void ChangePage(int changeAmount)
        {
            _curPage = ETMath.ClampInLoop(_curPage + changeAmount, 0, 5);
            _curPageFromNumber = _curPage * itemPerPage;
            _curPageToNumber = Mathf.Max(0, Mathf.Min((_curPage + 1) * itemPerPage, _curMaxItemCount)) - 1;
        }
        private PageNavigatorState GetPageNavigatorState()
        {
            if (_curMaxItemCount <= itemPerPage)
            {
                _curPage = 0;
                return PageNavigatorState.OnePageOnly;
            }
            else
            {
                if (_curPage == 0)
                {
                    return PageNavigatorState.FarLeft;
                }
                else if (_curPage == _maxPage - 1) 
                {
                    return PageNavigatorState.FarRight;
                }
                else 
                { return PageNavigatorState.Middle; }


            }
        }
        public void UpdateState()
        {
            switch (GetPageNavigatorState())
            {
                case PageNavigatorState.OnePageOnly:
                    _butLeft.SetInteractable(false);
                    _butRight.SetInteractable(false);
                    break;
                case PageNavigatorState.FarLeft:
                    _butLeft.SetInteractable(false);
                    _butRight.SetInteractable(true);
                    break;
                case PageNavigatorState.Middle:
                    _butLeft.SetInteractable(true);
                    _butRight.SetInteractable(true);
                    break;
                case PageNavigatorState.FarRight:
                    _butLeft.SetInteractable(true);
                    _butRight.SetInteractable(false);
                    break;
                default:
                    break;
            }
            _pageIndicator.Show(
                _curMaxItemCount, 
                CurPage, 
                itemPerPage,
                CurPageFromNumber,
                CurPageToNumber
                );
        }
        public enum PageNavigatorState
        {
            OnePageOnly,
            FarLeft,
            Middle,
            FarRight
        }
    }
}
