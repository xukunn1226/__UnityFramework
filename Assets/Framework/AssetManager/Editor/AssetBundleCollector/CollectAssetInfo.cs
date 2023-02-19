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
        /// 资源隶属的收集器
        /// 注意：
        /// </summary>
        public AssetBundleCollector Collector { get; private set; }

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

        public CollectAssetInfo(ECollectorType collectorType, string bundleName, string assetPath, bool isRawAsset, AssetBundleCollector collector = null)
        {
            CollectorType = collectorType;
            BundleName = bundleName;
            AssetPath = assetPath;
            IsRawAsset = isRawAsset;
            Collector = collector;
        }

        public void SetNewBundleName(string newBundleName)
        {
            BundleName = newBundleName;
        }

        public List<DependNode> GetDirectDependNodes()
        {
            return DependTree.children;
        }

        public List<DependNode> GetAllDependNodes()
        {
            List<DependNode> dependNodes = new List<DependNode>();
            foreach(var child in DependTree.children)
            {
                dependNodes.Add(child);
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

        public string LogInfo()
        {
            return string.Format($"AssetPath: {AssetPath}   ECollectorType: {CollectorType}     BundleName: {BundleName}    IsRawAsset: {IsRawAsset}");
        }

        /// <summary>
        /// 资源是否可以合入其他资源包
        /// 注意：CollectorType.None表示此资源非收集器收集，UsedBy == 1表示仅被引用一次
        /// </summary>
        /// <returns></returns>
        public bool CanBeMerged()
        {
            if(CollectorType == ECollectorType.None && UsedBy == 1)
                return true;
            return false;
        }
    }
}