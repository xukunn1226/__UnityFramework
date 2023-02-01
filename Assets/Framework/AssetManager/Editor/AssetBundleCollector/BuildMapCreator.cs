using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using System.Linq;
using static Framework.AssetManagement.AssetEditorWindow.CollectAssetInfo;

namespace Framework.AssetManagement.AssetEditorWindow
{
    static public class BuildMapCreator
    {
        public static BuildMapContext CreateBuildMap(string configName)
        {
            BuildMapContext context = new BuildMapContext();
            
            // step1. 检查此配置的合法性
            AssetBundleCollectorSettingData.Instance.CheckConfigError(configName);

            string error = AssetBundleCollectorSettingData.Instance.CheckValidation(configName);
            if(string.IsNullOrEmpty(error) == false)
            {
                context.Error = error;
                return context;
            }

            // step2. 获取所有收集器收集的资源
            List<CollectAssetInfo> allCollectAssets = AssetBundleCollectorSettingData.Instance.GetAllCollectAssets(configName);

            // step3. 遍历所有收集资源的依赖资源，扩展资源列表
            ParseAllDependNodes(ref allCollectAssets);

            // step4. 再次遍历所有收集资源的依赖资源，进行合并处理
            MergeAllAssets(ref allCollectAssets);

            // step5. 记录所有收集器资源
            Dictionary<string, BuildAssetInfo> buildAssetDic = CreateBuildAssetInfos(allCollectAssets);

            // step6. 记录依赖资源列表
            FillDependAssetInfos(ref allCollectAssets, ref buildAssetDic);

            // step7. 计算完整的资源包名
            foreach (KeyValuePair<string, BuildAssetInfo> pair in buildAssetDic)
            {
                pair.Value.CalculateFullBundleName();
            }

            // step8. 构建资源包
            var allBuildinAssets = buildAssetDic.Values.ToList();
            if (allBuildinAssets.Count == 0)
                throw new System.Exception("构建的资源列表不能为空");
            foreach (var assetInfo in allBuildinAssets)
            {
                context.PackAsset(assetInfo);
            }

            return context;
        }

        /// <summary>
        /// 根据资源的依赖列表扩充
        /// </summary>
        /// <param name="allCollectAssets"></param>
        /// <exception cref="System.Exception"></exception>
        static private void ParseAllDependNodes(ref List<CollectAssetInfo> allCollectAssets)
        {
            // 把所有依赖资源展开加入总资源列表中
            for (int i = allCollectAssets.Count - 1; i >= 0; --i)
            {
                CollectAssetInfo collectAssetInfo = allCollectAssets[i];

                if (collectAssetInfo.AssetPath != collectAssetInfo.DependTree.assetPath)
                    throw new System.Exception("should never get here");

                if (collectAssetInfo.DependTree.children.Count == 0)
                    continue;

                List<CollectAssetInfo.DependNode> allDependNodes = collectAssetInfo.GetAllDependNodes();
                foreach (var node in allDependNodes)
                {
                    CollectAssetInfo assetInfo = allCollectAssets.Find(item => { return item.AssetPath == node.assetPath; });
                    if (assetInfo == null)
                    {
                        // 自动分析收集到的资源，默认使用PackFile打包规则
                        IPackRule packRule = PackFile.StaticPackRule;
                        string bundleName = $"share_{packRule.GetBundleName(new PackRuleData(node.assetPath))}";

                        assetInfo = new CollectAssetInfo(ECollectorType.None, bundleName, node.assetPath, false);
                        assetInfo.UsedBy = 0;
                        allCollectAssets.Add(assetInfo);
                    }
                }
            }

            // 统计所有直接的依赖资源被引用的次数
            foreach (var collectAsset in allCollectAssets)
            {
                List<DependNode> dependNodes = collectAsset.GetDirectDependNodes();
                foreach (var depend in dependNodes)
                {
                    CollectAssetInfo dependAssetInfo = allCollectAssets.Find(item => { return item.AssetPath == depend.assetPath; });
                    if (dependAssetInfo == null)
                        throw new System.Exception($"should never get here");

                    if (dependAssetInfo.CollectorType == ECollectorType.None)
                    {
                        ++dependAssetInfo.UsedBy;
                    }
                }
            }
        }

        static private void MergeAllAssets(ref List<CollectAssetInfo> allCollectAssets)
        {
            foreach (var topAsset in allCollectAssets)
            {
                foreach (var childNode in topAsset.DependTree.children)
                {
                    MergeDependAssets(allCollectAssets, topAsset, childNode);
                }
            }
        }

        /// <summary>
        /// 遍历依赖资源节点，向上追溯进行必要的合并操作
        /// 注意：依赖资源的三种情况：已被收集器收集（标记打包）、未被收集器收集且被引用两次以上、未被收集器收集且仅被一次引用
        /// 第三种情况需要向上追溯，找到合适的节点把资源归并一起打包
        /// </summary>
        /// <param name="allCollectAssets"></param>
        /// <param name="topAsset"></param>
        /// <param name="childNode"></param>
        /// <returns>节点不需要合并或合并完成，则返回true，否则返回false</returns>
        /// <exception cref="System.Exception"></exception>        
        static private bool MergeDependAssets(List<CollectAssetInfo> allCollectAssets, CollectAssetInfo topAsset, CollectAssetInfo.DependNode childNode)
        {
            CollectAssetInfo assetInfo = allCollectAssets.Find(item => { return item.AssetPath == childNode.assetPath; });
            if (assetInfo == null)
                throw new System.Exception($"should never get here: {childNode.assetPath}");

            // 第一种情况：被收集器收集（标记打包）
            if (assetInfo.CollectorType != ECollectorType.None)
                return true;

            // 第二种情况：未被收集器收集，但被引用了两次以上，将独立打为资源包
            if (assetInfo.UsedBy > 1)
                return true;

            // 第三种情况：未被收集器收集，且仅被引用一次，需要向上追溯找到合适的资源归类一并打包
            CollectAssetInfo.DependNode parent = childNode.parent;
            bool bMerged = false;
            while (parent != null)
            {
                CollectAssetInfo parentAssetInfo = allCollectAssets.Find(item => { return item.AssetPath == parent.assetPath; });
                if (parentAssetInfo == null)
                    throw new System.Exception($"should never get here: {parent.assetPath}");

                // 向上追溯找到资源包
                if (parentAssetInfo.CanBeMerged() == false)
                {
                    // 找到可合并的资源
                    bMerged = true;
                    assetInfo.CloneBundleName(parentAssetInfo);
                    break;
                }
                parent = parent.parent;
            }
            if (bMerged == false)
            { // 未找到可合并的资源，则与顶层资源合并
                assetInfo.CloneBundleName(topAsset);
            }

            foreach (var child in childNode.children)
            {
                MergeDependAssets(allCollectAssets, topAsset, child);
            }
            return bMerged;
        }

        /// <summary>
        /// 由收集器资源列表生成构建资源列表
        /// </summary>
        /// <param name="allCollectAssets"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        static private Dictionary<string, BuildAssetInfo> CreateBuildAssetInfos(List<CollectAssetInfo> allCollectAssets)
        {
            Dictionary<string, BuildAssetInfo> buildAssetDic = new Dictionary<string, BuildAssetInfo>(1000);
            foreach (var collectAsset in allCollectAssets)
            {
                if (string.IsNullOrEmpty(collectAsset.BundleName))
                    throw new System.Exception($"BundleName is null, {collectAsset.AssetPath}");

                if (buildAssetDic.ContainsKey(collectAsset.AssetPath))
                    throw new System.Exception($"Should never get here! {collectAsset.AssetPath} has already exists");

                var buildAssetInfo = new BuildAssetInfo(collectAsset.CollectorType, collectAsset.BundleName, collectAsset.AssetPath, collectAsset.IsRawAsset);
                buildAssetDic.Add(collectAsset.AssetPath, buildAssetInfo);
            }
            return buildAssetDic;
        }

        static private void FillDependAssetInfos(ref List<CollectAssetInfo> allCollectAssets, ref Dictionary<string, BuildAssetInfo> buildAssetDic)
        {
            foreach (var collectAsset in allCollectAssets)
            {
                if (collectAsset.CanBeMerged())
                    continue;

                if (buildAssetDic.ContainsKey(collectAsset.AssetPath) == false)
                    throw new System.Exception($"Should never get here! {collectAsset.AssetPath} is not exists");

                var dependAssetInfos = new List<BuildAssetInfo>();
                List<CollectAssetInfo.DependNode> allDependNodes = collectAsset.GetAllDependNodes();
                foreach (var dependNode in allDependNodes)
                {
                    if (buildAssetDic.TryGetValue(dependNode.assetPath, out var buildAssetInfo) == false)
                        throw new System.Exception($"Should never get here!");

                    CollectAssetInfo assetInfo = allCollectAssets.Find(item => { return item.AssetPath == dependNode.assetPath; });
                    if (assetInfo == null)
                        throw new System.Exception($"Should never get here! {dependNode.assetPath} not exists in allCollectAssets");

                    // 此类资源将被合并至其他资源包中
                    if (assetInfo.CanBeMerged())
                        continue;

                    dependAssetInfos.Add(buildAssetInfo);
                }
                buildAssetDic[collectAsset.AssetPath].SetAllDependAssetInfos(dependAssetInfos);
            }
        }
    }
}