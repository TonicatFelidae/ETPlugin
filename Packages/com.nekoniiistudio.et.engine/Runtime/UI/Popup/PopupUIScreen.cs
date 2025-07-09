using ET;
using UnityEngine;
using Zenject;

namespace ET.UIKit.ZenjectUIScreen
{
    public class PopupUIScreen : MonoBehaviour, IIDItem
    {
        protected PopupUI _popupUI;


        public string _ID;
        public string ID => _ID;
        public virtual void Load(params object[] paras)
        {
            gameObject.SetActive(true);
        }
        virtual public void Dispose()
        {
        }

        virtual public void Initialize()
        {
        }

        virtual public void UpdateNewState()
        {
        }
        public void Unload()
        {
            gameObject.SetActive(false);
        }
        public void ClosePopupUI()
        {
            _popupUI.ClosePopupUI();
        }
        public class Factory : PlaceholderFactory<GameObject, PopupUIScreen>
        { }
        public class CustomFactory : IFactory<GameObject, PopupUIScreen>
        {
            private readonly DiContainer _container;

            public CustomFactory(DiContainer container)
            {
                _container = container;
            }

            [Inject] ScreenMachine _screenMachine;
            public PopupUIScreen Create(GameObject prefab)
            {
                if (!prefab.TryGetComponent(out PopupUIScreen l))
                    prefab.AddComponent<PopupUIScreen>();
                PopupUI popupUI = _screenMachine.Get<PopupUI>();
                Transform parent = popupUI.boxContent;

                PopupUIScreen unityGo = _container.InstantiatePrefabForComponent<PopupUIScreen>(prefab, parent);

                unityGo._popupUI = popupUI; 
                unityGo.Initialize();
                Debug.Log("[ScreenMachine] loaded " + unityGo);
                return unityGo;
            }
        }
        private void OnDestroy()
        {
            Dispose();
        }
    }
}