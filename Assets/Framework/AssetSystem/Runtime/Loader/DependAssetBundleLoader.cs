using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    internal class DependAssetBundleLoader
    {
        private readonly List<BundleLoaderBase> m_DependBundleLoaders;

        private DependAssetBundleLoader() { }
        public DependAssetBundleLoader(List<BundleLoaderBase> dependBundleLoaders)
        {
            m_DependBundleLoaders = dependBundleLoaders;
        }

        public void IncreaseRefs()
        {
            foreach(var loader in m_DependBundleLoaders)
            {
                loader.IncreaseRefs();
            }
        }

        public void DecreaseRefs()
        {
            foreach(var loader in m_DependBundleLoaders)
            {
                loader.DecreaseRefs();
            }
        }

        public bool IsDone()
        {
            foreach(var loader in m_DependBundleLoaders)
            {
                if (!loader.isDone)
                    return false;
            }
            return true;
        }

        public bool IsSucceed()
        {
            foreach(var loader in m_DependBundleLoaders)
            {
                if (loader.status != EBundleLoadStatus.Succeed)
                    return false;
            }
            return true;
        }

        public string GetLastError()
        {
            string err = null;
            foreach(var loader in m_DependBundleLoaders)
            {
                if(loader.isDone && loader.status == EBundleLoadStatus.Failed)
                {
                    err += loader.lastError;
                    err += "\n";
                }
            }
            return err;
        }

        public void WaitForAsyncComplete()
        {
            foreach(var loader in m_DependBundleLoaders)
            {
                loader.WaitForAsyncComplete();
            }
        }
    }
}