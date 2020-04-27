using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;
using Framework.AssetManagement.Runtime;

public static class PoolManagerExtension
{
    static public TPool GetOrCreatePool<TPooledObject, TPool>(string assetPath) where TPooledObject : MonoPooledObjectBase where TPool : MonoPoolBase
    {
        return PoolManager.GetOrCreatePool<TPooledObject, TPool, AssetLoaderEx>(assetPath);
    }

    static public PrefabObjectPool GetOrCreatePool<TPooledObject>(string assetPath) where TPooledObject : MonoPooledObjectBase
    {
        return PoolManager.GetOrCreatePool<TPooledObject, AssetLoaderEx>(assetPath);
    }
}

public class AssetLoaderEx : IAssetLoader
{
    private AssetLoader<GameObject> m_Loader;

    public GameObject asset
    {
        get
        {
            return m_Loader?.asset;
        }
    }

    public GameObject Load(string assetPath)
    {
        m_Loader = ResourceManager.LoadAsset<GameObject>(assetPath);
        return m_Loader?.asset;
    }

    public void Unload()
    {
        ResourceManager.UnloadAsset(m_Loader);
    }
}