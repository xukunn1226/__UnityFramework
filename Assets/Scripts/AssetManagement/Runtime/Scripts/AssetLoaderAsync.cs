using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AssetManagement.Runtime
{
    public class AssetLoaderAsync<T> : IEnumerator where T : UnityEngine.Object
    {
        static private ResLoaderPool<AssetLoaderAsync<T>>   m_Pool;

        private LinkedListNode<AssetBundleLoader>           m_Loader;

        private AssetBundleRequest                          m_Request;

        public T                                            asset           { get; private set; }

#if UNITY_EDITOR
        static public ResLoaderPool<AssetLoaderAsync<T>>    Pool            { get { return m_Pool; } }

        public string                                       AssetPath       { get; private set; }       // display for debug

        public AssetBundleLoader                            abLoader        { get { return m_Loader?.Value; } }
#endif

        public AssetLoaderAsync()
        {
            m_Loader = null;
            m_Request = null;
            asset = default;
        }

        static public LinkedListNode<AssetLoaderAsync<T>> Get(string assetPath)
        {
            if(m_Pool == null)
            {
                m_Pool = new ResLoaderPool<AssetLoaderAsync<T>>(AssetManager.preAllocateAssetLoaderAsyncPoolSize);
            }

            LinkedListNode<AssetLoaderAsync<T>> loader = m_Pool.Get();
            loader.Value.LoadAsset(assetPath);
            return loader;
        }

        static public LinkedListNode<AssetLoaderAsync<T>> Get(string assetBundleName, string assetName)
        {
            if (m_Pool == null)
            {
                m_Pool = new ResLoaderPool<AssetLoaderAsync<T>>(AssetManager.preAllocateAssetLoaderAsyncPoolSize);
            }

            LinkedListNode<AssetLoaderAsync<T>> loader = m_Pool.Get();
            loader.Value.LoadAsset(assetBundleName, assetName);
            return loader;
        }

        static public void Release(LinkedListNode<AssetLoaderAsync<T>> loader)
        {
            if (m_Pool == null || loader == null)
                return;

            loader.Value.Unload();
            m_Pool.Release(loader);
        }
        
        private void LoadAsset(string assetPath)
        {
#if UNITY_EDITOR
            switch (AssetManager.loaderType)
            {
                case LoaderType.FromEditor:
                    asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                    AssetPath = assetPath;
                    break;
                case LoaderType.FromAB:
                    LoadAssetInternal(assetPath);
                    AssetPath = assetPath;
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
                        AssetPath = assetBundleName + "/" + assetName;
                    }
                    break;
                case LoaderType.FromAB:
                    LoadAssetInternal(assetBundleName, assetName);
                    AssetPath = assetBundleName.Replace(".ab", "") + "/" + assetName;
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
            m_Loader = AssetBundleLoader.Get(assetBundleName);
            if (m_Loader.Value.assetBundle != null)
            {
                m_Request = m_Loader.Value.assetBundle.LoadAssetAsync<T>(assetName);
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
            if (m_Loader != null)
            {
                AssetBundleLoader.Release(m_Loader);
                m_Loader = null;
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
    }
}