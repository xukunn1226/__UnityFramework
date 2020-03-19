using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public partial class PoolManager : MonoBehaviour
    {
        //static private Dictionary<string, AssetLoader> m_kAssetLoaderDict = new Dictionary<string, AssetLoader>();

        //static public PrefabObjectPool GetOrCreatePool<T>(string assetPath) where T : MonoPooledObjectBase
        //{
        //    AssetLoader loader;
        //    if(m_kAssetLoaderDict.TryGetValue(assetPath, out loader))
        //    {
        //        return GetOrCreatePool(loader.asset);
        //    }

        //    AssetLoader loader = new AssetLoader(assetPath);
        //    GameObject prefabAsset = loader.asset as GameObject;
        //    return GetOrCreatePool<T, PrefabObjectPool>(prefabAsset);
        //}
    }
}