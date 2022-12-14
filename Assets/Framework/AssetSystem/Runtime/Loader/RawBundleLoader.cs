using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    internal class RawBundleLoader : BundleLoaderBase
    {
        private enum EStep
        {
            None        = 0,
            Unpack      = 1,
            WaitUnpack  = 2,
            CheckFile   = 3,
            Done        = 4,
        }
        private EStep m_Step = EStep.None;
        
        private RawBundleLoader() { }
        public RawBundleLoader(AssetSystem assetSystem, BundleInfo bundleInfo) : base(assetSystem, bundleInfo)
        { }

        // TODO: 这里需要再好好想想，不是所有原生资源都需要提取出来，例如FMOD就支持直接加载streaming下的原生资源
        public override void Update()
        {
            if (m_Step == EStep.Done)
                return;

            if(m_Step == EStep.None)
            {
                if(bundleInfo.loadMethod == ELoadMethod.LoadFromStreaming)
                {
#if UNITY_ANDROID
                    // 安卓平台无法直接使用内置的原生资源（streamingAssets），需要提取出来
                    m_Step = EStep.Unpack;
                    bundlePath = bundleInfo.descriptor.cachedFilePath;
#else
                    m_Step = EStep.CheckFile;
                    bundlePath = bundleInfo.descriptor.streamingFilePath;
#endif
                }
                else if(bundleInfo.loadMethod == ELoadMethod.LoadFromCache)
                {
                    m_Step = EStep.CheckFile;
                    bundlePath = bundleInfo.descriptor.cachedFilePath;
                }
                else if(bundleInfo.loadMethod == ELoadMethod.LoadFromRemote)
                {
                    throw new System.Exception($"Unsupport to load raw bundle from remote: {bundleInfo.descriptor.bundleName}");
                }
                else
                {
                    throw new System.NotImplementedException($"{bundleInfo.loadMethod.ToString()}");
                }
            }

            if(m_Step == EStep.Unpack)
            {
                // TODO: 解压文件

                m_Step = EStep.WaitUnpack;
            }

            if(m_Step == EStep.WaitUnpack)
            {
                // TODO: 等待解压结果

                m_Step = EStep.CheckFile;
            }

            if(m_Step == EStep.CheckFile)
            {
                downloadProgress = 1;
                downloadBytes = (ulong)bundleInfo.descriptor.fileSize;

                m_Step = EStep.Done;
                if(File.Exists(bundlePath))
                {
                    status = EBundleLoadStatus.Succeed;
                }
                else
                {
                    status = EBundleLoadStatus.Failed;
                    lastError = $"Raw file not found: {bundlePath}";
                }
            }
        }

        public override void WaitForAsyncComplete()
        {
            // TODO: 如果Update的行为不是在线程中执行（例如，协程），此处会有死锁的风险吗？
            while(true)
            {
                Update();
                if (isDone)
                    break;
            }
        }
    }
}