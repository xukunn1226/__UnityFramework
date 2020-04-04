using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cache;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AssetManagement.Runtime
{
    public class AssetLoader<T> : IBetterLinkedListNode<AssetLoader<T>>, IPooledObject where T : UnityEngine.Object
    {
        static private LinkedObjectPool<AssetLoader<T>> m_Pool;


        public AssetBundleLoader      abLoader    { get; private set; }

        public                      T   asset       { get; private set; }

#if UNITY_EDITOR
        public                  string  assetPath   { get; private set; }       // display for debug

        public static LinkedObjectPool<AssetLoader<T>> kPool { get { return m_Pool; } }
#endif

        public AssetLoader()
        {
            abLoader = null;
            asset = null;
        }

        static internal AssetLoader<T> Get(string assetPath)
        {
            if (m_Pool == null)
            {
                m_Pool = new LinkedObjectPool<AssetLoader<T>>(AssetManager.PreAllocateAssetLoaderPoolSize);
            }

            AssetLoader<T> loader = (AssetLoader<T>)m_Pool.Get();
            loader.LoadAsset(assetPath);
            loader.Pool = m_Pool;
            return loader;
        }

        static internal AssetLoader<T> Get(string assetBundleName, string assetName)
        {
            if (m_Pool == null)
            {
                m_Pool = new LinkedObjectPool<AssetLoader<T>>(AssetManager.PreAllocateAssetLoaderPoolSize);
            }

            AssetLoader<T> loader = (AssetLoader<T>)m_Pool.Get();
            loader.LoadAsset(assetBundleName, assetName);
            loader.Pool = m_Pool;
            return loader;
        }

        static public void Release(AssetLoader<T> loader)
        {
            if (m_Pool == null || loader == null)
                return;

            //loader.Unload();
            m_Pool.Return(loader);
        }

        private void LoadAsset(string assetPath)
        {
#if UNITY_EDITOR
            switch (AssetManager.loaderType)
            {
                case LoaderType.FromEditor:
                    asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                    this.assetPath = assetPath;
                    break;
                case LoaderType.FromAB:
                    LoadAssetInternal(assetPath);
                    this.assetPath = assetPath;
                    break;
            }
#else
            LoadAssetInternal(assetPath);
#endif
        }

        private void LoadAsset(string assetBundleName, string assetName)
        {
#if UNITY_EDITOR
            switch (AssetManager.loaderType)
            {
                case LoaderType.FromEditor:
                    {
                        if (assetBundleName.EndsWith(".ab", System.StringComparison.OrdinalIgnoreCase))
                        {
                            assetBundleName = assetBundleName.Substring(0, assetBundleName.Length - 3);
                        }
                        asset = AssetDatabase.LoadAssetAtPath<T>(assetBundleName + "/" + assetName);
                        assetPath = assetBundleName + "/" + assetName;
                    }
                    break;
                case LoaderType.FromAB:
                    LoadAssetInternal(assetBundleName, assetName);
                    assetPath = assetBundleName.Replace(".ab", "") + "/" + assetName;
                    break;
            }
#else
            LoadAssetInternal(assetBundleName, assetName);
#endif
        }

        private void LoadAssetInternal(string assetPath)
        {
            string assetBundleName, assetName;
            if (!AssetManager.ParseAssetPath(assetPath, out assetBundleName, out assetName))
            {
                Debug.LogWarningFormat("AssetLoader -- Failed to reslove assetbundle name: {0} {1}", assetPath, typeof(T));
                return;
            }

            LoadAssetInternal(assetBundleName, assetName);
        }

        private void LoadAssetInternal(string assetBundleName, string assetName)
        {
            abLoader = AssetBundleLoader.Get(assetBundleName);
            if (abLoader.assetBundle != null)
            {
                asset = abLoader.assetBundle.LoadAsset<T>(assetName);
                if (asset == null)
                {
                    Unload();           // asset加载失败则释放所有的AB包
                }
            }
            else
            {
                Unload();               // asset bundle加载失败则释放所有的AB包
            }
        }

        private void Unload()
        {
            if (abLoader != null)
            {
                AssetBundleLoader.Release(abLoader);
                abLoader = null;
            }
            asset = null;
        }

        public LinkedObjectPool<AssetLoader<T>>        List    { get; set; }

        public IBetterLinkedListNode<AssetLoader<T>>   Next    { get; set; }

        public IBetterLinkedListNode<AssetLoader<T>>   Prev    { get; set; }

        public void OnInit() { }

        /// <summary>
        /// 从对象池中拿出时的回调
        /// </summary>
        public void OnGet() { }

        /// <summary>
        /// 放回对象池时的回调
        /// </summary>
        public void OnRelease()
        {
            Unload();
            Pool = null;
        }

        /// <summary>
        /// 放回对象池
        /// </summary>
        public void ReturnToPool()
        {
            Pool?.Return(this);
        }

        public IPool Pool { get; set; }
    }
}