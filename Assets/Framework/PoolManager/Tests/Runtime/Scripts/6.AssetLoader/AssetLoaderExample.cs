using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;

namespace Cache.Tests
{
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
                // method 1.
                // Pool = (LivingPrefabObjectPool)PoolManagerEx.GetOrCreatePool<Decal, LivingPrefabObjectPool>(assetPath);

                // method 2.
                Pool = PoolManager.BeginCreateEmptyPool<LivingPrefabObjectPool>();
                Pool.NormalSpeedLimitAmount = 5;
                Pool.AmplifiedSpeed = 3;
                Pool.PreAllocateAmount = 3;
                Pool = (LivingPrefabObjectPool)PoolManager.EndCreateEmptyPool<Decal>(Pool, assetPath);
            }

            if (GUI.Button(new Rect(100, 200, 150, 80), "Run"))
            {
                // step 2. get it
                Spawn();
            }

            if (GUI.Button(new Rect(100, 300, 150, 80), "Stop"))
            {
                // step 3. remove pool
                PoolManager.RemoveMonoPool(assetPath);
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
}