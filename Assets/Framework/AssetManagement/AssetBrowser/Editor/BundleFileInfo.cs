using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;

namespace Framework.AssetBrowser
{
    internal class BundleFileInfo
    {
        public string                       name;                       // display name

        public string                       hashName;                   // to generate hash code

        public int                          depth;

        public string                       guid;                       // 记录abName对应的路径guid，可能无效，取决于ab name设置规则

        public string                       assetBundleName;            // 如果treeElement对应着ab则记录abName，否则为空

        public long                         size;

        public bool                         includeScene;

        public string[]                     dependentOnBundleList;      // 依赖的bundle列表

        public List<AssetFileInfo>          includedAssetFileList;      // 包含的资产列表

        public bool isBundle
        {
            get
            {
                return !string.IsNullOrEmpty(assetBundleName);
            }
        }

        /// <summary>
        /// 是否有包含内置资产或外部资产的情况
        /// </summary>
        public bool isValid
        {
            get
            {
                if (includedAssetFileList == null || includedAssetFileList.Count == 0)
                    return true;

                foreach (var file in includedAssetFileList)
                {
                    if (!file.isValid)
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 是否有missing reference
        /// </summary>
        public bool hasMissingReference
        {
            get
            {
                if (includedAssetFileList == null || includedAssetFileList.Count == 0)
                    return false;

                foreach (var file in includedAssetFileList)
                {
                    if (file.hasMissingReference)
                        return true;
                }
                return false;
            }
        }

        public bool allSceneOrNot
        {
            get
            {
                return includedAssetFileList.Where(item => item.isScene).Count() == includedAssetFileList.Count 
                    || includedAssetFileList.Where(item => !item.isScene).Count() == includedAssetFileList.Count;
            }
        }


        private static Dictionary<string, BundleFileInfo> m_CombinedNameToElement = new Dictionary<string, BundleFileInfo>();

        internal static string[] PreParse()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
            m_CombinedNameToElement.Clear();

            return AssetDatabase.GetAllAssetBundleNames();
        }

        internal static void ParseBundle(ref List<BundleFileInfo> bundleList, string abName)
        {
            if(bundleList == null)
            {
                bundleList = new List<BundleFileInfo>();
            }
            bundleList.AddRange(ParseBundle(abName));
        }

        internal static void PostParse()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }



        internal static List<BundleFileInfo> ParseAllBundles()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();

            m_CombinedNameToElement.Clear();
            List<BundleFileInfo> bundleList = new List<BundleFileInfo>();
            string[] abNames = AssetDatabase.GetAllAssetBundleNames();
            foreach (var abName in abNames)
            {
                bundleList.AddRange(ParseBundle(abName));
            }
            return bundleList;
        }

        private static List<BundleFileInfo> ParseBundle(string abName)
        {
            List<BundleFileInfo> bundleList = new List<BundleFileInfo>();       // abName可能被解析为多个bundle节点

            // split
            string[] pathList = abName.Split('/');

            // create tree element
            string combinedName = string.Empty;
            int parentDepth = -1;
            for (int i = 0; i < pathList.Length; ++i)
            {
                combinedName += pathList[i] + "/";

                string nameWithoutLastSlash = combinedName.Substring(0, combinedName.Length - 1);

                string nameWithoutExtensionAndLastSlash = nameWithoutLastSlash;
                string ext = Path.GetExtension(nameWithoutLastSlash);
                if (!string.IsNullOrEmpty(ext))
                {
                    nameWithoutExtensionAndLastSlash = nameWithoutExtensionAndLastSlash.Substring(0, nameWithoutExtensionAndLastSlash.Length - ext.Length);
                }

                BundleFileInfo parentElement;
                if (m_CombinedNameToElement.TryGetValue(nameWithoutLastSlash, out parentElement))
                {
                    parentDepth = parentElement.depth;
                    continue;
                }

                BundleFileInfo bundleFileInfo = new BundleFileInfo();
                bundleFileInfo.name = pathList[i];
                bundleFileInfo.hashName = nameWithoutLastSlash;
                bundleFileInfo.depth = ++parentDepth;
                bundleFileInfo.guid = AssetDatabase.AssetPathToGUID(nameWithoutExtensionAndLastSlash);
                bundleFileInfo.assetBundleName = (i == pathList.Length - 1 ? nameWithoutLastSlash : null);
                bundleFileInfo.size = GetTotalSize(bundleFileInfo.assetBundleName);
                bundleFileInfo.dependentOnBundleList = string.IsNullOrEmpty(bundleFileInfo.assetBundleName) ? new string[0] : AssetBrowserUtil.GetAllAssetBundleDependencies(bundleFileInfo.assetBundleName);
                ParseAllAssets(ref bundleFileInfo);

                //Debug.Log($"{bundleFileInfo.depth}  {bundleFileInfo.hashName}");
                bundleList.Add(bundleFileInfo);
                m_CombinedNameToElement.Add(nameWithoutLastSlash, bundleFileInfo);
            }

            return bundleList;
        }

        private static void ParseAllAssets(ref BundleFileInfo bundleFileInfo)
        {
            if (bundleFileInfo == null || string.IsNullOrEmpty(bundleFileInfo.assetBundleName))
                return;

            bundleFileInfo.includedAssetFileList = new List<AssetFileInfo>();

            string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundleFileInfo.assetBundleName);
            foreach (string assetPath in assetPaths)
            {
                AssetFileInfo afi = new AssetFileInfo();
                ParseAsset(assetPath, ref afi);
                bundleFileInfo.includedAssetFileList.Add(afi);
            }
            bundleFileInfo.includeScene = bundleFileInfo.includedAssetFileList.Find(item => item.isScene) != null;
        }

        static internal void ParseAsset(string assetPath, ref AssetFileInfo afi)
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset == null)
                return;

            afi.name = Path.GetFileName(assetPath);
            afi.assetPath = assetPath;
            FileInfo fi = new FileInfo(assetPath);
            afi.size = fi != null ? fi.Length : 0;
            afi.isScene = asset.GetType().Name == "SceneAsset";
            AssetBrowserUtil.FindMissingReference(asset, out afi.missingReferenceError);

            // add ReferencedObjectInfo list
            afi.dependentOn = new List<ReferencedObjectInfo>();
            UnityEngine.Object[] objs = UnityEditor.EditorUtility.CollectDependencies(new UnityEngine.Object[] { asset });
            foreach (UnityEngine.Object obj in objs)
            {
                ReferencedObjectInfo roi = new ReferencedObjectInfo();
                roi.name = obj.name;
                roi.type = obj.GetType();
                roi.assetPath = AssetDatabase.GetAssetPath(obj);
                roi.isBuiltIn = AssetImporter.GetAtPath(roi.assetPath) == null;         // 内置资产无法获取importer
                if (roi.type.Name == "MonoScript" || roi.isBuiltIn)
                { // 脚本或内置资产跳过此项检测
                    roi.isExternal = false;
                }
                else
                {
                    roi.assetBundleName = AssetDatabase.GetImplicitAssetBundleName(roi.assetPath);
                    if (ShouldExclude(roi.assetPath))
                        roi.isExternal = false;
                    else
                        roi.isExternal = string.IsNullOrEmpty(roi.assetBundleName);
                }

                //Debug.Log($"        {roi.name}  {roi.type}  {roi.assetPath}  {roi.isBuiltIn}    {roi.isExternal}");
                afi.dependentOn.Add(roi);
            }
        }

        static private string[] kExcludedList = new string[] { "Assets/WGame/Data/UI/Original" };
        static private bool ShouldExclude(string assetPath)
        {
#if UNITY_2019_1_OR_NEWER            
            return false;
#else
            foreach (string excludedPath in kExcludedList)
            {
                if (assetPath.ToLower().StartsWith(excludedPath.ToLower()))
                    return true;
            }
            return false;
#endif
        }

        static internal void CollectIssues(ref List<BundleFileInfo> bundleList)
        {
            IssueCollection.Clear();

            foreach(var bundleInfo in bundleList)
            {
                if (bundleInfo.includedAssetFileList == null || bundleInfo.includedAssetFileList.Count == 0)
                    continue;

                foreach(var assetInfo in bundleInfo.includedAssetFileList)
                {
                    if (assetInfo.dependentOn == null || assetInfo.dependentOn.Count == 0 || assetInfo.isValid)
                        continue;

                    foreach(var roi in assetInfo.dependentOn)
                    {
                        if(roi.isBuiltIn || roi.isExternal)
                        {
                            IssueCollection.AddIssue(roi, assetInfo);
                        }
                    }
                }
            }
        }

        static private long GetTotalSize(string assetBundleName)
        {
            if (string.IsNullOrEmpty(assetBundleName))
                return 0;

            long totalSize = 0;

            string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
            foreach (string assetPath in assetPaths)
            {
                FileInfo fi = new FileInfo(assetPath);
                if (fi.Exists)
                    totalSize += fi.Length;
            }
            return totalSize;
        }
    }

    internal class AssetFileInfo
    {
        public string                       name;

        public string                       assetPath;

        public long                         size;

        public string                       missingReferenceError;

        public bool                         isScene;
        
        public List<ReferencedObjectInfo>   dependentOn;

        /// <summary>
        /// 是否为有效资产
        /// </summary>
        public bool isValid
        {
            get
            {
                if (dependentOn == null || dependentOn.Count == 0)
                    return true;

                // TODO: 暂时屏蔽scene检查，始终返回true
                if (isScene)
                    return true;

                foreach (ReferencedObjectInfo doi in dependentOn)
                {
                    if (!doi.isValid)
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 是否有missing reference
        /// </summary>
        public bool hasMissingReference
        {
            get
            {
                return !string.IsNullOrEmpty(missingReferenceError);
            }
        }
    }

    internal class ReferencedObjectInfo
    {
        public string                       name;

        public string                       assetPath;

        public Type                         type;

        public bool                         isBuiltIn;              // 内置资产

        public bool                         isExternal;             // 外部资源，即没有被打包的资产

        /// <summary>
        /// 不是内置资产，也不是外部资产即为有效资产
        /// </summary>
        public bool                         isValid
        {
            get
            {
                return !isBuiltIn && !isExternal;
            }
        }

        public string                       assetBundleName;        // 资源所属AB name
    }
}