using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;
using Framework.AssetManagement.Runtime;

namespace Application.Runtime
{
    public static class PoolManagerExtension
    {
        /// <summary>
        /// 通过待缓存对象的资源地址获取或创建对象池
        /// 释放对象池接口：RemoveMonoPool<TPool>(string assetPath)
        /// </summary>
        /// <typeparam name="TPooledObject"></typeparam>
        /// <typeparam name="TPool"></typeparam>
        /// <param name="assetPath">待缓存对象资源地址</param>
        /// <returns></returns>
        static public TPool GetOrCreatePool<TPooledObject, TPool>(string assetPath) where TPooledObject : MonoPooledObject where TPool : MonoPoolBase
        {
            return PoolManager.GetOrCreatePool<TPooledObject, TPool, AssetLoader>(assetPath);
        }

        /// <summary>
        /// 同上，默认使用对象池PrefabObjectPool
        /// </summary>
        /// <typeparam name="TPooledObject"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        static public PrefabObjectPool GetOrCreatePool<TPooledObject>(string assetPath) where TPooledObject : MonoPooledObject
        {
            return PoolManager.GetOrCreatePool<TPooledObject, AssetLoader>(assetPath);
        }
    }

    public class AssetLoader : IAssetLoader
    {
        private AssetLoader<GameObject> m_Loader;

        public GameObject asset
        {
            get
            {
                return m_Loader?.asset;
            }
        }

        public GameObject Load(string assetPath)
        {
            m_Loader = AssetManager.LoadAsset<GameObject>(assetPath);
            return m_Loader?.asset;
        }

        public void Unload()
        {
            AssetManager.UnloadAsset(m_Loader);
        }
    }
}