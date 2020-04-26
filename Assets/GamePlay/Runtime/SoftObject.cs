using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using Framework.AssetManagement.Runtime;

public class SoftObject : SoftObjectPath
{
    private AssetLoader<UnityEngine.Object>         m_Loader;
    private AssetLoaderAsync<UnityEngine.Object>    m_LoaderAsync;

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

        m_Loader = ResourceManager.LoadAsset<UnityEngine.Object>(assetPath);
        return m_Loader.asset;
    }

    public AssetLoaderAsync<UnityEngine.Object> LoadAssetAsync()
    {
        // 已异步加载，不能再次加载
        if (m_LoaderAsync != null)
            throw new System.InvalidOperationException($"{assetPath} has already loaded async");

        // 已同步加载，不能再次加载
        if (m_Loader != null)
            throw new System.InvalidOperationException($"{assetPath} has already loaded, plz unload it");

        m_LoaderAsync = ResourceManager.LoadAssetAsync<UnityEngine.Object>(assetPath);
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
}
