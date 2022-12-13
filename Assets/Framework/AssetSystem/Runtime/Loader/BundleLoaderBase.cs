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
        public EBundleLoadStatus    status              { get; private set; }
        public string               lastError           { get; private set; }
        public AssetBundle          cachedBundle        { get; private set; }
        public string               bundlePath          { get; private set; }
        public float                downloadProgress    { get; set; }
        public ulong                downloadBytes       { get; set; }
        public bool                 isDone              { get { return status == EBundleLoadStatus.Succeed || status == EBundleLoadStatus.Failed; } }
        public bool                 canDestroy          { get { return isDone ? refCount <= 0 : false; } }

        private BundleLoaderBase() { }
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

        public abstract void Update();
        public abstract void WaitForAsyncComplete();
    }
}