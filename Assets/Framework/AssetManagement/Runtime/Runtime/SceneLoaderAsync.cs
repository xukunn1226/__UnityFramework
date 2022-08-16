using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;
using UnityEngine.SceneManagement;

namespace Framework.AssetManagement.Runtime
{
    /// <summary>
    /// 异步加载“静态场景”和“动态场景”
    /// </summary>
    public class SceneLoaderAsync : IEnumerator, ILinkedObjectPoolNode<SceneLoaderAsync>, IPooledObject
    {
        static private LinkedObjectPool<SceneLoaderAsync> m_Pool;
        static private int k_InitPoolSize = 4;

        private AssetBundleLoader   m_BundleLoader;

        private bool                m_bFromBundle;          // true: load scene from bundle; false: load scene from build settings

        public string               sceneName               { get; private set; }

        public LoadSceneMode        mode                    { get; private set; }

        public AsyncOperation       loadAsyncOp             { get; private set; }

        public AsyncOperation       unloadAsyncOp           { get; private set; }

#if UNITY_EDITOR
        private bool                m_LoadFromEditor;
#endif

        public SceneLoaderAsync()
        { }

        /// <summary>
        /// 从Bundle异步加载场景接口
        /// </summary>
        /// <param name="assetPath">场景文件路径</param>
        /// <param name="mode"></param>
        /// <returns></returns>
        static internal SceneLoaderAsync GetFromBundle(string assetPath, LoadSceneMode mode, bool allowSceneActivation = true)
        {
            if(m_Pool == null)
            {
                m_Pool = new LinkedObjectPool<SceneLoaderAsync>(k_InitPoolSize);
            }

            SceneLoaderAsync loader = (SceneLoaderAsync)m_Pool.Get();
            loader.LoadSceneAsyncFromBundle(assetPath, mode, allowSceneActivation);
            loader.Pool = m_Pool;
            loader.sceneName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            loader.mode = mode;
            loader.m_bFromBundle = true;
            return loader;
        }

        private void LoadSceneAsyncFromBundle(string assetPath, LoadSceneMode mode, bool allowSceneActivation)
        {
#if UNITY_EDITOR
            switch (AssetManager.Instance.loaderType)
            {
                case LoaderType.FromEditor:
                    SceneManager.LoadScene(assetPath, mode);
                    m_LoadFromEditor = true;
                    break;
                case LoaderType.FromStreamingAssets:
                case LoaderType.FromPersistent:
                    InternalLoadSceneAsyncFromBundle(assetPath, mode, allowSceneActivation);
                    break;
            }
#else
            InternalLoadSceneAsyncFromBundle(assetPath, mode, allowSceneActivation);
#endif
        }

        private void InternalLoadSceneAsyncFromBundle(string assetPath, LoadSceneMode mode, bool allowSceneActivation = true)
        {
            m_BundleLoader = AssetManager.LoadAssetBundle(assetPath);
            if (m_BundleLoader.assetBundle == null)
                throw new System.Exception($"failed to load scene bundle: {assetPath}");
            if (!m_BundleLoader.assetBundle.isStreamedSceneAssetBundle)
                throw new System.Exception($"{assetPath} is not streamed scene asset bundle");

            loadAsyncOp = SceneManager.LoadSceneAsync(System.IO.Path.GetFileNameWithoutExtension(AssetManager.GetFileDetail(assetPath).fileName), mode);
            loadAsyncOp.allowSceneActivation = allowSceneActivation;
        }

        /// <summary>
        /// 从Build Settings加载场景接口
        /// </summary>
        /// <param name="sceneName">不带后缀名，小写</param>
        /// <param name="mode"></param>
        /// <returns></returns>
        static internal SceneLoaderAsync Get(string sceneName, LoadSceneMode mode, bool allowSceneActivation = true)
        {
            if (m_Pool == null)
            {
                m_Pool = new LinkedObjectPool<SceneLoaderAsync>(k_InitPoolSize);
            }

            SceneLoaderAsync loader = (SceneLoaderAsync)m_Pool.Get();
            loader.LoadSceneAsync(sceneName, mode, allowSceneActivation);
            loader.Pool = m_Pool;
            loader.sceneName = sceneName;
            loader.mode = mode;
            loader.m_bFromBundle = false;
            return loader;
        }

        private void LoadSceneAsync(string sceneName, LoadSceneMode mode, bool allowSceneActivation)
        {
#if UNITY_EDITOR
            switch (AssetManager.Instance.loaderType)
            {
                case LoaderType.FromEditor:
                    SceneManager.LoadScene(sceneName, mode);
                    m_LoadFromEditor = true;
                    break;
                case LoaderType.FromStreamingAssets:
                case LoaderType.FromPersistent:
                    InternalLoadSceneAsync(sceneName, mode, allowSceneActivation);
                    break;
            }
#else
            InternalLoadSceneAsync(sceneName, mode, allowSceneActivation);
#endif
        }

        private void InternalLoadSceneAsync(string sceneName, LoadSceneMode mode, bool allowSceneActivation)
        {
            loadAsyncOp = SceneManager.LoadSceneAsync(sceneName, mode);
            loadAsyncOp.allowSceneActivation = allowSceneActivation;
        }

        static internal AsyncOperation Release(SceneLoaderAsync loader)
        {
            if (m_Pool == null || loader == null)
                throw new System.ArgumentNullException();

            m_Pool.Return(loader);
            return loader.unloadAsyncOp;
        }

        private void InternalUnloadScene()
        {
            if(m_BundleLoader != null)
            {
                AssetManager.UnloadAssetBundle(m_BundleLoader);
                m_BundleLoader = null;
            }

            unloadAsyncOp = SceneManager.UnloadSceneAsync(sceneName);
#if UNITY_EDITOR
            m_LoadFromEditor = false;
#endif
        }

        private bool IsDone()
        {
            if (loadAsyncOp == null)
                return true;

            //Debug.Log($"[{Time.frameCount}]  {loadAsyncOp.progress}     {loadAsyncOp.isDone}");

            return loadAsyncOp.isDone;
        }

        object IEnumerator.Current
        {
            get { return null; }
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



        LinkedObjectPool<SceneLoaderAsync>       ILinkedObjectPoolNode<SceneLoaderAsync>.List     { get; set; }

        ILinkedObjectPoolNode<SceneLoaderAsync>  ILinkedObjectPoolNode<SceneLoaderAsync>.Next     { get; set; }

        ILinkedObjectPoolNode<SceneLoaderAsync>  ILinkedObjectPoolNode<SceneLoaderAsync>.Prev     { get; set; }

        void IPooledObject.OnInit() { }

        /// <summary>
        /// 从对象池中拿出时的回调
        /// </summary>
        void IPooledObject.OnGet()
        {
            if (m_bFromBundle && m_BundleLoader != null)
                throw new System.Exception("Previous Bundle Loader not Release!!");
            unloadAsyncOp = null;       // OnRelease时不释放，因为卸载是异步
            loadAsyncOp = null;
        }

        /// <summary>
        /// 放回对象池时的回调
        /// </summary>
        void IPooledObject.OnRelease()
        {
            InternalUnloadScene();
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