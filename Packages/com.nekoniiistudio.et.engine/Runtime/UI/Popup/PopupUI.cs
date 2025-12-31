using ET.Engine;
using ET.SupportKit.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace ET.UIKit.ZenjectUIScreen
{
    /// <summary>
    /// ET buildin UIScreen PopupUI control PopupUIItem
    /// IT will inject all nessasary class and put it in its item for them to work separatly
    /// Load popup depend on string namex
    /// </summary>
    public class PopupUI : UIScreen, IFactoryManager<PopupUIScreen>
    {
        //////DEVTOOL
        private bool enableDebugLog = false;
        //////
        public static string path = "Prefabs/UI/Popup";

        [Inject] PopupUIScreen.Factory _popupUIScreen;
        private PopupUIScreen _curScreen;
        public PopupUIScreen CurItem => _curScreen;
        public T Get<T>() where T : PopupUIScreen => (T)(ItemDict.FirstOrDefault(pair => pair.Value is T).Value);
        public Transform boxContent;
        public Dictionary<string, PopupUIScreen> ItemDict { get; set; } = new();
        private Animator _animator;
        public Animator Animator { get { if (_animator == null) _animator = GetComponent<Animator>(); return _animator; } }

        public T Load<T>() where T : PopupUIScreen => LoadPopup<T>();
        public T LoadPopup<T>(LoadType loadType = LoadType.Single, params object[] datas) where T : PopupUIScreen => (T)LoadPopup(typeof(T).Name,loadType, datas);
        public PopupUIScreen LoadPopup(string namex, LoadType loadType = LoadType.Single, params object[] datas)
        {
            PopupUIScreen newScreen;
            if (ItemDict.ContainsKey(namex))
            {
                newScreen = ItemDict[namex];
            }
            else
            {
                newScreen = _popupUIScreen.Create(GetScreenFromPath(namex));
                ItemDict.Add(namex, newScreen);
            }
            switch (loadType)
            {
                case LoadType.Single:
                    foreach (var item in ItemDict)
                    {
                        if (item.Key == namex)
                        {
                            item.Value.gameObject.SetActive(true);
                        }
                        else
                        {
                            item.Value.gameObject.SetActive(false);

                        }
                    }
                    break;
                case LoadType.Addition:
                    newScreen.gameObject.SetActive(true);
                    newScreen.transform.SetAsLastSibling();
                    break;
                case LoadType.Permanently:
                    break;
                default:
                    break;
            }

            Debug.Log("[PopupUI] loaded " + namex);
            _curScreen = newScreen;
            Animator?.SetTrigger("LoadPopup");
            ItemDict[namex].Load(datas);
            return _curScreen;
        }

        public GameObject GetScreenFromPath(string namex)
        {
            Debug.Log(GetPathFromName(namex));
            GameObject go = (GameObject)Resources.Load(GetPathFromName(namex));
            go.SetActive(true);
            return go;
        }
        public string GetPathFromName(string namex)
        {
            return $"{path}/{namex}";
        }
        public void ClosePopupUI()
        {
            Animator?.SetTrigger("UnloadPopup"); ;
        }
        public void DisposeUI()
        {
            foreach (var popup in ItemDict)
            {
                popup.Value.Unload();
            }
            gameObject.SetActive(false);
        }


        public T Unload<T>() where T : PopupUIScreen
        {
            T uIScreen = Get<T>();
            uIScreen.Unload();
            return uIScreen;
        }

        public T UpdateNewState<T>() where T : PopupUIScreen
        {
            T uIScreen = Get<T>();
            uIScreen.UpdateNewState();
            return uIScreen;
        }
    }
    public enum PopupType
    {
        Confirm, // => TextOk 
        YesNO, // => TextYesNo 
        Loading, // => Small Box of loading 
    }
    public interface IPopupUIItem
    {
        PopupUI popupUI { get; set; }
    }
}