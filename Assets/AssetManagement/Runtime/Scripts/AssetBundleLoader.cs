using System.Collections.Generic;
using UnityEngine;

namespace AssetManagement.Runtime
{
    /// <summary>
    /// 负责加载AB及其依赖的AB包
    /// </summary>
    public class AssetBundleLoader
    {
        static private ResLoaderPool<AssetBundleLoader>     m_Pool;

        private LinkedListNode<AssetBundleRef>              m_MainAssetBundleRef;
        private List<LinkedListNode<AssetBundleRef>>        m_DependentAssetBundleRefs  = new List<LinkedListNode<AssetBundleRef>>();

#if UNITY_EDITOR
        public LinkedListNode<AssetBundleRef>               mainAssetBundleRef          { get { return m_MainAssetBundleRef; } }
        public List<LinkedListNode<AssetBundleRef>>         dependentAssetBundleRefs    { get { return m_DependentAssetBundleRefs; } }
#endif

        public AssetBundle                                  assetBundle                 { get { return m_MainAssetBundleRef?.Value.AssetBundle; } }

        static public LinkedListNode<AssetBundleLoader> Get(string InAssetBundleName)
        {
            if (m_Pool == null)
            {
                m_Pool = new ResLoaderPool<AssetBundleLoader>(AssetManager.preAllocateAssetBundleLoaderPoolSize);
            }

            LinkedListNode<AssetBundleLoader> abLoader = m_Pool.Get();
            abLoader.Value.Load(InAssetBundleName);
            return abLoader;
        }

        static public void Release(LinkedListNode<AssetBundleLoader> abloader)
        {
            if (m_Pool == null || abloader == null)
                throw new System.ArgumentNullException();

            abloader.Value.Unload();
            m_Pool.Release(abloader);
        }

        /// <summary>
        /// 加载AB及其依赖AB，一旦遇到ab加载失败则卸载之前已加载的AB
        /// </summary>
        /// <param name="InAssetBundleName"></param>
        private void Load(string InAssetBundleName)
        {
            bool exception = false;

            LinkedListNode<AssetBundleRef> abRef = AssetBundleManager.LoadAssetBundleFromFile(InAssetBundleName);
            if(abRef != null && abRef.Value.AssetBundle != null)
            {
                m_MainAssetBundleRef = abRef;
            }
            else
            {
                exception = true;
            }

            string[] Dependencies = AssetBundleManager.GetAllDependencies(InAssetBundleName);            
            if (!exception && Dependencies != null && Dependencies.Length > 0)
            {
                for (int i = 0; i < Dependencies.Length; ++i)
                {
                    abRef = AssetBundleManager.LoadAssetBundleFromFile(Dependencies[i]);
                    if (abRef != null && abRef.Value.AssetBundle != null)
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
            if(exception)
            {
                Unload();
            }
        }
        
        /// <summary>
        /// 卸载已加载的AB
        /// </summary>
        private void Unload()
        {
            if(m_MainAssetBundleRef != null && m_MainAssetBundleRef.Value != null)
            {
                AssetBundleManager.Unload(m_MainAssetBundleRef.Value.AssetBundleName);
                m_MainAssetBundleRef = null;
            }

            if(m_DependentAssetBundleRefs != null)
            {
                foreach(LinkedListNode<AssetBundleRef> abRef in m_DependentAssetBundleRefs)
                {
                    if(abRef != null && abRef.Value != null)
                    {
                        AssetBundleManager.Unload(abRef.Value.AssetBundleName);
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
    }
}