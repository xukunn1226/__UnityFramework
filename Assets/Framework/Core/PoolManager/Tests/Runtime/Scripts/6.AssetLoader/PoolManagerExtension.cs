using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Cache.Tests
{
    public static class PoolManagerExtension
    {
        static public TPool GetOrCreatePool<TPooledObject, TPool>(string assetPath) where TPooledObject : MonoPooledObjectBase where TPool : MonoPoolBase
        {
            return PoolManager.GetOrCreatePool<TPooledObject, TPool, AssetLoader>(assetPath);
        }

        static public PrefabObjectPool GetOrCreatePool<TPooledObject>(string assetPath) where TPooledObject : MonoPooledObjectBase
        {
            return PoolManager.GetOrCreatePool<TPooledObject, AssetLoader>(assetPath);
        }
    }

    public class AssetLoader : IAssetLoader
    {
        private UnityEngine.GameObject m_Asset;

        public UnityEngine.GameObject asset { get { return m_Asset; } }

        public UnityEngine.GameObject Load(string assetPath)
        {
#if UNITY_EDITOR
            m_Asset = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(assetPath);
#endif
            return m_Asset;
        }

        public void Unload()
        {
        }
    }
}