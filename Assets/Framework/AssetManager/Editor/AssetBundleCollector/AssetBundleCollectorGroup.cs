using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [Serializable]
    public class AssetBundleCollectorGroup
    {
        public string GroupName;
        public string GroupDesc;
        public List<AssetBundleCollector> Collectors = new List<AssetBundleCollector>();

        /// <summary>
        /// 检测配置错误
        /// </summary>
        public void CheckConfigError()
        {
            foreach(var collector in Collectors)
            {
                collector.CheckConfigError();
            }
        }

        public List<CollectAssetInfo> GetAllCollectAssets()
        {
            Dictionary<string, CollectAssetInfo> result = new Dictionary<string, CollectAssetInfo>(10000);

            // 收集打包资源
            foreach (var collector in Collectors)
            {
                var temper = collector.GetAllCollectAssets(this);
                foreach (var assetInfo in temper)
                {
                    if (result.ContainsKey(assetInfo.AssetPath) == false)
                        result.Add(assetInfo.AssetPath, assetInfo);
                    else
                        throw new Exception($"The collecting asset file is existed : {assetInfo.AssetPath} in group : {GroupName}");
                }
            }

            // 返回列表
            return result.Values.ToList();
        }

        public string LogCollector(AssetBundleCollector collector)
        {
            StringBuilder sb = new StringBuilder();
            List<CollectAssetInfo> allAssetInfos = collector.GetAllCollectAssets(this);
            foreach(var assetInfo in allAssetInfos)
            {
                sb.AppendLine(assetInfo.LogInfo());
            }
            return sb.ToString();
        }

        public AssetBundleCollector AddCollector()
        {
            AssetBundleCollector collector = new AssetBundleCollector();
            Collectors.Add(collector);
            AssetBundleCollectorSettingData.SetDirty();
            return collector;
        }

        public void RemoveCollector(AssetBundleCollector collector)
        {
            if (Collectors.Remove(collector))
            {
                AssetBundleCollectorSettingData.SetDirty();
            }
            else
            {
                Debug.LogWarning($"Failed to remove AssetBundleCollector: {collector.CollectPath}");
            }
        }

        public AssetBundleCollector FindCollector(string collectPath)
        {
            return Collectors.Find(item => string.Compare(item.CollectPath, collectPath, true) == 0);
        }
    }
}