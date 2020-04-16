using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using System;

public class ResourceManager : MonoBehaviour
{
    static internal ResourceManager Instance { get; private set; }

    private AssetManager    m_AssetManager;

    [SerializeField]
    private LoaderType      m_LoaderType;

    [SerializeField]
    private string          m_RootPath          = "Deployment/AssetBundles";

    static private bool     k_bDynamicLoad;                                                             // true: dynamic loading AssetManager; false: static loading AssetManager

    internal LoaderType     loaderType
    {
        get
        {
#if UNITY_EDITOR
            if (Instance != null)
            {
                return m_LoaderType;
            }
            return LoaderType.FromEditor;
#else
            return LoaderType.FromAB;       // 移动平台强制AB加载
#endif
        }
    }


    private void Awake()
    {
        // 已有AssetManager，则自毁
        if (FindObjectsOfType<ResourceManager>().Length > 1)
        {
            DestroyImmediate(this);
            throw new Exception("AssetManager has already exist...");
        }

        Instance = this;

        //if (!k_bDynamicLoad)
        //    InternalInit(m_LoaderType, m_RootPath);

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (loaderType == LoaderType.FromAB)
        {
            // 确保应用层持有的AssetLoader(Async)释放后再调用
            //AssetBundleManager.Uninit();
        }
        Instance = null;
    }
}
