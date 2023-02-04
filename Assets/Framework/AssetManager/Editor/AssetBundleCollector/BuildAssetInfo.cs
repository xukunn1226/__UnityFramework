using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;

namespace Framework.AssetManagement.AssetEditorWindow
{
    public class BuildAssetInfo
    {
        /// <summary>
        /// 资源所属的资源包名
        /// </summary>
        public string MainBundleName { get; private set; }
        
        /// <summary>
        /// 收集器类型
        /// </summary>
        public ECollectorType CollectorType { private set; get; }

        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath { private set; get; }

        /// <summary>
        /// 是否为原生资源
        /// </summary>
        public bool IsRawAsset { private set; get; }

        /// <summary>
        /// 是否为着色器资源
        /// </summary>
        public bool IsShaderAsset { private set; get; }

        /// <summary>
        /// 依赖的所有资源
        /// </summary>
        public List<BuildAssetInfo> AllDependAssetInfos { private set; get; } = new List<BuildAssetInfo>();

        /// <summary>
        /// 依赖的所有资源包名
        /// </summary>
        public List<string> AllDependBundleNames { private set; get; } = new List<string>();

        public BuildAssetInfo(ECollectorType collectorType, string mainBundleName, string assetPath, bool isRawAsset)
        {
            MainBundleName = mainBundleName;
            CollectorType = collectorType;
            AssetPath = assetPath;
            IsRawAsset = isRawAsset;

            System.Type assetType = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            if (assetType == typeof(UnityEngine.Shader) || assetType == typeof(UnityEngine.ShaderVariantCollection))
                IsShaderAsset = true;
            else
                IsShaderAsset = false;
        }

        /// <summary>
        /// 设置所有依赖的资源
        /// </summary>
        public void SetAllDependAssetInfos(List<BuildAssetInfo> dependAssetInfos)
        {
            AllDependAssetInfos = dependAssetInfos;

            List<string> allDependBundleNames = new List<string>();
            foreach(var assetInfo in dependAssetInfos)
            {
                if (string.IsNullOrEmpty(assetInfo.MainBundleName))
                    throw new Exception($"should never get here");

                // 依赖资源与主资源在同一个资源包，跳过
                if(assetInfo.MainBundleName == MainBundleName)
                    continue;

                // 依赖资源包已统计，跳过
                if (allDependBundleNames.FindIndex(item => { return item == assetInfo.MainBundleName; }) != -1)
                    continue;

                allDependBundleNames.Add(assetInfo.MainBundleName);
            }
            AllDependBundleNames = allDependBundleNames;
        }

        /// <summary>
        /// 获取资源包名称
        /// </summary>
        public string GetBundleName()
        {
            return MainBundleName;
        }

        /// <summary>
        /// 计算主资源的完整包名
        /// </summary>
        public void CalculateFullBundleName()
        {
            if (IsRawAsset)
            {
                string mainBundleName = $"{MainBundleName}.{AssetManagerSettingsData.Setting.RawFileVariant}";
                MainBundleName = EditorTools.GetRegularPath(mainBundleName).ToLower();
            }
            else
            {
                string mainBundleName = $"{MainBundleName}.{AssetManagerSettingsData.Setting.AssetBundleFileVariant}";
                MainBundleName = EditorTools.GetRegularPath(mainBundleName).ToLower(); ;
            }
        }
    }
}