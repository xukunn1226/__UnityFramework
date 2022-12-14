using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    /// <summary>
    /// ��Դ��������ʱ��Ϣ
    /// </summary>
    public class BundleInfo
    {
        public readonly BundleDescriptor    descriptor;
        public readonly ELoadMethod         loadMethod;

        private BundleInfo() { }

        public BundleInfo(BundleDescriptor descriptor, ELoadMethod loadMethod)
        {
            this.descriptor = descriptor;
            this.loadMethod = loadMethod;
        }
    }
}