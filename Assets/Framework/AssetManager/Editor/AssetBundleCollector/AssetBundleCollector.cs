using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Text;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [Serializable]
    public class AssetBundleCollector
    {
        /// <summary>
        /// 收集路径GUID，文件夹或单个资源文件
        /// </summary>
        public string           CollectGUID;

        public string           CollectPath     { get { return AssetDatabase.GUIDToAssetPath(CollectGUID);  } }

        public ECollectorType   CollectorType   = ECollectorType.MainAssetCollector;

        public string           PackRuleName    = nameof(PackDirectory);

        public string           FilterRuleName  = nameof(CollectAll);

        public bool IsValid()
        {
            if(string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(CollectGUID)))
                return false;

            if (CollectorType == ECollectorType.None)
                return false;

            if (AssetBundleCollectorSettingData.HasPackRuleName(PackRuleName) == false)
                return false;

            if (AssetBundleCollectorSettingData.HasFilterRuleName(FilterRuleName) == false)
                return false;

            return true;
        }

        /// <summary>
        /// 检测配置错误
        /// </summary>
        public void CheckConfigError()
        {
            string assetGUID = AssetDatabase.AssetPathToGUID(CollectPath);
            if (string.IsNullOrEmpty(assetGUID))
                throw new Exception($"Invalid collect path : {CollectPath}");

            if (CollectorType == ECollectorType.None)
                throw new Exception($"{nameof(ECollectorType)}.{ECollectorType.None} is invalid in collector : {CollectPath}");

            if (AssetBundleCollectorSettingData.HasPackRuleName(PackRuleName) == false)
                throw new Exception($"Invalid {nameof(IPackRule)} class type : {PackRuleName} in collector : {CollectPath}");

            if (AssetBundleCollectorSettingData.HasFilterRuleName(FilterRuleName) == false)
                throw new Exception($"Invalid {nameof(IFilterRule)} class type : {FilterRuleName} in collector : {CollectPath}");
        }

        public List<CollectAssetInfo> GetAllCollectAssets(AssetBundleCollectorGroup group)
        {
            Dictionary<string, CollectAssetInfo> result = new Dictionary<string, CollectAssetInfo>(1000);
            bool isRawAsset = PackRuleName == nameof(PackRawFile);

            // 检测原生资源包的收集器类型
            if (isRawAsset && CollectorType != ECollectorType.MainAssetCollector)
                throw new Exception($"The raw file must be set to {nameof(ECollectorType)}.{ECollectorType.MainAssetCollector} : {CollectPath}");

            if (string.IsNullOrEmpty(CollectPath))
                throw new Exception($"The collect path is null or empty in group : {group.GroupName}");

            // 收集打包资源
            if (AssetDatabase.IsValidFolder(CollectPath))
            {
                string collectDirectory = CollectPath;
                string[] findAssets = EditorTools.FindAssets(EAssetSearchType.All, collectDirectory);
                foreach (string assetPath in findAssets)
                {
                    if (IsValidateAsset(assetPath) && IsCollectAsset(assetPath))
                    {
                        if (result.ContainsKey(assetPath) == false)
                        {
                            var collectAssetInfo = CreateCollectAssetInfo(group, assetPath, isRawAsset);
                            result.Add(assetPath, collectAssetInfo);
                        }
                        else
                        {
                            throw new Exception($"The collecting asset file is existed : {assetPath} in collector : {CollectPath}");
                        }
                    }
                }
            }
            else
            {
                string assetPath = CollectPath;
                if (IsValidateAsset(assetPath) && IsCollectAsset(assetPath))
                {
                    var collectAssetInfo = CreateCollectAssetInfo(group, assetPath, isRawAsset);
                    result.Add(assetPath, collectAssetInfo);
                }
                else
                {
                    throw new Exception($"The collecting single asset file is invalid : {assetPath} in collector : {CollectPath}");
                }
            }            

            // 返回列表
            return result.Values.ToList();
        }

        private CollectAssetInfo CreateCollectAssetInfo(AssetBundleCollectorGroup group, string assetPath, bool isRawAsset)
        {
            string bundleName = GetBundleName(group, assetPath);
            CollectAssetInfo collectAssetInfo = new CollectAssetInfo(CollectorType, bundleName, assetPath, isRawAsset);
            collectAssetInfo.DependTree = CreateDependTree(assetPath);

            return collectAssetInfo;
        }

        private CollectAssetInfo.DependNode CreateDependTree(string mainAssetPath)
        {
            return CreateDependNode(mainAssetPath, null);
        }

        private CollectAssetInfo.DependNode CreateDependNode(string assetPath, CollectAssetInfo.DependNode parent)
        {
            CollectAssetInfo.DependNode node = new CollectAssetInfo.DependNode();
            node.assetPath = assetPath;
            node.parent = parent;

            // create child node
            string[] depends = AssetDatabase.GetDependencies(assetPath, false);     // 获取直接依赖
            List<string> dependAssetPaths = new List<string>();
            foreach (string path in depends)
            {
                if (IsValidateAsset(path, false))      // 获取直接依赖时，仅过滤指定的扩展名文件，不过滤指定文件夹下的文件
                {
                    // 排除主资源对象
                    if (path == assetPath)
                        continue;

                    dependAssetPaths.Add(path);
                }
            }
            
            foreach(var path in dependAssetPaths)
            {
                node.children.Add(CreateDependNode(path, node));
            }

            return node;
        }

        private bool IsCollectAsset(string assetPath)
        {
            IFilterRule filterRuleInstance = AssetBundleCollectorSettingData.GetFilterRuleInstance(FilterRuleName);
            return filterRuleInstance.IsCollectAsset(new FilterRuleData(assetPath));
        }

        private bool IsValidateAsset(string assetPath, bool considerByDirectory = true)
        {
            if (assetPath.StartsWith("Assets/") == false && assetPath.StartsWith("Packages/") == false)
            {
                UnityEngine.Debug.LogError($"Invalid asset path : {assetPath}");
                return false;
            }

            // 忽略文件夹
            if (AssetDatabase.IsValidFolder(assetPath))
                return false;

            // 忽略编辑器下的类型资源
            Type type = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            if (type == typeof(LightingDataAsset))
                return false;

            // 忽略Unity无法识别的无效文件
            // 注意：只对非原生文件收集器处理
            if (PackRuleName != nameof(PackRawFile))
            {
                if (type == typeof(UnityEditor.DefaultAsset))
                {
                    UnityEngine.Debug.LogWarning($"Cannot pack default asset : {assetPath}");
                    return false;
                }
            }

            // 不收集指定扩展名和特定文件夹下的资源
            if (IsIgnoreFileByExtension(assetPath))
                return false;
            if (considerByDirectory && IsIgnoreFileByDirectory(assetPath))
                return false;

            return true;
        }

        private bool IsIgnoreFileByExtension(string assetPath)
        {
            string fileExtension = System.IO.Path.GetExtension(assetPath);
            foreach (var extension in AssetBundleCollectorSetting.IgnoreFileExtensions)
            {
                if (extension == fileExtension)
                    return true;
            }

            return false;
        }

        private bool IsIgnoreFileByDirectory(string assetPath)
        {
            string directory = EditorTools.GetRegularPath(System.IO.Path.GetDirectoryName(assetPath));
            string[] splits = directory.Split("/", StringSplitOptions.RemoveEmptyEntries);
            foreach (var ignoreDir in AssetBundleCollectorSetting.IgnoreDirectoryName)
            {
                foreach (var dir in splits)
                {
                    if (string.Compare(dir, ignoreDir, true) == 0)
                        return true;
                }
            }

            return false;
        }

        private string GetBundleName(AssetBundleCollectorGroup group, string assetPath)
        {
            // 根据规则设置获取资源包名称
            IPackRule packRuleInstance = AssetBundleCollectorSettingData.GetPackRuleInstance(PackRuleName);
            string bundleName = packRuleInstance.GetBundleName(new PackRuleData(assetPath, CollectPath, group.GroupName));
            return EditorTools.GetRegularPath(bundleName).ToLower();
        }
    }

    public enum ECollectorType
    {
        /// <summary>
        /// 收集参与打包的主资源，写入资源清单列表，可通过代码加载
        /// </summary>
        MainAssetCollector,

        /// <summary>
        /// 收集参与打包的主资源，不写入资源清单列表，不可通过代码加载
        /// </summary>
        StaticAssetCollector,

        /// <summary>
        /// 收集参与打包的依赖资源，不写入资源清单列表，不可通过代码加载，如果没有被主资源对象引用，则不参与构建
        /// </summary>
        DependAssetCollector,

        /// <summary>
        /// 不由收集器负责收集，解析过程中程序化生成
        /// </summary>
        None
    }
}