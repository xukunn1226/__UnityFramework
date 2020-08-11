using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace Framework.AssetManagement.AssetBuilder
{
    static internal class AssetBuilderUtil
    {
        /// <summary>
        /// 路径是否在白名单内
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns>true：在白名单内</returns>
        static internal bool IsPassByWhiteList(string assetPath)
        {
            for (int i = 0; i < AssetBuilderSetting.GetDefault().WhiteListOfPath.Length; ++i)
            {
                string whitePath = AssetBuilderSetting.GetDefault().WhiteListOfPath[i];
                whitePath = whitePath.TrimEnd(new char[] { '/' }) + "/";
                if (assetPath.Contains(whitePath))
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
            string[] folderNames = assetPath.Split('/');
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

        static internal bool IsSpecialFolderName(string folderName)
        {
            return AssetBuilderSetting.GetDefault().BundleNameWithParent.Count( t =>  string.Compare(t, folderName, true) == 0 ) > 0;
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
                        path = files[i].FullName.Replace("\\", "/").Replace(Application.dataPath, "Assets");
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
    }
}