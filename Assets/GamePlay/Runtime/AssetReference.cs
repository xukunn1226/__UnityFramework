using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;
using Framework.AssetManagement.Runtime;

public class AssetReference : IAssetLoader
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
