using System.Threading.Tasks;
using UnityEngine;

namespace ETEngine.ResourceManagement
{
    public abstract class AssetLoaderObject : ScriptableObject, IAssetLoader
    {
        public Task<T> LoadAssetAsync<T>(string path) where T : Object
        {
            throw new System.NotImplementedException();
        }

        public T LoadAsset<T>(string path) where T : Object
        {
            throw new System.NotImplementedException();
        }

        public abstract AssetLoadHandle<T> Load<T>(string key) where T : Object;

        public abstract AssetLoadHandle<T> LoadAsync<T>(string key) where T : Object;

        public abstract void Release(AssetLoadHandle handle);
    }
}