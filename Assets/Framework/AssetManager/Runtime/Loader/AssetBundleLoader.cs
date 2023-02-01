using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    /// <summary>
    /// AssetBundle加载器
    /// </summary>
    internal class AssetBundleLoaderEx : BundleLoaderBase
    {
        private enum ESteps
        {
            None            = 0,
            LoadFile        = 1,
            CheckLoadFile   = 2,
            Done            = 3,
        }
        private ESteps                      m_Step = ESteps.None;
        private bool                        m_RequestAsyncComplete;
        private AssetBundleCreateRequest    m_BundleRequest;

        private AssetBundleLoaderEx() { }

        public AssetBundleLoaderEx(AssetSystem assetSystem, BundleInfo bundleInfo) : base(assetSystem, bundleInfo)
        { }

        public override void Update()
        {
            isTriggerLoadingRequest = false;
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
                { // todo: 暂不支持
                    throw new System.Exception($"Unsupport load bundle from remote: {bundleInfo.loadMethod}");
                }
                else
                {
                    throw new System.NotImplementedException($"{bundleInfo.loadMethod}");
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
                { // 其他加载模式需要解密
                    if(assetSystem.decryptionServices == null)
                    {
                        m_Step = ESteps.Done;
                        status = EBundleLoadStatus.Failed;
                        lastError = $"IDecryptionServices is null";
                        Debug.LogError(lastError);
                        return;
                    }

                    // TODO: 以后再做
                }
                m_Step = ESteps.CheckLoadFile;
                isTriggerLoadingRequest = true;
            }

            // 检测AssetBundle加载结果
            if(m_Step == ESteps.CheckLoadFile)
            {
                if(m_BundleRequest != null)
                {
                    if(m_RequestAsyncComplete)
                    { // 初始是异步请求，加载结束前执行WaitForAsyncComplete，将执行到这里，强制把异步转为同步
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

                    // TODO: 如果是从cache加载资源失败，可能是资源损坏，需要重新下载
                    if(bundleInfo.loadMethod == ELoadMethod.LoadFromCache)
                    {

                    }
                }
            }
        }

        // TODO: 要不要有超时机制做保护？
        public override void WaitForAsyncComplete()
        {
            m_RequestAsyncComplete = true;
            while(true)     // 因为可能有下载，所以需要while(true)
            {
                Update();
                if (isDone)
                    break;
            }
        }
    }
}