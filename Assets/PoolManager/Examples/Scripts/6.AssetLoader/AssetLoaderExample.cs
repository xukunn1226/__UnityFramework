using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cache;

/// <summary>
/// 展示如何通过assetPath使用缓存对象
/// </summary>
public class AssetLoaderExample : MonoBehaviour
{
    public string assetPath;

    private LivingPrefabObjectPool Pool;

    private void OnGUI()
    {
        if (GUI.Button(new Rect(100, 100, 150, 80), "Create Pool"))
        {
            // step 1. 创建对象池
            Pool = PoolManager.GetOrCreatePool<Decal, LivingPrefabObjectPool>(assetPath);

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
