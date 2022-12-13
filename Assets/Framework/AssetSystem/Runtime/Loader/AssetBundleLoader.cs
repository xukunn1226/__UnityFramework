using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    internal class AssetBundleLoader : BundleLoaderBase
    {
        private enum ESteps
        {
            None            = 0,
            LoadFile        = 1,
            WaitLoadFile    = 2,
            Done            = 3,
        }
        private ESteps                      m_Step = ESteps.None;
        private bool                        m_RequestAsyncComplete;
        private AssetBundleCreateRequest    m_BundleRequest;

        protected AssetBundleLoader() : base() { }

        public AssetBundleLoader(AssetSystem assetSystem, BundleInfo bundleInfo) : base(assetSystem, bundleInfo)
        { }

        public override void Update()
        {
            if (m_Step == ESteps.Done)
                return;

            if(m_Step == ESteps.None)
            {
                if(bundleInfo.loadMethod == ELoadMethod.LoadFromStreaming)
                {
                    m_Step = ESteps.LoadFile;
                    bundlePath = bundleInfo.descriptor.streamingFilePath;
                }
                else if(bundleInfo.loadMethod == ELoadMethod.LoadFromCache)
                {
                    m_Step = ESteps.LoadFile;
                    bundlePath = bundleInfo.descriptor.cachedFilePath;
                }
                else if(bundleInfo.loadMethod == ELoadMethod.LoadFromRemote)
                { // todo: ÔÝ²»Ö§³Ö

                }
                else
                {
                    throw new System.NotImplementedException($"{bundleInfo.loadMethod.ToString()}");
                }
            }

            if(m_Step == ESteps.LoadFile)
            {
                downloadProgress = 1;
                downloadBytes = (ulong)bundleInfo.descriptor.fileSize;

                var loadMethod = (EBundleLoadMethod)bundleInfo.descriptor.loadMethod;
                if(loadMethod == EBundleLoadMethod.LoadFromFile)
                {
                    if(m_RequestAsyncComplete)
                    {
                        cachedBundle = AssetBundle.LoadFromFile(bundlePath);
                    }
                    else
                    {
                        m_BundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
                    }
                }
                else
                {

                }
                m_Step = ESteps.WaitLoadFile;
            }

            if(m_Step == ESteps.WaitLoadFile)
            {
                if(m_BundleRequest != null)
                {
                    if(m_RequestAsyncComplete)
                    {
                        Debug.LogWarning($"Suspend the main thread to load asset bundle.");
                        cachedBundle = m_BundleRequest.assetBundle;
                    }
                    else
                    {
                        if (!m_BundleRequest.isDone)
                            return;
                        cachedBundle = m_BundleRequest.assetBundle;
                    }
                }

                if(cachedBundle != null)
                {
                    m_Step = ESteps.Done;
                    status = EBundleLoadStatus.Succeed;
                }
                else
                {
                    // error
                    m_Step = ESteps.Done;
                    status = EBundleLoadStatus.Failed;

                    lastError = $"Failed to load asset bundle: {bundleInfo.descriptor.bundleName}";
                    Debug.LogError(lastError);
                }
            }
        }

        public override void WaitForAsyncComplete()
        {
            m_RequestAsyncComplete = true;
            while(true)
            {
                Update();
                if (isDone)
                    break;
            }
        }
    }
}