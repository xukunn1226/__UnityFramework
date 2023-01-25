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
        /// 记录被其他资源依赖的次数
        /// 注意：仅CollectorType == ECollectorType.None才需要记录，其他type资源必定打包bundle
        /// </summary>
        public int UsedBy { set; get; }

        /// <summary>
        /// 依赖的资源树列表
        /// </summary>
        public class DependNode
        {
            public string           assetPath;
            public DependNode       parent;
            public List<DependNode> children = new List<DependNode>();
        }
        public DependNode DependTree = new DependNode();

        public CollectAssetInfo(ECollectorType collectorType, string bundleName, string assetPath, bool isRawAsset)
        {
            CollectorType = collectorType;
            BundleName = bundleName;
            AssetPath = assetPath;
            IsRawAsset = isRawAsset;
        }

        /// <summary>
        /// 复制BundleName
        /// </summary>
        /// <param name="other"></param>
        public void CloneBundleName(CollectAssetInfo other)
        {
            BundleName = other.BundleName;
        }

        public List<DependNode> GetAllDependNodes()
        {
            List<DependNode> dependNodes = new List<DependNode>();
            foreach(var child in DependTree.children)
            {
                dependNodes.AddRange(GetDependNodes(child));
            }
            return dependNodes;
        }

        private List<DependNode> GetDependNodes(DependNode node)
        {
            if (node == null)
                throw new System.Exception($"Should never get here");

            List<DependNode> nodes = new List<DependNode>(node.children);
            foreach(var child in node.children)
            {
                nodes.AddRange(GetDependNodes(child));
            }
            return nodes;
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