using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace ET.UIKit.EUExpandableSystem
{
    //BASE CLASS
    public class EUExpandableListItemUI : MonoBehaviour
    {
        private EUExpandableListItemUI[] _pp_EUExpandableListItemUIs;
        private Transform _content;
        [SerializeField] private Button _expandButton;
        [HideInInspector] public int dataDepth;
        private EUE_stringListData _data;
        private ExpandableUIState _uIExpandState;
        public ExpandableUIState ExpandState
        {
            get => _uIExpandState;
            set
            {
                _uIExpandState = value;
                OnExpandStateChange(value);
            }
        }
        public EUExpandableType expandableType;
        public void Init(EUExpandableListItemUI[] pp_EUExpandableListItemUIs, Transform content)
        {
            _pp_EUExpandableListItemUIs = pp_EUExpandableListItemUIs;
            _content = content;
        }
        public virtual void Setup(EUE_stringListData data, int depth, UnityAction<EUE_stringListData, int> expandUI)
        {
            _data = data;
            dataDepth = depth;
            _expandButton.onClick.AddListener(() =>
            {
                OnExpandButtonClicked(_data.items.Count>0 || expandableType != EUExpandableType.ExpandChild);
                expandUI.Invoke(_data, dataDepth);
            }
            );
            //one loop only use recursive
            for (int i = 0; i < _data.items.Count; i++)
            {
                EUExpandableListItemUI go = GameObject.Instantiate(_pp_EUExpandableListItemUIs[depth], _content);
                _data.items[i].bindUI = go;  
                go.Init(_pp_EUExpandableListItemUIs, _content);
                go.Setup(_data.items[i], depth + 1, expandUI);
            }
            ExpandState = ExpandableUIState.Expand;
            gameObject.SetActive(true);
        }
        public virtual void OnExpandButtonClicked(bool expandable)
        {

        }
        public virtual void OnExpandStateChange(ExpandableUIState uiExpandState)
        {

        }
        public virtual void OnExpandSelfContent(ExpandableUIState uiExpandState)
        {

            LayoutRebuilder.ForceRebuildLayoutImmediate(_content.GetComponent<RectTransform>());
        }
        public void ExpandShrink()
        {
            switch (ExpandState)
            {
                case ExpandableUIState.Expand:
                    Shrink();
                    break;
                case ExpandableUIState.Shrink:
                    Expand();
                    break;
                default:
                    Shrink();
                    break;
            }
        }
        public void Expand()
        {
            switch (expandableType)
            {
                case EUExpandableType.ExpandChild:
                    for (int i = 0; i < _data.items.Count; i++)
                    {
                        _data.items[i].bindUI.Show();
                    }
                    break;
                case EUExpandableType.ExpandSelfContent:
                    OnExpandSelfContent(ExpandableUIState.Expand);
                    break;
                default:
                    break;
            }
            ExpandState = ExpandableUIState.Expand;
        }
        public void Shrink()
        {
            switch (expandableType)
            {
                case EUExpandableType.ExpandChild:
                    for (int i = 0; i < _data.items.Count; i++)
                    {
                        _data.items[i].bindUI.Shrink();
                        _data.items[i].bindUI.Hide();
                    }
                    break;
                case EUExpandableType.ExpandSelfContent:
                    OnExpandSelfContent(ExpandableUIState.Shrink);
                    break;
                default:
                    break;
            }
            ExpandState = ExpandableUIState.Shrink;
        }
        public void Show()
        {
            gameObject.SetActive(true);
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
    public enum EUExpandableType
    { 
        ExpandChild,
        ExpandSelfContent,
    }

}
