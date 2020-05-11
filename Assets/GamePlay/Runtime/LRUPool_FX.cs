using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;
using MeshParticleSystem;

public class LRUPool_FX : LRUPoolBase
{
    static private LRUQueue<string, FX_Root> k_FXPool;

    public override int countOfUsed { get { return k_FXPool.Count; } }

    public override void Clear()
    {
        k_FXPool?.Clear();
    }

    protected override void InitLRU()
    {
        k_FXPool = new LRUQueue<string, FX_Root>(Capacity);        // 自动注册到PoolManager
        k_FXPool.OnDiscard += OnDiscard;
    }

    protected override void UninitLRU()
    {
        if (k_FXPool == null)
            throw new System.ArgumentNullException("k_FXPool");

        k_FXPool.OnDiscard -= OnDiscard;
        PoolManager.RemoveObjectPool(typeof(FX_Root));
    }

    private void OnDiscard(string assetPath, FX_Root fx)
    {
        Destroy(fx.gameObject);
    }

    public override IPooledObject Get(string assetPath)
    {
        FX_Root fx = k_FXPool.Exist(assetPath);
        if(fx == null)
        {
            GameObject go = ResourceManager.InstantiatePrefab(assetPath);
            fx = go.GetComponent<FX_Root>();
            if (fx == null)
                throw new System.ArgumentNullException("FX_Root", "no FX_Root script attached to prefab");
            fx.Pool = this;
        }

        fx.OnGet();
        k_FXPool.Cache(assetPath, fx);
        return fx;
    }

    public override void Return(IPooledObject obj)
    {
        if (obj == null)
            throw new System.ArgumentNullException("obj");

        obj.OnRelease();
    }
}
