using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    /// <summary>
    /// 资源包加载器
    /// </summary>
    internal abstract class BundleLoaderBase
    {
        public BundleInfo           bundleInfo          { get; private set; }
        public AssetSystem          assetSystem         { get; private set; }
        public int                  refCount            { get; private set; }
        public EBundleLoadStatus    status              { get; protected set; }
        public string               lastError           { get; protected set; }
        public AssetBundle          cachedBundle        { get; protected set; }
        public string               bundlePath          { get; protected set; }     // Bundle资源包加载路径
        public float                downloadProgress    { get; set; }
        public ulong                downloadBytes       { get; set; }
        public bool                 isDone              { get { return status == EBundleLoadStatus.Succeed || status == EBundleLoadStatus.Failed; } }
        public bool                 canDestroy          { get { return isDone ? refCount <= 0 : false; } }

        protected BundleLoaderBase() { }
        public BundleLoaderBase(AssetSystem assetSystem, BundleInfo bundleInfo)
        {
            if (assetSystem == null || bundleInfo == null)
                throw new System.Exception($"assetSystem == null || bundleInfo == null");

            this.assetSystem = assetSystem;
            this.bundleInfo = bundleInfo;
            this.refCount = 0;
            this.status = EBundleLoadStatus.None;
        }

        public void IncreaseRefs()
        {
            ++refCount;
        }

        public void DecreaseRefs()
        {
            --refCount;
        }

        // TODO: 考虑边界情况，bundle未加载完时就被销毁，该如何处理？
        public void Destroy(bool forceDestroy)
        {
            if(!forceDestroy)
            {
                if (refCount > 0)
                    throw new System.Exception($"Bundle file loader ref is not zero: {bundleInfo.descriptor.bundleName}");
                if (!isDone)
                    throw new System.Exception($"Bundle file loader is not done: {bundleInfo.descriptor.bundleName}");
            }
            if(cachedBundle != null)
            {
                cachedBundle.Unload(true);
                cachedBundle = null;
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        internal void GetBundleDebugInfo(ref DebugBundleInfo info)
        {
            info.BundleName = bundleInfo.descriptor.bundleName;
            info.RefCount   = refCount;
            info.Status     = status.ToString();
        }

        public abstract void Update();
        public abstract void WaitForAsyncComplete();
    }
}