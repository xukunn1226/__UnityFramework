using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.AssetEditorWindow
{
    public class BuildMapContext : IContextObject
    {
        public string Error;

        public int AssetFileCount;

        public List<BuildBundleInfo> BuildBundleInfos = new List<BuildBundleInfo>(1000);

        /// <summary>
        /// 清理包含场景资源的资源包
        /// </summary>
        public void ClearSceneBundles()
        {
            foreach(var bundleInfo in BuildBundleInfos)
            {
                if (!bundleInfo.IsIncludeSceneAsset)
                    continue;
                bundleInfo.ClearNonSceneAsset();
            }
        }

        /// <summary>
		/// 添加一个打包资源
		/// </summary>
		public void PackAsset(BuildAssetInfo assetInfo)
        {
            string bundleName = assetInfo.MainBundleName;
            if (string.IsNullOrEmpty(bundleName))
                throw new System.Exception("Should never get here !");

            if (TryGetBundleInfo(bundleName, out BuildBundleInfo bundleInfo))
            {
                bundleInfo.PackAsset(assetInfo);
            }
            else
            {
                BuildBundleInfo newBundleInfo = new BuildBundleInfo(bundleName);
                newBundleInfo.PackAsset(assetInfo);
                BuildBundleInfos.Add(newBundleInfo);
            }
        }

        public bool TryGetBundleInfo(string bundleName, out BuildBundleInfo result)
        {
            foreach (var bundleInfo in BuildBundleInfos)
            {
                if (bundleInfo.BundleName == bundleName)
                {
                    result = bundleInfo;
                    return true;
                }
            }
            result = null;
            return false;
        }

        /// <summary>
		/// 获取构建管线里需要的数据
		/// </summary>
		public UnityEditor.AssetBundleBuild[] GetPipelineBuilds()
        {
            List<UnityEditor.AssetBundleBuild> builds = new List<UnityEditor.AssetBundleBuild>(BuildBundleInfos.Count);
            foreach (var bundleInfo in BuildBundleInfos)
            {
                if (bundleInfo.IsRawFile == false)
                    builds.Add(bundleInfo.CreatePipelineBuild());
            }
            return builds.ToArray();
        }
    }
}