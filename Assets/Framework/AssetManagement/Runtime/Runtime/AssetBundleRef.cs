using UnityEngine;
using Framework.Cache;

namespace Framework.AssetManagement.Runtime
{
    /// <summary>
    /// asset bundle reference info
    /// </summary>
    public class AssetBundleRef : ILinkedObjectPoolNode<AssetBundleRef>, IPooledObject
    {
        static private LinkedObjectPool<AssetBundleRef>   m_Pool;

        public int              refs                { get; private set; }

        public AssetBundle      assetBundle         { get; private set; }

        public string           assetBundleName     { get; private set; }

        public AssetBundleRef()
        {
            refs = 1;
        }

        internal int IncreaseRefs()
        {
            return ++refs;
        }

        internal int DecreaseRefs()
        {
            return --refs;
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
                throw new System.ArgumentNullException();

            m_Pool.Return(abRef);
        }

        private void Reset()
        {
            refs = 1;
            assetBundle = null;
            assetBundleName = null;
            Pool = null;
        }

        LinkedObjectPool<AssetBundleRef>        ILinkedObjectPoolNode<AssetBundleRef>.List  { get; set; }

        ILinkedObjectPoolNode<AssetBundleRef>   ILinkedObjectPoolNode<AssetBundleRef>.Next  { get; set; }

        ILinkedObjectPoolNode<AssetBundleRef>   ILinkedObjectPoolNode<AssetBundleRef>.Prev  { get; set; }

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
            assetBundle?.Unload(true);
            Reset();
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