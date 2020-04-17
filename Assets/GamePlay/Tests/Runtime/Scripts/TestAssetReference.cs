using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;
using Cache.Tests;
using Framework.AssetManagement.Runtime;

/// <summary>
/// 展示如何通过assetPath使用缓存对象
/// </summary>
public class TestAssetReference : MonoBehaviour
{
    public LoaderType m_LoaderType;

    public string assetPath;

    private LivingPrefabObjectPool Pool;


    private void Awake()
    {
        ResourceManager.Init(m_LoaderType);
    }

    private void OnDestroy()
    {
        ResourceManager.Uninit();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(100, 100, 150, 80), "Create Pool"))
        {
            // step 1. 创建对象池
            Pool = PoolManagerExtension.GetOrCreatePool<Decal, LivingPrefabObjectPool>(assetPath);

            Pool.NormalSpeedLimitAmount = 5;
            Pool.AmplifiedSpeed = 3;
            Pool.PreAllocateAmount = 3;
            Pool.Init();
        }

        if (GUI.Button(new Rect(100, 200, 150, 80), "Run"))
        {
            // step 2. get it
            Spawn();
        }

        if (GUI.Button(new Rect(100, 300, 150, 80), "Stop"))
        {
            // step 3. remove pool
            PoolManager.RemoveMonoPool<LivingPrefabObjectPool>(assetPath);
        }
    }

    private void Spawn()
    {
        if (Pool == null)
            return;

        Decal d = (Decal)Pool.Get();
        d.transform.position = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
    }
}