using UnityEngine;
using Zenject;

namespace ET.UIKit.ZenjectUIScreen
{
    // Version 1.2
    public abstract class UIScreen : MonoBehaviour
    {
        public bool isPermanently;
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
        public class Factory : PlaceholderFactory<GameObject, UIScreenLayer, UIScreen>
        { }
        public class CustomFactory : IFactory<GameObject, UIScreenLayer, UIScreen>
        {
            private readonly DiContainer _container;

            public CustomFactory(DiContainer container)
            {
                _container = container;
            }

            [Inject] MainCanvas _mainCanvas;
            public UIScreen Create(GameObject prefab, UIScreenLayer uIScreenLayer)
            {
                if (!prefab.TryGetComponent(out UIScreen l))
                    prefab.AddComponent<UIScreen>();
                Transform parent = null;
                switch (uIScreenLayer)
                {
                    case UIScreenLayer.Default:
                        parent = _mainCanvas.content;

                        break;
                    case UIScreenLayer.Top:
                        parent = _mainCanvas.topContent;
                        break;
                    case UIScreenLayer.Bottom:
                        parent = _mainCanvas.bottomContent;
                        break;

                    case UIScreenLayer.Popup:
                        parent = _mainCanvas.popupContent;
                        break;

                    default:
                        break;
                }

                UIScreen unityGo = _container.InstantiatePrefabForComponent<UIScreen>(prefab, parent);
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

