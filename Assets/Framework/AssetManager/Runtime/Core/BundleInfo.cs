using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    /// <summary>
    /// 资源包的运行时信息
    /// </summary>
    public class BundleInfo
    {
        public readonly BundleDescriptor    descriptor;
        public readonly ELoadMethod         loadMethod;

        /// <summary>
        /// 远端下载地址
        /// </summary>
        public string RemoteMainURL { private set; get; }

        /// <summary>
        /// 远端下载备用地址
        /// </summary>
        public string RemoteFallbackURL { private set; get; }

        private BundleInfo() { }

        /// <summary>
        /// 加载内置资源或补丁资源
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="loadMethod"></param>
        public BundleInfo(BundleDescriptor descriptor, ELoadMethod loadMethod)
        {
            this.descriptor = descriptor;
            this.loadMethod = loadMethod;
        }

        /// <summary>
        /// 边玩边下或提取资源时（from streaming assets）
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="loadMethod"></param>
        /// <param name="mainURL"></param>
        /// <param name="fallbackURL"></param>
        public BundleInfo(BundleDescriptor descriptor, ELoadMethod loadMethod, string mainURL, string fallbackURL)
        {
            this.descriptor = descriptor;
            this.loadMethod = loadMethod;
            RemoteMainURL = mainURL;
            RemoteFallbackURL = fallbackURL;
        }
    }
}