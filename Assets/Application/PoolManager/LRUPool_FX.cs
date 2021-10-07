using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;
using MeshParticleSystem;
using Framework.AssetManagement.Runtime;

namespace Application.Runtime
{
// 不足：以assetPath为KEY，所以同一资源不能有多个实例
// 改善：以isntanceID为KEY可以解决上述问题
public class LRUPool_FX : LRUPoolBase
{
    static private LRUQueue<string, FX_Root>            s_FXPool;
    static private Dictionary<string, GameObjectLoader> s_LoaderPool = new Dictionary<string, GameObjectLoader>();      // <assetPath, GameObjectLoader>

    public override int countOfUsed { get { return s_FXPool.Count; } }

    public override void Clear()
    {
        s_FXPool?.Clear();
    }

    protected override void InitLRU()
    {
        s_FXPool = new LRUQueue<string, FX_Root>(Capacity);        // 自动注册到PoolManager
        s_FXPool.OnDiscard += OnDiscard;
    }

    protected override void UninitLRU()
    {
        if (s_FXPool == null)
            throw new System.ArgumentNullException("k_FXPool");

        s_FXPool.OnDiscard -= OnDiscard;
        PoolManager.RemoveObjectPool(typeof(FX_Root));
    }

    private void OnDiscard(string assetPath, FX_Root fx)
    {
        Destroy(fx.gameObject);

        GameObjectLoader loader;
        if(s_LoaderPool.TryGetValue(assetPath, out loader))
        {
            s_LoaderPool.Remove(assetPath);
            AssetManager.ReleaseInst(loader);
        }
    }

    public override IPooledObject Get(string assetPath)
    {
        // FX_Root fx = s_FXPool.Exist(assetPath);
        // if(fx == null)
        // {
        //     GameObjectLoader loader = ResourceManager.Instantiate(assetPath);
        //     if(loader.asset == null)
        //     {
        //         return null;
        //     }
        //     fx = loader.asset.GetComponent<FX_Root>();
        //     if (fx == null)
        //         throw new System.ArgumentNullException("FX_Root", "no FX_Root script attached to prefab");
        //     fx.Pool = this;

        //     s_LoaderPool.Add(assetPath, loader);
        // }

        // fx.OnGet();
        // s_FXPool.Cache(assetPath, fx);        
        // return fx;
        throw new System.NotImplementedException();
    }
    
    public override IPooledObject Get(string bundleName, string assetName)
    {
        FX_Root fx = s_FXPool.Exist(assetName);
        if(fx == null)
        {
            GameObjectLoader loader = AssetManager.Instantiate(bundleName, assetName);
            if(loader.asset == null)
            {
                return null;
            }
            fx = loader.asset.GetComponent<FX_Root>();
            if (fx == null)
                throw new System.ArgumentNullException("FX_Root", "no FX_Root script attached to prefab");
            fx.Pool = this;

            s_LoaderPool.Add(assetName, loader);
        }

        fx.OnGet();
        s_FXPool.Cache(assetName, fx);        
        return fx;
    }

    public override void Return(IPooledObject obj)
    {
        if (obj == null)
            throw new System.ArgumentNullException("obj");

        obj.OnRelease();
    }
}
}