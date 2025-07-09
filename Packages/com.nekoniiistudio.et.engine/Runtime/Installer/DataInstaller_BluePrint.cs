using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace ET.Engine.Installer
{
    /// <summary>
    /// Type 100 blueprint installer class >> DataInstaller <<
    /// TODO : improve debug color
    /// Unitask required for webgl
    /// </summary>
    public class DataInstaller_BluePrint : MonoInstaller
    {
        //SData_BluePrint _sData = new();
        ////private List<string> randomCityName;
        //private List<GameObject> _vehicles = new();
        ////private Func<GameObject, VehicleType> gVehiclesConv => x => x.GetComponent<VehicleBody>().vehicleType;
        ////private Func<GameObject, VehicleBody> gVehiclesConvKey => x => x.GetComponent<VehicleBody>();
        //public override void InstallBindings()
        //{
        //    Container.BindInstance(_sData);
        //    _ = LoadAssets();
        //    //_staticData.randomCityName = randomCityName;
        //
        //}
        //private async UniTask LoadAssets()
        //{
        //    UniTask loadVehicles = _vehicles.ReLoadAssetsByKey("GVehicle");
        //    await UniTask.WhenAll(
        //        loadVehicles
        //        );
        ////    _staticData.vehicles = _vehicles.ToDictionary(gVehiclesConv, gVehiclesConvKey);
        //   // D.Sys.File($"[DataInstaller] vehicles: {_staticData.vehicles.Count}");
        //}
    }
    /// <summary>
    /// Static data class, keep data thoughy ouit game, never change
    /// </summary>
    public class SData_BluePrint
    {
        //public List<GameObject> _vehicles = new();
    }

}
