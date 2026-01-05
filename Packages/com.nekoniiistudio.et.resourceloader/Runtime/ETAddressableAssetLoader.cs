using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ET
{
    public static class ETAddressableAssetLoader
    {//Load asset
        public static Task<IList<T>> LoadAssetsByKey<T>(this IList<T> retList, string key)
        {
            Addressables.LoadAssetsAsync<T>(key, obj =>
            {
                retList.Add(obj);
            }).WaitForCompletion();
            return Task.FromResult(retList);
        }
        //Clean current assetlist then reload asset
        public static Task<IList<T>> ReLoadAssetsByKey<T>(this IList<T> retList, string key)
        {
            retList.Clear();
            Addressables.LoadAssetsAsync<T>(key, obj =>
            {
                retList.Add(obj);
            }).WaitForCompletion();
            return  Task.FromResult(retList);
        }
        //TO DO not make enough webgl app to create some thing like this
        #region case web GL
        //public static async UniTask<IList<T>> LoadAssetsByKeyWebGL<T>(this IList<T> retList, string key)
        //{
        //    var handle = Addressables.LoadAssetsAsync<T>(key, obj =>
        //    {
        //        retList.Add(obj);
        //    });
        //
        //    await handle.Task;
        //
        //    return retList;
        //}
        //public static async UniTask<IList<T>> ReLoadAssetsByKeyWebGL<T>(this IList<T> retList, string key)
        //{
        //    if(retList!=null && retList.Count>0) retList.Clear();
        //    var handle = Addressables.LoadAssetsAsync<T>(key, obj =>
        //    {
        //        retList.Add(obj);
        //    });
        //
        //    await handle.Task;
        //
        //    return retList;
        //}
        #endregion
    }
}

