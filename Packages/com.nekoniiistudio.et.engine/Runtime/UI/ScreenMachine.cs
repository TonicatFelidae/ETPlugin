using ET.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace ET.UIKit.ZenjectUIScreen
{
    public class ScreenMachine : IFactoryManager<UIScreen>
    {
        //////DEVTOOL
        private bool enableDebugLog = false;
        //////
        public static string path = "Prefabs/UI";
        [Inject] UIScreen.Factory _UIScreen;
        private UIScreen _curScreen;
        public UIScreen CurItem => _curScreen;
        /// <summary>
        /// Get exist Screen, do not create new one, be careful when using it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetScreenClass<T>() where T : UIScreen => (T)(ItemDict.FirstOrDefault(pair => pair.Value is T).Value);
        public T Get<T>() where T : UIScreen => GetScreenClass<T>();
        public Dictionary<string, UIScreen> ItemDict { get; set; } = new();

        public T Load<T>() where T : UIScreen => LoadScreen<T>();
        /// <summary>
        /// Create or get exist screen
        /// </summary>
        /// <param name="loadType"></param>
        /// <param name="uIScreenLayer"></param>
        /// <returns></returns>
        public T LoadScreen<T>(LoadType loadType = LoadType.Single, UIScreenLayer uIScreenLayer = UIScreenLayer.Default) where T : UIScreen => (T)LoadScreen(typeof(T).Name, loadType, uIScreenLayer);
        /// <summary>
        /// Create or get exist screen
        /// </summary>
        /// <param name="loadType"></param>
        /// <param name="uIScreenLayer"></param>
        /// <returns></returns>
        public UIScreen LoadScreen(string namex, LoadType loadType = LoadType.Single, UIScreenLayer uIScreenLayer = UIScreenLayer.Default)
        {
            UIScreen newScreen;
            if (ItemDict.ContainsKey(namex))
            {
                newScreen = ItemDict[namex];
            }
            else
            {
                newScreen = _UIScreen.Create(GetScreenFromPath(namex), uIScreenLayer);
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
                            if (!item.Value.isPermanently) item.Value.gameObject.SetActive(false);

                        }
                    }
                    break;
                case LoadType.Addition:
                    newScreen.gameObject.SetActive(true);
                    newScreen.transform.SetAsLastSibling();
                    break;
                case LoadType.Permanently:
                    newScreen.isPermanently = true;
                    break;
                default:
                    break;
            }

            Debug.Log("[ScreenMachine] loaded " + namex);
            _curScreen = newScreen;
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

        public T Unload<T>() where T : UIScreen
        {
            T uIScreen = Get<T>();
            uIScreen.Unload();
            return uIScreen;
        }

        public T UpdateNewState<T>() where T : UIScreen
        {
            T uIScreen = Get<T>();
            uIScreen.UpdateNewState();
            return uIScreen;
        }
        public T LoadPopup<T>(LoadType loadType = LoadType.Single, params object[] datas) where T : PopupUIScreen
        {
            PopupUI popupUI = LoadScreen<PopupUI>(LoadType.Addition);
            T popup = popupUI.LoadPopup<T>(loadType, datas);
            return popup;
        }
        public T GetPopup<T>() where T : PopupUIScreen => Get<PopupUI>().Get<T>();
    }
    public enum LoadType
    {
        Single,
        Addition,
        Permanently, 
    }
    /// <summary>
    /// Position when object load from inactive prefabs
    /// </summary>
    public enum LoadPositionType
    {
        Current,
        SpawningPoint,
        QuestSpawningPoint,

    }

    public enum PopupName
    {
        None,
        PopupSetting,
        PopupNewVehicleNoti,
        PopupPause,

    }
}
