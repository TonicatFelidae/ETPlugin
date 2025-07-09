using ET.UIKit.ZenjectUIScreen;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace ET.Engine.Installer
{
    public class ScreenMachineInstaller
    {
        public static void Install(DiContainer container)
        {

            container.Bind<ScreenMachine>().AsSingle().Lazy();
            container
                .BindFactory<GameObject, UIScreenLayer, UIScreen, UIScreen.Factory>()
                .FromFactory<UIScreen.CustomFactory>();
            container
                .BindFactory<GameObject, PopupUIScreen, PopupUIScreen.Factory>()
                .FromFactory<PopupUIScreen.CustomFactory>();

        }
    }

}
