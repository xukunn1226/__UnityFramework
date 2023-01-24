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
        private string                      m_ShareBundleName;
        private readonly HashSet<string>    m_ReferenceBundleNames = new HashSet<string>();

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

        public BuildAssetInfo(string assetPath)
        {
            CollectorType = ECollectorType.None;
            AssetPath = assetPath;
            IsRawAsset = false;

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
            if (CollectorType == ECollectorType.None)
                return m_ShareBundleName;
            else
                return m_MainBundleName;
        }

        /// <summary>
        /// 添加关联的资源包名称
        /// </summary>
        public void AddReferenceBundleName(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName))
                throw new Exception("Should never get here !");

            if (m_ReferenceBundleNames.Contains(bundleName) == false)
                m_ReferenceBundleNames.Add(bundleName);
        }

        /// <summary>
        /// 计算主资源或共享资源的完整包名
        /// </summary>
        public void CalculateFullBundleName()
        {
            if (CollectorType == ECollectorType.None)
            {
                if (IsRawAsset)
                    throw new Exception("Should never get here !");

                if (IsShaderAsset)
                {
                    string shareBundleName = AssetManagerSettingsData.GetUnityShadersBundleFullName();
                    m_ShareBundleName = EditorTools.GetRegularPath(shareBundleName).ToLower();
                }
                else
                {
                    if (m_ReferenceBundleNames.Count > 1)
                    {
                        IPackRule packRule = PackDirectory.StaticPackRule;
                        var bundleName = packRule.GetBundleName(new PackRuleData(AssetPath));
                        var shareBundleName = $"share_{bundleName}.{AssetManagerSettingsData.Setting.AssetBundleFileVariant}";
                        m_ShareBundleName = EditorTools.GetRegularPath(shareBundleName).ToLower();
                    }
                }

                //if (uniqueBundleName)
                //{
                //    if (string.IsNullOrEmpty(m_ShareBundleName) == false)
                //        m_ShareBundleName = $"{packageName.ToLower()}_{m_ShareBundleName}";
                //}
            }
            else
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
}