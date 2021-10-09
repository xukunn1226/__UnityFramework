using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;

namespace Cache.Tests
{
    /// <summary>
    /// 对象池静态创建、动态创建、释放、查找等范例
    /// </summary>
    public class CreatePoolExample : MonoBehaviour
    {
        public GameObject PrefabAsset;
        private PrefabObjectPoolEx Pool;

        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 150, 80), "Create Pool"))
            {
                // step 1. 创建对象池

                // method 1. 创建空对象池，此时对象池还不能工作。再修改其配置，最后register mono pool
                Pool = PoolManagerEx.GetOrCreateEmptyPool();
                Pool.PreAllocateAmount = 3;
                Pool.PrefabAsset = PrefabAsset.AddComponent<RectanglePooledObject>();
                PoolManagerEx.RegisterMonoPool(Pool);

                // method 2.
                // Pool = PoolManagerEx.GetOrCreatePool<RectanglePooledObject>(PrefabAsset);

                // method 3.
                // Pool = PoolManagerEx.GetOrCreatePool(PrefabAsset);
            }

            if (GUI.Button(new Rect(100, 200, 150, 80), "Run"))
            {
                // step 2. get it
                if (Pool)
                    Pool.Get();
            }

            if (GUI.Button(new Rect(100, 300, 150, 80), "Stop"))
            {
                // step 3. remove pool

                // method 1.
                if (Pool != null)
                    PoolManagerEx.RemoveMonoPool(Pool);

                // method 2.
                //if (Pool != null)
                //    PoolManager.RemoveMonoPool<PrefabObjectPool>(Pool.PrefabAsset);
            }
        }
    }
}