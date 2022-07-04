using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.AssetManagement.Runtime
{
    /// <summary>
    /// 同步加载“静态场景”和“动态场景”
    /// </summary>
    public class SceneLoader : ILinkedObjectPoolNode<SceneLoader>, IPooledObject
    {
        static private LinkedObjectPool<SceneLoader> m_Pool;
        static private int k_InitPoolSize = 1;

        private AssetBundleLoader   m_BundleLoader;

        private bool                m_bFromBundle;          // true: load scene from bundle; false: load scene from build settings

        public string               sceneName       { get; private set; }

        public LoadSceneMode        mode            { get; private set; }

        public AsyncOperation       unloadAsyncOp   { get; private set; }

        public SceneLoader()
        { }

        /// <summary>
        /// 从Bundle同步加载场景接口
        /// </summary>
        /// <param name="assetPath">场景文件路径</param>
        /// <param name="mode"></param>
        /// <returns></returns>
        static internal SceneLoader GetFromBundle(string assetPath, LoadSceneMode mode)
        {
            if (m_Pool == null)
            {
                m_Pool = new LinkedObjectPool<SceneLoader>(k_InitPoolSize);
            }

            SceneLoader loader = (SceneLoader)m_Pool.Get();
            loader.LoadSceneFromBundle(assetPath, mode);
            loader.Pool = m_Pool;
            loader.sceneName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            loader.mode = mode;
            loader.m_bFromBundle = true;
            return loader;
        }

        private void LoadSceneFromBundle(string assetPath, LoadSceneMode mode)
        {
#if UNITY_EDITOR
            switch (AssetManager.Instance.loaderType)
            {
                case LoaderType.FromEditor:
                    SceneManager.LoadScene(assetPath, mode);
                    break;
                case LoaderType.FromStreamingAssets:
                case LoaderType.FromPersistent:
                    InternalLoadSceneFromBundle(assetPath, mode);
                    break;
            }
#else
            InternalLoadSceneFromBundle(assetPath, mode);
#endif
        }

        private void InternalLoadSceneFromBundle(string assetPath, LoadSceneMode mode)
        {
            m_BundleLoader = AssetManager.LoadAssetBundle(assetPath);
            if (m_BundleLoader.assetBundle == null)
                throw new System.Exception("failed to load scene bundle");
            if (!m_BundleLoader.assetBundle.isStreamedSceneAssetBundle)
                throw new System.Exception($"{assetPath} is not streamed scene asset bundle");

            SceneManager.LoadScene(System.IO.Path.GetFileNameWithoutExtension(AssetManager.GetFileDetail(assetPath).fileName), mode);
        }

        /// <summary>
        /// 从Build Settings加载场景接口
        /// </summary>
        /// <param name="assetPath">不带后缀名，小写</param>
        /// <param name="mode"></param>
        /// <returns></returns>
        static internal SceneLoader Get(string assetPath, LoadSceneMode mode)
        {
            if (m_Pool == null)
            {
                m_Pool = new LinkedObjectPool<SceneLoader>(k_InitPoolSize);
            }

            SceneLoader loader = (SceneLoader)m_Pool.Get();
            loader.LoadScene(assetPath, mode);
            loader.Pool = m_Pool;
            loader.sceneName = assetPath;
            loader.mode = mode;
            loader.m_bFromBundle = false;
            return loader;
        }

        /// <summary>
        /// scene must be add in build settings
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="mode"></param>
        private void LoadScene(string assetPath, LoadSceneMode mode)
        {
            SceneManager.LoadScene(assetPath, mode);
        }

        static internal AsyncOperation Release(SceneLoader loader)
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
        }



        LinkedObjectPool<SceneLoader>       ILinkedObjectPoolNode<SceneLoader>.List     { get; set; }

        ILinkedObjectPoolNode<SceneLoader>  ILinkedObjectPoolNode<SceneLoader>.Next     { get; set; }

        ILinkedObjectPoolNode<SceneLoader>  ILinkedObjectPoolNode<SceneLoader>.Prev     { get; set; }

        void IPooledObject.OnInit() { }

        /// <summary>
        /// 从对象池中拿出时的回调
        /// </summary>
        void IPooledObject.OnGet()
        {
            if (m_bFromBundle && m_BundleLoader != null)
                throw new System.Exception("Previous Bundle Loader not Release!!");
            unloadAsyncOp = null;       // OnRelease时不释放，因为卸载是异步
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