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

        // TODO: ������Ҫ�ٺú����룬��������ԭ����Դ����Ҫ��ȡ����������FMOD��֧��ֱ�Ӽ���streaming�µ�ԭ����Դ
        public override void Update()
        {
            if (m_Step == EStep.Done)
                return;

            if(m_Step == EStep.None)
            {
                if(bundleInfo.loadMethod == ELoadMethod.LoadFromStreaming)
                {
#if UNITY_ANDROID
                    // ��׿ƽ̨�޷�ֱ��ʹ�����õ�ԭ����Դ��streamingAssets������Ҫ��ȡ����
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
                // TODO: ��ѹ�ļ�

                m_Step = EStep.WaitUnpack;
            }

            if(m_Step == EStep.WaitUnpack)
            {
                // TODO: �ȴ���ѹ���

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
            // TODO: ���Update����Ϊ�������߳���ִ�У����磬Э�̣����˴����������ķ�����
            while(true)
            {
                Update();
                if (isDone)
                    break;
            }
        }
    }
}