using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;

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
            for(int i = allCollectAssets.Count - 1; i >= 0; --i)
            {
                CollectAssetInfo collectAssetInfo = allCollectAssets[i];
                
                if (collectAssetInfo.AssetPath != collectAssetInfo.DependTree.assetPath)
                    throw new System.Exception("should never get here");

                if (collectAssetInfo.DependTree.children.Count == 0)
                    continue;

                foreach(var node in collectAssetInfo.DependTree.children)
                {
                    CollectAssetInfo assetInfo = allCollectAssets.Find(item => { return item.AssetPath == node.assetPath; });
                    if(assetInfo != null)
                    {
                        // 依赖资源已在收集器收集的资源列表中，且类型是ECollectorType.None，则自增被依赖的次数
                        if (assetInfo.CollectorType == ECollectorType.None)
                        {
                            ++assetInfo.UsedBy;
                        }
                    }
                    else
                    {
                        // 自动分析收集到的资源，默认使用PackFile打包规则
                        IPackRule packRule = PackFile.StaticPackRule;
                        string bundleName = $"share_{packRule.GetBundleName(new PackRuleData(node.assetPath))}.{AssetManagerSettingsData.Setting.AssetBundleFileVariant}";

                        assetInfo = new CollectAssetInfo(ECollectorType.None, bundleName, node.assetPath, false);
                        assetInfo.UsedBy = 1;
                        allCollectAssets.Add(assetInfo);
                    }
                }
            }

            // step4. 再次遍历所有收集资源的依赖资源，进行合并处理
            foreach (var topAsset in allCollectAssets)
            {
                foreach (var childNode in topAsset.DependTree.children)
                {
                    MergeDependAssets(allCollectAssets, topAsset, childNode);
                }
            }

            // step5. 记录所有收集器资源
            Dictionary<string, BuildAssetInfo> buildAssetDic = new Dictionary<string, BuildAssetInfo>(1000);
            foreach(var collectAsset in allCollectAssets)
            {

            }

            return context;
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

            // 第二种情况：未被收集器收集，但被引用了两次以上
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

                if (parentAssetInfo.CollectorType != ECollectorType.None || parentAssetInfo.UsedBy > 1)
                {
                    // 找到可合并的资源
                    bMerged = true;
                    assetInfo.CloneBundleName(parentAssetInfo);
                    break;
                }
                parent = parent.parent;
            }
            if(bMerged == false)
            { // 未找到可合并的资源，则与顶层资源合并
                assetInfo.CloneBundleName(topAsset);
            }

            foreach(var child in childNode.children)
            {
                MergeDependAssets(allCollectAssets, topAsset, child);
            }
            return bMerged;
        }
    }
}