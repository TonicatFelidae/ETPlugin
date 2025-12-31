using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;
namespace ET.UIKit.EUExpandableSystem
{
    /// <summary>
    /// Read data and render to UI
    /// Limit to string list
    /// Prefab is in order of parrent,child,child
    /// EUExpandableList        AAAAAA (the different in metaphor)
    /// EUExpandableListItemUI  BBAAAA (the different in metaphor)
    /// Solo class
    /// </summary>
    public class EUExpandableList : MonoBehaviour
    {
        private EUE_stringListData _eue_stringListData;
        private EUE_stringListData Data => _eue_stringListData; 
        [SerializeField] private EUExpandableListItemUI[] _pp_EUExpandableListItemUIs;
        [SerializeField] private Transform _content;
        private List<EUExpandableListItemUI> _UIList;
        public void Init(EUE_stringListData eue_stringListData)
        {
            this._eue_stringListData = eue_stringListData;
            CheckValid();
        }
        public void ReBuildListUI()
        {
            _UIList = new();
            HideAllPP();
            //first value is not matter, ignore it
            int depth = 0;
            //one loop only use recursive
            for (int i = 0; i < Data.items.Count; i++)
            {
                EUExpandableListItemUI go = GameObject.Instantiate(_pp_EUExpandableListItemUIs[depth], _content);
                go.Init(_pp_EUExpandableListItemUIs, _content);
                go.Setup(Data.items[i], depth +1, ExpandItemUI);
                Data.items[i].bindUI = go;
                _UIList.Add(go);    
            }
            ShrinkAllItemToDepthLevel(1);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content.GetComponent<RectTransform>());

            void HideAllPP()
            {
                for (int i = 0; i < _pp_EUExpandableListItemUIs.Length; i++)
                {
                    _pp_EUExpandableListItemUIs[i].gameObject.SetActive(false);   
                }
            }
        }
        private void ExpandItemUI(EUE_stringListData xData, int depth )
        {
            Debug.Log(xData.items.Count + " " + depth);
            ExpandableUIState curState = xData.bindUI.ExpandState;
            switch (curState)
            {
                case ExpandableUIState.Expand:
                    ShrinkAllItemToDepthLevel(depth);
                    break;
                case ExpandableUIState.Shrink:
                    ShrinkAllItemToDepthLevel(depth);
                    xData.bindUI.Expand();
                    break;
                default:
                    break;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content.GetComponent<RectTransform>());
        }
        public void ShrinkAllItemToDepthLevel(int depth)
        {
            for (int i = 0; i < _UIList.Count; i++)
            {
                if (_UIList[i].dataDepth == depth)
                    _UIList[i].Shrink();
            }
        }
        public void CheckValid()
        {
            int level = GetDepthLevel(_eue_stringListData); // ignore first level
            Debug.Log($"Data depth level: {level}");
            if (level == -1 )
            {
                Debug.LogError("Data level -1 error");
            }
            else if(level == 0)
            {
                Debug.LogError("Data level 0 error: empty items list");
            }
            else if (_pp_EUExpandableListItemUIs == null)
            {
                Debug.LogError($"Prefab array null");
            }
            else if (level != _pp_EUExpandableListItemUIs.Length)
            {
                Debug.LogError($"Data level {level} error not same as prefab count {_pp_EUExpandableListItemUIs.Length}");
            }
            else
            {
                Debug.Log($"Data valid");
            }


            int GetDepthLevel(EUE_stringListData data)
            {
                if (data.items == null || data.items.Count == 0)
                    return 0;

                int maxDepth = 0;
                foreach (var child in data.items)
                {
                    int childDepth = GetDepthLevel(child);
                    if (childDepth > maxDepth)
                        maxDepth = childDepth;
                }
            
                return maxDepth + 1;
            }
        }

        public EUE_stringListData CreateData_KeyID_ValueIDTittleContent(Dictionary<string, List<string>> data)
        {
            //""
            //1
            //--2
            //1
            //--2
            //1
            //--2
            EUE_stringListData ret = new("");
            foreach (var item in data)
            {
                var scope0 = item.Value;
                ret.items.Add(new EUE_stringListData(scope0[1], scope0[2]));
            }
            return ret;
        }
        public EUE_stringListData CreateData_DictionaryStringString (Dictionary<string, string> data)
        {
            //""
            //1
            //--2
            //1
            //--2
            //1
            //--2
            EUE_stringListData ret = new("");
            foreach (var item in data)
            {
                ret.items.Add(new EUE_stringListData(item.Value));
            }
            return ret;
        }
    }
}