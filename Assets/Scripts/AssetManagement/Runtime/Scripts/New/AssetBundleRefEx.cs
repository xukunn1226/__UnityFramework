using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cache;

namespace AssetManagement.Runtime
{
    /// <summary>
    /// asset bundle reference info
    /// </summary>
    internal class AssetBundleRefEx : IBetterLinkedListNode<AssetBundleRefEx>, IPooledObject
    {
        static private LinkedObjectPool<AssetBundleRefEx>   m_Pool;

        private int             m_Refs;

        internal AssetBundle    AssetBundle         { get; set; }

        internal string         AssetBundleName     { get; set; }

        public AssetBundleRefEx()
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

        static internal AssetBundleRefEx Get(string assetBundleName, AssetBundle assetBundle)
        {
            if (m_Pool == null)
            {
                m_Pool = new LinkedObjectPool<AssetBundleRefEx>(AssetManagerEx.PreAllocateAssetBundlePoolSize);
            }

            AssetBundleRefEx abRef = (AssetBundleRefEx)m_Pool.Get();
            abRef.AssetBundleName = assetBundleName;
            abRef.AssetBundle = assetBundle;
            abRef.Pool = m_Pool;

            return abRef;
        }

        static internal void Release(AssetBundleRefEx abRef)
        {
            if (m_Pool == null || abRef == null)
                return;

            m_Pool.Return(abRef);
        }

        private void Reset()
        {
            m_Refs = 1;
            AssetBundle = null;
            AssetBundleName = null;
            Pool = null;
        }

        public LinkedObjectPool<AssetBundleRefEx>       List { get; set; }

        public IBetterLinkedListNode<AssetBundleRefEx>  Next { get; set; }

        public IBetterLinkedListNode<AssetBundleRefEx>  Prev { get; set; }

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
            AssetBundle?.Unload(true);
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