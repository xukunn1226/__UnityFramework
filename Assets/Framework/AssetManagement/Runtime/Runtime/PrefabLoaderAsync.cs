using System.Collections;
using UnityEngine;
using Framework.Cache;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.AssetManagement.Runtime
{
    public class PrefabLoaderAsync : IEnumerator, ILinkedObjectPoolNode<PrefabLoaderAsync>, IPooledObject
    {
        static private LinkedObjectPool<PrefabLoaderAsync>    m_Pool;
        public static LinkedObjectPool<PrefabLoaderAsync>     kPool { get { return m_Pool; } }

        public AssetBundleLoader        abLoader    { get; private set; }

        private AssetBundleRequest      m_Request;

        public GameObject               asset       { get; private set; }

#if UNITY_EDITOR
        public string                   assetPath;
        private bool                    m_LoadFromEditor;
#endif

        public PrefabLoaderAsync()
        {
            abLoader = null;
            m_Request = null;
            asset = null;
        }

        static internal PrefabLoaderAsync Get(string assetPath)
        {
            if (m_Pool == null)
            {
                m_Pool = new LinkedObjectPool<PrefabLoaderAsync>(AssetManager.PreAllocateAssetLoaderAsyncPoolSize);
            }

            PrefabLoaderAsync loader = (PrefabLoaderAsync)m_Pool.Get();
            loader.LoadAsset(assetPath);
            loader.Pool = m_Pool;
            return loader;
        }

        static internal PrefabLoaderAsync Get(string assetBundleName, string assetName)
        {
            if (m_Pool == null)
            {
                m_Pool = new LinkedObjectPool<PrefabLoaderAsync>(AssetManager.PreAllocateAssetLoaderAsyncPoolSize);
            }

            PrefabLoaderAsync loader = (PrefabLoaderAsync)m_Pool.Get();
            loader.LoadAsset(assetBundleName, assetName);
            loader.Pool = m_Pool;
            return loader;
        }

        static internal void Release(PrefabLoaderAsync loader)
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
                    GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);       // hack: 编辑模式
                    if(prefabAsset != null)
                    {
                        asset = Object.Instantiate(prefabAsset);
                        m_LoadFromEditor = true;

                        if(asset != null)
                        {
                            PrefabDestroyer destroyer = asset.AddComponent<PrefabDestroyer>();
                            destroyer.loaderAsync = this;
                        }
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
        }
        
        private void LoadAsset(string assetBundleName, string assetName)
        {
#if UNITY_EDITOR
            AssetManager.ParseBundleAndAssetName(assetBundleName, assetName, out assetPath);
            switch (AssetManager.Instance.loaderType)
            {
                case LoaderType.FromEditor:
                    GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (prefabAsset != null)
                    {
                        asset = Object.Instantiate(prefabAsset);
                        m_LoadFromEditor = true;

                        if(asset != null)
                        {
                            PrefabDestroyer destroyer = asset.AddComponent<PrefabDestroyer>();
                            destroyer.loaderAsync = this;
                        }
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
            abLoader = AssetBundleLoader.Get(assetBundleName);
            if (abLoader.assetBundle != null)
            {
                m_Request = abLoader.assetBundle.LoadAssetAsync<GameObject>(assetName);
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
            m_Request = null;
#if UNITY_EDITOR
            m_LoadFromEditor = false;
#endif
        }

        private bool IsDone()
        {
#if UNITY_EDITOR
            if(AssetManager.Instance.loaderType == LoaderType.FromEditor)
            {
                return true;                // 编辑模式直接返回，LoadAssetAtPath之后立即实例化
            }
#endif
            // Debug.Log($"{Time.frameCount}   GameLoaderAsync:IsDone  Update");
            if (m_Request == null)
            { // bundle加载失败，释放所有bundle
                Unload();
                return true;
            }

            if (m_Request.isDone)
            {
                // Debug.Log($"{Time.frameCount}   GameLoaderAsync:IsDone  TRUE");
                if(m_Request.asset != null && m_Request.asset is GameObject)
                {
                    asset = Object.Instantiate(m_Request.asset) as GameObject;
                    if(asset != null)
                    {
                        PrefabDestroyer destroyer = asset.AddComponent<PrefabDestroyer>();
                        destroyer.loaderAsync = this;
                    }
                }
                else
                { // asset加载失败
                    Unload();
                    return true;
                }
            }
            return m_Request.isDone;
        }

        object IEnumerator.Current
        {
            get { return asset; }
        }

        bool IEnumerator.MoveNext()
        {
#if UNITY_EDITOR
            return !m_LoadFromEditor && !IsDone();
#else
            return !IsDone();
#endif            
        }

        void IEnumerator.Reset()
        {
        }

        LinkedObjectPool<PrefabLoaderAsync>       ILinkedObjectPoolNode<PrefabLoaderAsync>.List     { get; set; }

        ILinkedObjectPoolNode<PrefabLoaderAsync>  ILinkedObjectPoolNode<PrefabLoaderAsync>.Next     { get; set; }

        ILinkedObjectPoolNode<PrefabLoaderAsync>  ILinkedObjectPoolNode<PrefabLoaderAsync>.Prev     { get; set; }

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