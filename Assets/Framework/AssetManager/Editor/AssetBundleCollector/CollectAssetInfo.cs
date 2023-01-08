using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.AssetEditorWindow
{
    public class CollectAssetInfo
    {
        /// <summary>
        /// 收集器类型
        /// </summary>
        public ECollectorType CollectorType { private set; get; }

        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName { private set; get; }

        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath { private set; get; }

        /// <summary>
        /// 是否为原生资源
        /// </summary>
        public bool IsRawAsset { private set; get; }

        /// <summary>
        /// 依赖的资源列表
        /// </summary>
        public List<string> DependAssets = new List<string>();

        public CollectAssetInfo(ECollectorType collectorType, string bundleName, string assetPath, bool isRawAsset)
        {
            CollectorType = collectorType;
            BundleName = bundleName;
            AssetPath = assetPath;
            IsRawAsset = isRawAsset;
        }

        /// <summary>
        /// 资源包名称追加包裹名
        /// </summary>
        public void BundleNameAppendPackageName(string packageName)
        {
            BundleName = $"{packageName.ToLower()}_{BundleName}";
        }
    }
}