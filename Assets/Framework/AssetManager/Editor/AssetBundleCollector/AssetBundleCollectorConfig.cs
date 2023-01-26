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
                            sb.AppendLine($"{assetInfo.AssetPath}: The collecting asset file is coexisted [{collector.CollectPath}] and [{result[assetInfo.AssetPath].CollectPath}]");
                        }
                    }
                }
            }
            return sb.ToString();
        }

        public List<CollectAssetInfo> GetAllCollectAssets()
        {
            List<CollectAssetInfo> result = new List<CollectAssetInfo>();
            foreach(var group in Groups)
            {
                result.AddRange(group.GetAllCollectAssets());
            }
            return result;
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