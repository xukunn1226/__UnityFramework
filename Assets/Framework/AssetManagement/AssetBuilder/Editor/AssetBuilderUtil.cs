using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace Framework.AssetManagement.AssetBuilder
{
    static internal class AssetBuilderUtil
    {
        // [MenuItem("Tools/Clear All BundleName")]
        static private void ClearAllBundleNames()
        {
            string[] guids = AssetDatabase.FindAssets("*");
            foreach(var guid in guids)
            {
                AssetImporter importer = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
                importer.assetBundleName = null;
            }
            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log($"------- Done: {guids.Length}");
        }

        // 返回资源所属的bundle name，如果不打包则返回null
        static public string GetAssetBundleName(string assetPath)
        {
            // 文件夹不设置bundle name
            if(AssetDatabase.IsValidFolder(assetPath))
                return null;

            if(AssetBuilderUtil.IsBlockedByExtension(assetPath))
                return null;

            if(AssetBuilderUtil.IsPassByWhiteList(assetPath) && !AssetBuilderUtil.IsBlockedByBlackList(assetPath))
            {
                string[] folderNames = assetPath.Substring(0, assetPath.LastIndexOf("/")).TrimEnd(new char[] { '/' }).Split('/');
                UnityEngine.Debug.Assert(folderNames.Length >= 2);

                return assetPath.Substring(0, assetPath.LastIndexOf("/")).ToLower();
            }

            return null;
        }

        /// <summary>
        /// 路径是否在白名单内
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns>true：在白名单内</returns>
        static internal bool IsPassByWhiteList(string assetPath)
        { // assetPath可能是文件或文件夹的
            string directory = Directory.Exists(assetPath) ? assetPath : assetPath.Substring(0, assetPath.LastIndexOf("/") + 1);
            directory = directory.TrimEnd(new char[] { '/' }) + "/";
            for (int i = 0; i < AssetBuilderSetting.GetDefault().WhiteListOfPath.Length; ++i)
            {
                string whitePath = AssetBuilderSetting.GetDefault().WhiteListOfPath[i];
                whitePath = whitePath.TrimEnd(new char[] { '/' }) + "/";
                if(directory.StartsWith(whitePath, true, System.Globalization.CultureInfo.CurrentCulture))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 文件夹是否在黑名单内
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns>true: 在黑名单内</returns>
        static internal bool IsBlockedByBlackList(string assetPath)
        {
            string directory = Directory.Exists(assetPath) ? assetPath : assetPath.Substring(0, assetPath.LastIndexOf("/") + 1);
            string[] folderNames = directory.TrimEnd(new char[] { '/' }).Split('/');
            foreach (string path in AssetBuilderSetting.GetDefault().BlackListOfFolder)
            {
                for (int i = 0; i < folderNames.Length; ++i)
                {
                    if (string.Compare(path, folderNames[i], true) == 0)
                        return true;
                }
            }
            return false;
        }

        static internal bool IsBlockedByExtension(string assetPath)
        {
            string ext = Path.GetExtension(assetPath);
            return AssetBuilderSetting.GetDefault().Extension.Count(t => string.Compare(t, ext, true) == 0) > 0;
        }

        static public string[] GetAllAssetBundleNames()
        {
            HashSet<string> set = new HashSet<string>();
            string[] guids = AssetDatabase.FindAssets("*", AssetBuilderSetting.GetDefault().WhiteListOfPath);
            foreach(var guid in guids)
            {
                string bundleName = AssetBuilderUtil.GetAssetBundleName(AssetDatabase.GUIDToAssetPath(guid).ToLower());
                if(string.IsNullOrEmpty(bundleName))
                    continue;
                set.Add(bundleName);
            }
            return set.ToArray();
        }

        // 根据扩展名筛选文件 e.g. ".fbx", ".prefab", ".asset", "*.*"
        static internal List<string> GetSelectedAllPaths(string extension, bool bConsiderFilter = false)
        {
            List<string> paths = new List<string>();
            bool bAll = string.Compare(extension, "*.*", System.StringComparison.OrdinalIgnoreCase) == 0 ? true : false;

            UnityEngine.Object[] objs = Selection.objects;
            foreach (UnityEngine.Object obj in objs)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (bConsiderFilter)
                {
                    if (!IsPassByWhiteList(path))
                        continue;
                }

                if (AssetDatabase.IsValidFolder(path))
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    FileInfo[] files = di.GetFiles(bAll ? extension : "*" + extension, SearchOption.AllDirectories);
                    for (int i = 0; i < files.Length; ++i)
                    {
                        path = files[i].FullName.Replace("\\", "/").Replace(UnityEngine.Application.dataPath, "Assets");
                        if (ValidExtension(path))
                        {
                            if (!bConsiderFilter || !IsBlockedByBlackList(path))
                                paths.Add(path);
                        }
                    }
                }
                else
                {
                    if (bAll || path.IndexOf(extension, System.StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        if (ValidExtension(path))
                        {
                            if (!bConsiderFilter || !IsBlockedByBlackList(path))
                                paths.Add(path);
                        }
                    }
                }
            }

            return paths;
        }

        static private bool ValidExtension(string filePath)
        {
            if (filePath.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase) || filePath.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase))
                return false;
            return true;
        }


        // 复制资源路径到剪贴板，方便策划配置使用
        [MenuItem("Assets/AssetBuilder/Copy Path(ToLower) &c", false, 20)]
        static void FetchAssetPath()
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID).ToLower();

            EditorGUIUtility.systemCopyBuffer = assetPath;
        }

        [MenuItem("Assets/AssetBuilder/Copy AssetBundle Name(ToLower) &v", false, 21)]
        static void FetchAssetBundleName()
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID).ToLower();

            string directory = assetPath;
            if (!string.IsNullOrEmpty(Path.GetExtension(assetPath)))
            {
                directory = assetPath.Substring(0, assetPath.LastIndexOf("/"));
            }

            AssetImporter ti = AssetImporter.GetAtPath(directory);
            if (ti == null || string.IsNullOrEmpty(ti.assetBundleName))
            {
                Debug.LogWarningFormat("Failed to fetch asset bundle name, plz contact programmer...");
                return;
            }

            EditorGUIUtility.systemCopyBuffer = ti.assetBundleName;
            Debug.Log($"BundleName is: {ti.assetBundleName}");
        }

        [MenuItem("Assets/AssetBuilder/Copy Asset Name(ToLower) &b", false, 22)]
        static void FetchAssetName()
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID).ToLower();

            if (string.IsNullOrEmpty(Path.GetExtension(assetPath)))
            {
                Debug.LogWarning("Failed to fetch asset name, because it is a directory.");
                return;
            }

            EditorGUIUtility.systemCopyBuffer = Path.GetFileName(assetPath);
            Debug.Log($"AssetName is: {EditorGUIUtility.systemCopyBuffer}");
        }

        //[MenuItem("Tools/Print PackType")]
        static void PrintPackType()
        {
            string[] guids = AssetDatabase.FindAssets("*", AssetBuilderSetting.GetDefault().WhiteListOfPath);
            foreach(var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid).ToLower();
                if (AssetBuilderUtil.IsBlockedByBlackList(assetPath))
                    continue;

                string packPath;
                AssetBuilderSetting.PackType type = AssetBuilderSetting.GetDefault().GetPackType(assetPath, out packPath);
                if(type == AssetBuilderSetting.PackType.Pack_ByFile)
                {
                    Debug.Log($"ByFile: {assetPath}");
                }
                if(type == AssetBuilderSetting.PackType.Pack_BySize)
                {
                    Debug.Log($"BySize: {assetPath}");
                }
                if(type == AssetBuilderSetting.PackType.Pack_ByTopFolder)
                {
                    Debug.Log($"ByTopFolder: {assetPath}");
                }
            }
        }
    }
}