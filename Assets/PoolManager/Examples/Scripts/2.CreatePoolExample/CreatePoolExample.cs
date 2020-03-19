using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CacheMech;

/// <summary>
/// 对象池静态创建、动态创建、释放、查找等范例
/// </summary>
public class CreatePoolExample : MonoBehaviour
{
    public GameObject               PrefabAsset;
    private PrefabObjectPool        Pool;

    private void OnGUI()
    {
        if(GUI.Button(new Rect(100, 100, 150, 80), "Create Pool"))
        {
            // step 1. 创建对象池

            // method 1.
            //Pool = PoolManager.GetOrCreatePool<RectanglePooledObject, PrefabObjectPool>(PrefabAsset);
            //Pool.PreAllocateAmount = 3;
            //Pool.Init();

            // method 2.
            Pool = PoolManager.GetOrCreatePool<RectanglePooledObject>(PrefabAsset);
            Pool.PreAllocateAmount = 3;
            Pool.Init();
        }

        if (GUI.Button(new Rect(100, 200, 150, 80), "Run"))
        {
            // step 2. get it
            if(Pool)
                Pool.Get();
        }

        if(GUI.Button(new Rect(100, 300, 150, 80), "Stop"))
        {
            // step 3. remove pool

            // method 1.
            if(Pool != null)
                PoolManager.RemoveMonoPool(Pool);

            // method 2.
            if(Pool != null)
                PoolManager.RemoveMonoPool<PrefabObjectPool>(Pool.PrefabAsset);
        }
    }
}
