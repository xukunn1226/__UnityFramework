﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;
using Framework.AssetManagement.Runtime;

/// <summary>
/// 把对象池製作Prefab的幾個優勢：
/// 1、配置Pool參數
/// 2、动态加载
/// </summary>
public class TestPrefabPool : MonoBehaviour
{
    public string assetPath;

    private GameObject PoolInst;

    private void OnGUI()
    {
        if (GUI.Button(new Rect(100, 100, 150, 80), "Create Pool"))
        {
            // step 1. 创建对象池
            PoolInst = ResourceManager.InstantiatePrefab(assetPath);
        }

        if (GUI.Button(new Rect(100, 200, 150, 80), "Run"))
        {
            // step 2. get it
            Spawn();
        }

        if (GUI.Button(new Rect(100, 300, 150, 80), "Stop"))
        {
            // step 3. remove pool
            if (PoolInst != null)
                Destroy(PoolInst);
        }
    }

    private void Spawn()
    {
        if (PoolInst == null)
            return;

        MonoPoolBase Pool = PoolInst.GetComponent<MonoPoolBase>();
        if (Pool == null)
            return;

        Decal d = (Decal)Pool.Get();
        d.transform.position = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 8);
    }
}