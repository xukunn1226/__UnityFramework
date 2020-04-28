using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;
using Framework.AssetManagement.Runtime;

public static class PoolManagerExtension
{
    static private Dictionary<string, MonoPoolBase> k_PrefabPoolDict    = new Dictionary<string, MonoPoolBase>();       // [assetPath, MonoPoolBase]   instantiated prefab pool
    static private Dictionary<string, int>          k_PoolRefCountDict  = new Dictionary<string, int>();                // [assetPath, refCount]

    /// <summary>
    /// 通过待缓存对象的资源地址获取或创建对象池
    /// 释放对象池接口：RemoveMonoPool<TPool>(string assetPath)
    /// </summary>
    /// <typeparam name="TPooledObject"></typeparam>
    /// <typeparam name="TPool"></typeparam>
    /// <param name="assetPath">待缓存对象资源地址</param>
    /// <returns></returns>
    static public TPool GetOrCreatePool<TPooledObject, TPool>(string assetPath) where TPooledObject : MonoPooledObjectBase where TPool : MonoPoolBase
    {
        return PoolManager.GetOrCreatePool<TPooledObject, TPool, AssetLoaderEx>(assetPath);
    }

    /// <summary>
    /// 同上，默认使用对象池PrefabObjectPool
    /// </summary>
    /// <typeparam name="TPooledObject"></typeparam>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    static public PrefabObjectPool GetOrCreatePool<TPooledObject>(string assetPath) where TPooledObject : MonoPooledObjectBase
    {
        return PoolManager.GetOrCreatePool<TPooledObject, AssetLoaderEx>(assetPath);
    }

    /// <summary>
    /// 获取或创建对象池Prefab
    /// WARNING: 不同于其他GetOrCreatePool接口，每次获取使得对象池引用计数自增
    /// 最佳实践是不同应用环境只使用一次，把MonoPoolBase保存下来反复使用
    /// </summary>
    /// <param name="assetPath">对象池Prefab资源地址</param>
    /// <returns></returns>
    static public MonoPoolBase GetOrCreatePoolInst(string assetPath)
    {
        MonoPoolBase pool;
        if(k_PrefabPoolDict.TryGetValue(assetPath, out pool))
        {
            k_PoolRefCountDict[assetPath] = k_PoolRefCountDict[assetPath] + 1;
            return pool;
        }

        GameObject poolInst = ResourceManager.InstantiatePrefab(assetPath);
        if (poolInst == null)
            throw new System.ArgumentNullException("poolInst", $"failed to inst prefab pool: {assetPath}");

        pool = poolInst.GetComponent<MonoPoolBase>();
        if (pool == null)
            throw new System.ArgumentNullException("");

        k_PrefabPoolDict.Add(assetPath, pool);
        k_PoolRefCountDict.Add(assetPath, 1);
        return pool;
    }

    /// <summary>
    /// 释放对象池Prefab，与GetOrCreatePoolInst对应
    /// </summary>
    /// <param name="assetPath"></param>
    static public void RemoveMonoPoolInst(string assetPath)
    {
        MonoPoolBase pool;
        if(!k_PrefabPoolDict.TryGetValue(assetPath, out pool))
        {
            Debug.LogError($"RemoveMonoPool: {assetPath} deduplication!");
            return;
        }

        int refCount = k_PoolRefCountDict[assetPath] - 1;
        if(refCount > 0)
        {
            k_PoolRefCountDict[assetPath] = refCount;
        }
        else
        {
            Object.Destroy(k_PrefabPoolDict[assetPath].gameObject);
            k_PrefabPoolDict.Remove(assetPath);
            k_PoolRefCountDict.Remove(assetPath);
        }
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