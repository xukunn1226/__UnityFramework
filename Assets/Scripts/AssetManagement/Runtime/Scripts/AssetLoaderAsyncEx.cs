using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cache;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AssetManagement.Runtime
{
    public class AssetLoaderAsyncEx<T> : IEnumerator, IBetterLinkedListNode<AssetLoaderAsyncEx<T>>, IPooledObject where T : UnityEngine.Object
    {
        static private LinkedObjectPool<AssetLoaderAsyncEx<T>> m_Pool;

        public AssetBundleLoaderEx      abLoader    { get; private set; }

        private AssetBundleRequest      m_Request;

        public                  T       asset       { get; private set; }

#if UNITY_EDITOR
        public              string      assetPath   { get; private set; }       // display for debug

        public static LinkedObjectPool<AssetLoaderAsyncEx<T>> kPool { get { return m_Pool; } }
#endif

        public AssetLoaderAsyncEx()
        {
            abLoader = null;
            m_Request = null;
            asset = default;
        }

        static internal AssetLoaderAsyncEx<T> Get(string assetPath)
        {
            if (m_Pool == null)
            {
                m_Pool = new LinkedObjectPool<AssetLoaderAsyncEx<T>>(AssetManagerEx.PreAllocateAssetLoaderAsyncPoolSize);
            }

            AssetLoaderAsyncEx<T> loader = (AssetLoaderAsyncEx<T>)m_Pool.Get();
            loader.LoadAsset(assetPath);
            loader.Pool = m_Pool;
            return loader;
        }

        static internal AssetLoaderAsyncEx<T> Get(string assetBundleName, string assetName)
        {
            if (m_Pool == null)
            {
                m_Pool = new LinkedObjectPool<AssetLoaderAsyncEx<T>>(AssetManagerEx.PreAllocateAssetLoaderAsyncPoolSize);
            }

            AssetLoaderAsyncEx<T> loader = (AssetLoaderAsyncEx<T>)m_Pool.Get();
            loader.LoadAsset(assetBundleName, assetName);
            loader.Pool = m_Pool;
            return loader;
        }

        static internal void Release(AssetLoaderAsyncEx<T> loader)
        {
            if (m_Pool == null || loader == null)
                return;

            //loader.Unload();
            m_Pool.Return(loader);
        }

        private void LoadAsset(string assetPath)
        {
#if UNITY_EDITOR
            switch (AssetManagerEx.loaderType)
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
            switch (AssetManagerEx.loaderType)
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
            if (!AssetManagerEx.ParseAssetPath(assetPath, out assetBundleName, out assetName))
            {
                Debug.LogWarningFormat("AssetLoader -- Failed to reslove assetbundle name: {0} {1}", assetPath, typeof(T));
                return;
            }

            LoadAssetInternal(assetBundleName, assetName);
        }

        private void LoadAssetInternal(string assetBundleName, string assetName)
        {
            abLoader = AssetBundleLoaderEx.Get(assetBundleName);
            if (abLoader.assetBundle != null)
            {
                m_Request = abLoader.assetBundle.LoadAssetAsync<T>(assetName);
                if (m_Request == null)
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
                AssetBundleLoaderEx.Release(abLoader);
                abLoader = null;
            }
            m_Request = null;
            asset = null;
        }

        private bool IsDone()
        {
            if (m_Request == null)
                return true;

            if (m_Request.isDone)
                asset = m_Request.asset as T;
            return m_Request.isDone;
        }

        public object Current
        {
            get { return asset; }
        }

        public bool MoveNext()
        {
            return !IsDone();
        }

        public void Reset()
        {
        }

        public LinkedObjectPool<AssetLoaderAsyncEx<T>>          List    { get; set; }

        public IBetterLinkedListNode<AssetLoaderAsyncEx<T>>     Next    { get; set; }

        public IBetterLinkedListNode<AssetLoaderAsyncEx<T>>     Prev    { get; set; }

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

        public IPool Pool { get; set; }    }
}