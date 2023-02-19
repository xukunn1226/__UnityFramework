using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [Serializable]
    public class AssetBundleCollectorConfig
    {
        public string ConfigName;
        public string ConfigDesc;
        public List<AssetBundleCollectorGroup> Groups = new List<AssetBundleCollectorGroup>();

        /// <summary>
        /// 检测配置错误
        /// </summary>
        public void CheckConfigError()
        {
            foreach (var group in Groups)
            {
                group.CheckConfigError();
            }
        }

        /// <summary>
        /// 判断配置的合法性
        /// 原则：同一个资源不能被多个收集器收集
        /// </summary>
        /// <returns></returns>
        public string CheckValidation()
        {
            StringBuilder sb = new StringBuilder();

            Dictionary<string, AssetBundleCollector> result = new Dictionary<string, AssetBundleCollector>(10000);      // key: assetPath; value: collector
            foreach(var group in Groups)
            {
                foreach (var collector in group.Collectors)
                {
                    var temper = collector.GetAllCollectAssets(group);
                    foreach (var assetInfo in temper)
                    {
                        if (result.ContainsKey(assetInfo.AssetPath) == false)
                            result.Add(assetInfo.AssetPath, collector);
                        else
                        {
                            sb.AppendLine($"{assetInfo.AssetPath}: 资源同时存在于收集器 [{collector.CollectPath}] and [{result[assetInfo.AssetPath].CollectPath}]");
                        }
                    }
                }
            }
            return sb.ToString();
        }

        public AssetBundleCollector FindCollector(string collectPath)
        {
            foreach(var group in Groups)
            {
                var collector = group.FindCollector(collectPath);
                if(collector != null)
                    return collector;
            }
            return null;
        }

        public List<CollectAssetInfo> GetAllCollectAssets()
        {
            List<CollectAssetInfo> results = new List<CollectAssetInfo>();
            foreach(var group in Groups)
            {
                results.AddRange(group.GetAllCollectAssets());
            }

            // 针对符合PackToOtherCollector打包规则的资源，在所有资源收集后更新其BundleName
            string ruleName = nameof(PackToOtherCollector);
            foreach(var assetInfo in results)
            {
                if (assetInfo.Collector == null)
                    continue;
                if (assetInfo.Collector.PackRuleName != ruleName)
                    continue;

                // 找到引用的其他收集器
                var referencedCollector = FindCollector(assetInfo.Collector.OtherCollectPath);
                if (referencedCollector == null)
                {
                    Debug.LogWarning($"Not found the referenced collector: {assetInfo.Collector.OtherCollectPath}");
                    continue;       // 没有找到引用的收集器，则不执行重定向
                }

                // 重定位后的资源路径
                string finalAssetPath = assetInfo.Collector.GetPackToOtherAssetPath(assetInfo.AssetPath);

                // 重新计算BundleName
                string newBundleName = referencedCollector.GetBundleName(referencedCollector.Group, finalAssetPath);
                assetInfo.SetNewBundleName(newBundleName);
            }
            return results;
        }

        public AssetBundleCollectorGroup AddGroup(string groupName, string groupDesc)
        {
            AssetBundleCollectorGroup group = new AssetBundleCollectorGroup();
            group.GroupName = groupName;
            group.GroupDesc = groupDesc;
            Groups.Add(group);
            AssetBundleCollectorSettingData.SetDirty();
            return group;
        }

        public void RemoveGroup(AssetBundleCollectorGroup group)
        {
            if(Groups.Remove(group))
            {
                AssetBundleCollectorSettingData.SetDirty();
            }
            else
            {
                Debug.LogWarning($"Failed to remove AssetBundleCollectorGroup: {group.GroupName}");
            }
        }
    }
}