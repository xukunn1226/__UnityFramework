using System.Collections;
using UnityEngine;
using Framework.Cache;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.AssetManagement.Runtime
{
    public class AssetLoaderAsync<T> : IEnumerator, ILinkedObjectPoolNode<AssetLoaderAsync<T>>, IPooledObject where T : UnityEngine.Object
    {
        static private LinkedObjectPool<AssetLoaderAsync<T>>    m_Pool;

        public static LinkedObjectPool<AssetLoaderAsync<T>>     kPool { get { return m_Pool; } }

        public AssetBundleLoader        abLoader    { get; private set; }

        private AssetBundleRequest      m_Request;

        public T                        asset       { get; private set; }

#if UNITY_EDITOR
        public string                   assetPath;
        private bool                    m_LoadFromEditor;
#endif

        public AssetLoaderAsync()
        {
            abLoader = null;
            m_Request = null;
            asset = default;
        }

        static internal AssetLoaderAsync<T> Get(string assetPath)
        {
            if (m_Pool == null)
            {
                m_Pool = new LinkedObjectPool<AssetLoaderAsync<T>>(AssetManager.PreAllocateAssetLoaderAsyncPoolSize);
            }

            AssetLoaderAsync<T> loader = (AssetLoaderAsync<T>)m_Pool.Get();
            loader.LoadAsset(assetPath);
            loader.Pool = m_Pool;
            return loader;
        }

        static internal void Release(AssetLoaderAsync<T> loader)
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
                    asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                    this.assetPath = assetPath;
                    m_LoadFromEditor = true;
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

        private void LoadAssetInternal(string assetPath)
        {
            CustomManifest.FileDetail fd = AssetManager.GetFileDetail(assetPath);
            abLoader = AssetBundleLoader.Get(fd.bundleHash);
            if (abLoader.assetBundle != null)
            {
                m_Request = abLoader.assetBundle.LoadAssetAsync<T>(fd.fileName);
            }
        }

        private void Unload()
        {
            if (abLoader != null)
            {
                AssetBundleLoader.Release(abLoader);
                abLoader = null;
            }
            m_Request = null;
            asset = null;
#if UNITY_EDITOR
            m_LoadFromEditor = false;
#endif           
        }

        private bool IsDone()
        {
            if (m_Request == null)
            { // bundle加载失败，释放所有bundle
                Unload();
                return true;
            }

            if (m_Request.isDone)
            {
                asset = m_Request.asset as T;
                if(asset == null)
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

        public bool MoveNext()
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

        LinkedObjectPool<AssetLoaderAsync<T>>       ILinkedObjectPoolNode<AssetLoaderAsync<T>>.List     { get; set; }

        ILinkedObjectPoolNode<AssetLoaderAsync<T>>  ILinkedObjectPoolNode<AssetLoaderAsync<T>>.Next     { get; set; }

        ILinkedObjectPoolNode<AssetLoaderAsync<T>>  ILinkedObjectPoolNode<AssetLoaderAsync<T>>.Prev     { get; set; }

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

        public IPool Pool { get; set; }    }
}