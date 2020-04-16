using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;

public static class PoolManagerExtension
{
    static public TPool GetOrCreatePool<TPooledObject, TPool>(string assetPath) where TPooledObject : MonoPooledObjectBase where TPool : MonoPoolBase
    {
        return PoolManager.GetOrCreatePool<TPooledObject, TPool, AssetReference>(assetPath);
    }

    static public PrefabObjectPool GetOrCreatePool<TPooledObject>(string assetPath) where TPooledObject : MonoPooledObjectBase
    {
        return PoolManager.GetOrCreatePool<TPooledObject, AssetReference>(assetPath);
    }
}