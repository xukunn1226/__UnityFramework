using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cache;

namespace AssetManagement.Runtime
{
    public class AssetBundleLoaderEx : IBetterLinkedListNode<AssetBundleLoaderEx>, IPooledObject
    {
        static private LinkedObjectPool<AssetBundleLoaderEx> m_Pool;

        private AssetBundleRefEx        m_MainAssetBundleRef;
        private List<AssetBundleRefEx>  m_DependentAssetBundleRefs  = new List<AssetBundleRefEx>();

        public AssetBundleRefEx         mainAssetBundleRef          { get { return m_MainAssetBundleRef; } }

        public List<AssetBundleRefEx>   dependentAssetBundleRefs    { get { return m_DependentAssetBundleRefs; } }

        public AssetBundle            assetBundle                 { get { return m_MainAssetBundleRef?.assetBundle; } }
        
        static internal AssetBundleLoaderEx Get(string InAssetBundleName)
        {
            if (m_Pool == null)
            {
                m_Pool = new LinkedObjectPool<AssetBundleLoaderEx>(AssetManagerEx.PreAllocateAssetBundleLoaderPoolSize);
            }

            AssetBundleLoaderEx abLoader = (AssetBundleLoaderEx)m_Pool.Get();
            abLoader.Load(InAssetBundleName);
            abLoader.Pool = m_Pool;
            return abLoader;
        }

        static internal void Release(AssetBundleLoaderEx abloader)
        {
            if (m_Pool == null || abloader == null)
                throw new System.ArgumentNullException();

            //abloader.Unload();
            //abloader.Pool = null;
            m_Pool.Return(abloader);
        }

        /// <summary>
        /// 加载AB及其依赖AB，一旦遇到ab加载失败则卸载之前已加载的AB
        /// </summary>
        /// <param name="InAssetBundleName"></param>
        private void Load(string InAssetBundleName)
        {
            bool exception = false;

            AssetBundleRefEx abRef = AssetBundleManagerEx.LoadAssetBundleFromFile(InAssetBundleName);
            if (abRef != null && abRef.assetBundle != null)
            {
                m_MainAssetBundleRef = abRef;
            }
            else
            {
                exception = true;
            }

            string[] dependencies = AssetBundleManager.GetAllDependencies(InAssetBundleName);
            if (!exception && dependencies != null && dependencies.Length > 0)
            {
                for (int i = 0; i < dependencies.Length; ++i)
                {
                    abRef = AssetBundleManagerEx.LoadAssetBundleFromFile(dependencies[i]);
                    if (abRef != null && abRef.assetBundle != null)
                    {
                        m_DependentAssetBundleRefs.Add(abRef);
                    }
                    else
                    {
                        exception = true;
                        break;
                    }
                }
            }

            // 一旦遇到ab加载异常，则把之前加载的所有ab卸载
            if (exception)
            {
                Unload();
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
                foreach (AssetBundleRefEx abRef in m_DependentAssetBundleRefs)
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

        public LinkedObjectPool<AssetBundleLoaderEx>        List    { get; set; }

        public IBetterLinkedListNode<AssetBundleLoaderEx>   Next    { get; set; }

        public IBetterLinkedListNode<AssetBundleLoaderEx>   Prev    { get; set; }

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