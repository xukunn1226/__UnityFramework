using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    internal sealed class BundleRawFileProvider : BundleProvider
    {
#pragma warning disable CS0628 // ���ܷ��������������µı�����Ա
        protected BundleRawFileProvider() { }
#pragma warning restore CS0628 // ���ܷ��������������µı�����Ա
        public BundleRawFileProvider(AssetSystem assetSystem, string providerGUID, AssetInfo assetInfo) : base(assetSystem, providerGUID, assetInfo)
        {
        }

        public override void Update()
        {
            if (isDone)
                return;

            if (status == EProviderStatus.None)
                status = EProviderStatus.CheckBundle;

            if(status == EProviderStatus.CheckBundle)
            {
                if(m_RequestAsyncComplete)
                {
                    mainBundleLoader.WaitForAsyncComplete();
                }

                if (!mainBundleLoader.isDone)
                    return;

                if(mainBundleLoader.status != EBundleLoadStatus.Succeed)
                {
                    status = EProviderStatus.Failed;
                    lastError = mainBundleLoader.lastError;
                    InvokeCompletion();
                    return;
                }
                status = EProviderStatus.Checking;
            }

            if(status == EProviderStatus.Checking)
            {
                bundlePath = mainBundleLoader.bundlePath;
                status = EProviderStatus.Succeed;
                InvokeCompletion();
            }
        }
    }
}