﻿using UnityEngine;
using Framework.Cache;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.AssetManagement.Runtime
{
    public class PrefabLoader : ILinkedObjectPoolNode<PrefabLoader>, IPooledObject
    {
        static private LinkedObjectPool<PrefabLoader>     m_Pool;

        public static LinkedObjectPool<PrefabLoader>      kPool   { get { return m_Pool; } }

        public AssetBundleLoader        abLoader    { get; private set; }

        public GameObject               asset       { get; private set; }

#if UNITY_EDITOR
        public string                   assetPath;
#endif

        public PrefabLoader()
        {
            abLoader = null;
            asset = null;
        }

        static internal GameObject Get(string assetPath)
        {
            if (m_Pool == null)
            {
                m_Pool = new LinkedObjectPool<PrefabLoader>(AssetManager.PreAllocateAssetLoaderPoolSize);
            }

            PrefabLoader loader = (PrefabLoader)m_Pool.Get();
            loader.LoadAsset(assetPath);
            loader.Pool = m_Pool;
            return loader.asset;
        }
        
        static internal GameObject Get(string assetBundleName, string assetName)
        {
            if (m_Pool == null)
            {
                m_Pool = new LinkedObjectPool<PrefabLoader>(AssetManager.PreAllocateAssetLoaderPoolSize);
            }

            PrefabLoader loader = (PrefabLoader)m_Pool.Get();
            loader.LoadAsset(assetBundleName, assetName);
            loader.Pool = m_Pool;
            return loader.asset;
        }

        static internal void Release(PrefabLoader loader)
        {
            if (m_Pool == null || loader == null)
                throw new System.ArgumentNullException();

            m_Pool.Return(loader);
        }

        private void LoadAsset(string assetPath)
        {
#if UNITY_EDITOR
            switch (AssetManager.Instance.loaderType)
            {
                case LoaderType.FromEditor:
                    GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if(prefabAsset != null)
                    {
                        asset = Object.Instantiate(prefabAsset);
                    }
                    this.assetPath = assetPath;
                    break;
                case LoaderType.FromStreamingAssets:
                case LoaderType.FromPersistent:
                    LoadAssetInternal(assetPath);
                    this.assetPath = assetPath;
                    break;
            }
#else
            LoadAssetInternal(assetPath);
#endif
            if(asset != null)
            {
                PrefabDestroyer destroyer = asset.AddComponent<PrefabDestroyer>();
                destroyer.loader = this;
            }
        }

        private void LoadAsset(string assetBundleName, string assetName)
        {
#if UNITY_EDITOR
            AssetManager.ParseBundleAndAssetName(assetBundleName, assetName, out assetPath);
            switch (AssetManager.Instance.loaderType)
            {
                case LoaderType.FromEditor:
                    GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if(prefabAsset != null)
                    {
                        asset = Object.Instantiate(prefabAsset);
                    }
                    break;
                case LoaderType.FromStreamingAssets:
                case LoaderType.FromPersistent:
                    LoadAssetInternal(assetBundleName, assetName);
                    break;
            }
#else
            LoadAssetInternal(assetBundleName, assetName);
#endif
            if(asset != null)
            {
                PrefabDestroyer destroyer = asset.AddComponent<PrefabDestroyer>();
                destroyer.loader = this;
            }
        }

        private void LoadAssetInternal(string assetPath)
        {
            string assetBundleName, assetName;
            if (!AssetManager.ParseAssetPath(assetPath, out assetBundleName, out assetName))
            {
                Debug.LogWarningFormat("AssetLoader -- Failed to reslove assetbundle name: {0} {1}", assetPath, typeof(GameObject));
                return;
            }

            LoadAssetInternal(assetBundleName, assetName);
        }

        private void LoadAssetInternal(string assetBundleName, string assetName)
        {
            GameObject prefabAsset = null;

            abLoader = AssetBundleLoader.Get(assetBundleName);
            if (abLoader.assetBundle != null)
            {
                prefabAsset = abLoader.assetBundle.LoadAsset<GameObject>(assetName);
            }

            if(prefabAsset != null)
            {
                asset = Object.Instantiate(prefabAsset);
            }
            else
            {
                Unload();
            }
        }

        private void Unload()
        {
            if(asset != null)
            {
                Object.Destroy(asset);
                asset = null;
            }

            if (abLoader != null)
            {
                AssetBundleLoader.Release(abLoader);
                abLoader = null;
            }
        }

        LinkedObjectPool<PrefabLoader>        ILinkedObjectPoolNode<PrefabLoader>.List      { get; set; }

        ILinkedObjectPoolNode<PrefabLoader>   ILinkedObjectPoolNode<PrefabLoader>.Next      { get; set; }

        ILinkedObjectPoolNode<PrefabLoader>   ILinkedObjectPoolNode<PrefabLoader>.Prev      { get; set; }

        void IPooledObject.OnInit() { }

        /// <summary>
        /// 从对象池中拿出时的回调
        /// </summary>
        void IPooledObject.OnGet() { }

        /// <summary>
        /// 放回对象池时的回调
        /// </summary>
        void IPooledObject.OnRelease()
        {
            Unload();
            Pool = null;
        }

        /// <summary>
        /// 放回对象池
        /// </summary>
        void IPooledObject.ReturnToPool()
        {
            Pool?.Return(this);
        }

        public IPool Pool { get; set; }
    }
}