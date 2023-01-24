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

            //string error;
            //if (IsValid(out error) == false)
            //{
            //    if (string.IsNullOrEmpty(error) == false)
            //    {
            //        Debug.LogError(error);
            //    }
            //    return result.Values.ToList();
            //}

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

            // 分析依赖资源不在收集列表中的情况
            //string info = ParseCollectResults(result);
            //if(string.IsNullOrEmpty(info) == false)
            //{
            //    Debug.LogWarning(info);
            //}

            // 返回列表
            return result.Values.ToList();
        }

        ///// <summary>
        ///// 判断配置的合法性
        ///// 原则：同一个资源不能被多个收集器收集
        ///// </summary>
        ///// <returns></returns>
        //public bool IsValid(out string error)
        //{
        //    StringBuilder sb = new StringBuilder();

        //    Dictionary<string, AssetBundleCollector> result = new Dictionary<string, AssetBundleCollector>(10000);      // key: assetPath; value: collector

        //    foreach (var collector in Collectors)
        //    {
        //        var temper = collector.GetAllCollectAssets(this);
        //        foreach (var assetInfo in temper)
        //        {
        //            if (result.ContainsKey(assetInfo.AssetPath) == false)
        //                result.Add(assetInfo.AssetPath, collector);
        //            else
        //            {
        //                sb.AppendLine($"{assetInfo.AssetPath}: The collecting asset file is coexisted [{collector.CollectPath}] and [{result[assetInfo.AssetPath].CollectPath}]");
        //            }
        //        }
        //    }
        //    error = sb.ToString();            
        //    return string.IsNullOrEmpty(error);
        //}

        //public string ParseCollectResults(Dictionary<string, CollectAssetInfo> results)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    foreach(var pair in results)
        //    {
        //        foreach(var assetPath in pair.Value.DependAssets)
        //        {
        //            if(results.ContainsKey(assetPath) == false)
        //            {
        //                sb.AppendLine($"{assetPath}: depend of {pair.Value.AssetPath} is not included any collector");
        //            }
        //        }
        //    }
        //    return sb.ToString();
        //}
    }
}