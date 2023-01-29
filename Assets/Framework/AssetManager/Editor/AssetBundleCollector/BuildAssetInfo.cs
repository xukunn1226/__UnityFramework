using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;

namespace Framework.AssetManagement.AssetEditorWindow
{
    public class BuildAssetInfo
    {
        private string                      m_MainBundleName;
        
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
        /// 注意：包括零依赖资源和冗余资源（资源包名无效）
        /// </summary>
        public List<BuildAssetInfo> AllDependAssetInfos { private set; get; }


        public BuildAssetInfo(ECollectorType collectorType, string mainBundleName, string assetPath, bool isRawAsset)
        {
            m_MainBundleName = mainBundleName;
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
            if (AllDependAssetInfos != null)
                throw new System.Exception("Should never get here !");

            AllDependAssetInfos = dependAssetInfos;
        }

        /// <summary>
        /// 资源包名是否存在
        /// </summary>
        public bool HasBundleName()
        {
            string bundleName = GetBundleName();
            if (string.IsNullOrEmpty(bundleName))
                return false;
            else
                return true;
        }

        /// <summary>
        /// 获取资源包名称
        /// </summary>
        public string GetBundleName()
        {
            return m_MainBundleName;
        }

        /// <summary>
        /// 计算主资源的完整包名
        /// </summary>
        public void CalculateFullBundleName()
        {
            if (IsRawAsset)
            {
                string mainBundleName = $"{m_MainBundleName}.{AssetManagerSettingsData.Setting.RawFileVariant}";
                m_MainBundleName = EditorTools.GetRegularPath(mainBundleName).ToLower();
            }
            else
            {
                string mainBundleName = $"{m_MainBundleName}.{AssetManagerSettingsData.Setting.AssetBundleFileVariant}";
                m_MainBundleName = EditorTools.GetRegularPath(mainBundleName).ToLower(); ;
            }
        }
    }
}