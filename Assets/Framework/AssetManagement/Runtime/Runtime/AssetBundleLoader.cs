using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;

namespace Framework.AssetManagement.Runtime
{
    public class AssetBundleLoader : ILinkedObjectPoolNode<AssetBundleLoader>, IPooledObject
    {
        static private LinkedObjectPool<AssetBundleLoader> m_Pool;

        private AssetBundleRef          m_MainAssetBundleRef;
        private List<AssetBundleRef>    m_DependentAssetBundleRefs  = new List<AssetBundleRef>();

        public AssetBundleRef           mainAssetBundleRef          { get { return m_MainAssetBundleRef; } }

        public List<AssetBundleRef>     dependentAssetBundleRefs    { get { return m_DependentAssetBundleRefs; } }

        public AssetBundle              assetBundle                 { get { return m_MainAssetBundleRef?.assetBundle; } }
        
        static internal AssetBundleLoader Get(string InAssetBundleName)
        {
            if (m_Pool == null)
            {
                m_Pool = new LinkedObjectPool<AssetBundleLoader>(AssetManager.PreAllocateAssetBundleLoaderPoolSize);
            }

            AssetBundleLoader abLoader = (AssetBundleLoader)m_Pool.Get();
            abLoader.Load(InAssetBundleName);
            abLoader.Pool = m_Pool;
            return abLoader;
        }

        static internal void Release(AssetBundleLoader abloader)
        {
            if (m_Pool == null || abloader == null)
                throw new System.ArgumentNullException();

            m_Pool.Return(abloader);
        }

        /// <summary>
        /// 加载AB及其依赖AB
        /// </summary>
        /// <param name="InAssetBundleName"></param>
        private void Load(string InAssetBundleName)
        {
            AssetBundleRef abRef = AssetBundleManager.LoadAssetBundleFromFile(InAssetBundleName);
            if(abRef == null || abRef.assetBundle == null)
            { // 主bundle加载失败，直接返回
                m_MainAssetBundleRef = null;
                return;
            }

            string[] dependencies = AssetBundleManager.GetAllDependencies(InAssetBundleName);
            if (dependencies != null && dependencies.Length > 0)
            {
                for (int i = 0; i < dependencies.Length; ++i)
                {
                    abRef = AssetBundleManager.LoadAssetBundleFromFile(dependencies[i]);
                    if (abRef != null && abRef.assetBundle != null)
                    { // 依赖bundle加载失败不影响其余的依赖bundle
                        m_DependentAssetBundleRefs.Add(abRef);
                    }
                }
            }
        }

        /// <summary>
        /// 卸载已加载的AB
        /// </summary>
        private void Unload()
        {
            if (m_MainAssetBundleRef != null)
            {
                AssetBundleManager.Unload(m_MainAssetBundleRef.assetBundleName);
                m_MainAssetBundleRef = null;
            }

            if (m_DependentAssetBundleRefs != null)
            {
                foreach (AssetBundleRef abRef in m_DependentAssetBundleRefs)
                {
                    if (abRef != null)
                    {
                        AssetBundleManager.Unload(abRef.assetBundleName);
                    }
                }
                m_DependentAssetBundleRefs.Clear();
            }
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public T LoadAsset<T>(string assetName) where T : Object
        {
            return assetBundle?.LoadAsset<T>(assetName);
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public AssetBundleRequest LoadAssetAsync<T>(string assetName) where T : Object
        {
            return assetBundle?.LoadAssetAsync<T>(assetName);
        }

        LinkedObjectPool<AssetBundleLoader>         ILinkedObjectPoolNode<AssetBundleLoader>.List   { get; set; }

        ILinkedObjectPoolNode<AssetBundleLoader>    ILinkedObjectPoolNode<AssetBundleLoader>.Next   { get; set; }

        ILinkedObjectPoolNode<AssetBundleLoader>    ILinkedObjectPoolNode<AssetBundleLoader>.Prev   { get; set; }

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