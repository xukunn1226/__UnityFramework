using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cache;

namespace AssetManagement.Runtime
{
    /// <summary>
    /// asset bundle reference info
    /// </summary>
    public class AssetBundleRef : IBetterLinkedListNode<AssetBundleRef>, IPooledObject
    {
        static private LinkedObjectPool<AssetBundleRef>   m_Pool;

        private int             m_Refs;

        public AssetBundle      assetBundle         { get; private set; }

        public string           assetBundleName     { get; private set; }

        public AssetBundleRef()
        {
            m_Refs = 1;
        }

        internal int IncreaseRefs()
        {
            return ++m_Refs;
        }

        internal int DecreaseRefs()
        {
            return --m_Refs;
        }

        static internal AssetBundleRef Get(string assetBundleName, AssetBundle assetBundle)
        {
            if (m_Pool == null)
            {
                m_Pool = new LinkedObjectPool<AssetBundleRef>(AssetManager.PreAllocateAssetBundlePoolSize);
            }

            AssetBundleRef abRef = (AssetBundleRef)m_Pool.Get();
            abRef.assetBundleName = assetBundleName;
            abRef.assetBundle = assetBundle;
            abRef.Pool = m_Pool;

            return abRef;
        }

        static internal void Release(AssetBundleRef abRef)
        {
            if (m_Pool == null || abRef == null)
                return;

            m_Pool.Return(abRef);
        }

        private void Reset()
        {
            m_Refs = 1;
            assetBundle = null;
            assetBundleName = null;
            Pool = null;
        }

        public LinkedObjectPool<AssetBundleRef>       List { get; set; }

        public IBetterLinkedListNode<AssetBundleRef>  Next { get; set; }

        public IBetterLinkedListNode<AssetBundleRef>  Prev { get; set; }

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
            assetBundle?.Unload(true);
            Reset();
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