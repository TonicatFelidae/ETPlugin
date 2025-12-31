using System.Threading.Tasks;
using UnityEngine;

namespace ETEngine.ResourceManagement
{
    public interface IAssetLoader
    {
        AssetLoadHandle<T> Load<T>(string key) where T : Object;

        AssetLoadHandle<T> LoadAsync<T>(string key) where T : Object;
        Task<T> LoadAssetAsync<T>(string path) where T : Object;
        T LoadAsset<T>(string path) where T : Object;
        void Release(AssetLoadHandle handle);
    }
}