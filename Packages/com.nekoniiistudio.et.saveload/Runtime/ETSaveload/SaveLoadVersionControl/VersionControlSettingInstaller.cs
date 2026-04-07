using ET.FileSystem;
using System;
namespace ET.Engine.Installer
{
    //[CreateAssetMenu(fileName = "VersionControlSettingInstaller", menuName = "Modules/Installer")]
    //public class VersionControlSettingInstaller : ScriptableObject, IModuleInstaller
    //{
    //    [SerializeField] ApplicationSetting _applicationSetting;
    //
    //    public void Install(DiContainer container)
    //    {
    //        container.BindInstance(_applicationSetting).AsSingle().NonLazy();
    //    }
    //}
}
namespace ET.Engine
{
    [Serializable]
    public class ApplicationSetting
    {
        public DataVersionControl dataVersionControl;
    }
    [Serializable]
    public struct DataVersionControl //control version of data// out date controls
    {
        public int currentDataVersion;
        public DataVersionControlDataVersionRequire[] dataVersionRequire;
    }
    [Serializable]
    public struct DataVersionControlDataVersionRequire //control version of data// out date controls
    {
        public SaveFileType saveFileType;
        public int requireDataVersion;
    }
}