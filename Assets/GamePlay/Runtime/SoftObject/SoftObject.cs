using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using Framework.AssetManagement.Runtime;
using Framework.Cache;
using Object = UnityEngine.Object;

public sealed class SoftObject : SoftObjectPath
{
    private AssetLoader<Object>         m_Loader;
    private AssetLoaderAsync<Object>    m_LoaderAsync;

    private MonoPoolBase                m_PoolScripted;         // 脚本创建的对象池
    private MonoPoolBase                m_PoolPrefabed;

    public GameObject Instantiate()
    {
        return ResourceManager.InstantiatePrefab(assetPath);
    }

    public IEnumerator InstantiateAsync(System.Action<GameObject> handler = null)
    {
        return ResourceManager.InstantiatePrefabAsync(assetPath, handler);
    }

    public void ReleaseInst(GameObject inst)
    {
        if (inst == null)
            throw new System.ArgumentNullException("inst");

        Destroy(inst);
    }

    public UnityEngine.Object LoadAsset()
    {
        // 已同步加载，不能再次加载
        if (m_Loader != null)
            throw new System.InvalidOperationException($"{assetPath} has already loaded, plz unload it");

        // 已异步加载，不能再次加载
        if (m_LoaderAsync != null)
            throw new System.InvalidOperationException($"{assetPath} has already loaded async");

        m_Loader = ResourceManager.LoadAsset<Object>(assetPath);
        return m_Loader.asset;
    }

    public AssetLoaderAsync<Object> LoadAssetAsync()
    {
        // 已异步加载，不能再次加载
        if (m_LoaderAsync != null)
            throw new System.InvalidOperationException($"{assetPath} has already loaded async");

        // 已同步加载，不能再次加载
        if (m_Loader != null)
            throw new System.InvalidOperationException($"{assetPath} has already loaded, plz unload it");

        m_LoaderAsync = ResourceManager.LoadAssetAsync<Object>(assetPath);
        return m_LoaderAsync;
    }

    public void UnloadAsset()
    {
        // 资源不能以两种方式同时加载
        if (m_Loader != null && m_LoaderAsync != null)
            throw new System.Exception("m_Loader != null && m_LoaderAsync != null");

        if(m_Loader != null)
        {
            ResourceManager.UnloadAsset(m_Loader);
            m_Loader = null;
        }

        if (m_LoaderAsync != null)
        {
            ResourceManager.UnloadAsset(m_LoaderAsync);
            m_LoaderAsync = null;
        }
    }

    /// <summary>
    /// 从对象池中创建对象（对象池由脚本创建）
    /// NOTE: 对象池的创建不够友好，无法设置对象池参数，建议使用SpawnFromPrefabedPool接口，把对象池制作为Prefab，然后配置池参数
    /// </summary>
    /// <typeparam name="TPooledObject"></typeparam>
    /// <typeparam name="TPool"></typeparam>
    /// <returns></returns>
    public IPooledObject SpawnFromPool<TPooledObject, TPool>() where TPooledObject : MonoPooledObjectBase where TPool : MonoPoolBase
    {
        if (m_PoolScripted == null)
        {
            m_PoolScripted = PoolManagerExtension.GetOrCreatePool<TPooledObject, TPool>(assetPath);
            m_PoolScripted.Warmup();
        }
        return m_PoolScripted.Get();
    }

    /// <summary>
    /// 销毁对象池，与SpawnFromPool对应
    /// </summary>
    /// <typeparam name="TPool"></typeparam>
    public void DestroyPool<TPool>() where TPool : MonoPoolBase
    {
        if (m_PoolScripted == null)
            throw new System.ArgumentNullException("Pool", "Scripted Pool not initialize");

        PoolManager.RemoveMonoPool<TPool>(assetPath);
    }

    /// <summary>
    /// 从对象池中创建对象（对象池制作成Prefab）
    /// </summary>
    /// <returns></returns>
    public IPooledObject SpawnFromPrefabedPool()
    {
        if(m_PoolPrefabed == null)
        {
            m_PoolPrefabed = PoolManagerExtension.GetOrCreatePoolInst(assetPath);
        }
        return m_PoolPrefabed.Get();
    }

    /// <summary>
    /// 销毁对象池，与SpawnFromPrefabedPool对应
    /// </summary>
    public void DestroyPrefabedPool()
    {
        if (m_PoolPrefabed == null)
            throw new System.ArgumentNullException("Pool", "Prefabed Pool not initialize");

        PoolManagerExtension.RemoveMonoPoolInst(assetPath);
    }
}
