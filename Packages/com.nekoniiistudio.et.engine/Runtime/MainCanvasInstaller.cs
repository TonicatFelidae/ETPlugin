using ET.UIKit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace ET.Engine.Installer
{
    public class MainCanvasInstaller
    {
        private static string path = "Prefabs/UI/MainCanvas";
        public static void Install(DiContainer container)
        {
            container.BindInterfacesAndSelfTo<MainCanvas>().FromComponentInNewPrefabResource(path).AsSingle();
        }
    }

}