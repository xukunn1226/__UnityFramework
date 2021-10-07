using System.IO;
using UnityEditor;

namespace Framework.AssetManagement.AssetBuilder
{
    class AutoGenAssetBundleName : AssetPostprocessor
    {
        /// <summary>
        /// 检查文件及文件夹名不能有空格 && 自动生成AB包名
        /// 仅对特定路径下文件进行处理
        /// </summary>
        /// <param name="importedAssets"></param>
        /// <param name="deletedAssets"></param>
        /// <param name="movedAssets"></param>
        /// <param name="movedFromAssetPaths"></param>
        /// 添加操作：importedAssets包含所有子目录及文件，无序
        /// 删除操作：deletedAssets包含所有子目录下的文件及文件夹，无序
        /// 移动操作：movedAssets, movedFromAssetPaths从哪里移动至哪里
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if(UnityEngine.Application.isBatchMode)
                return;
                
            for (int i = 0; i < importedAssets.Length; ++i)
            {
                // UnityEngine.Debug.Log($"import: {importedAssets[i]}");
                UpdateAssetBundleName(importedAssets[i]);
            }

            for (int i = 0; i < deletedAssets.Length; ++i)
            {
                // UnityEngine.Debug.Log($"delete: {deletedAssets[i]}");
                UpdateAssetBundleName(deletedAssets[i]);
            }

            for (int i = 0; i < movedAssets.Length; ++i)
            {
                // UnityEngine.Debug.Log($"move: {movedAssets[i]}");
                UpdateAssetBundleName(movedAssets[i]);
            }

            for (int i = 0; i < movedFromAssetPaths.Length; ++i)
            {
                // UnityEngine.Debug.Log($"movedFrom: {movedFromAssetPaths[i]}");
                UpdateAssetBundleName(movedFromAssetPaths[i]);
            }
            AssetDatabase.RemoveUnusedAssetBundleNames();
        }

        /// <summary>
        /// 更新文件夹的assetbundle name（文件并不生成ab name）
        /// AB名生成规则：文件夹下有任何一个文件即需要生成AB name，否则不生成
        /// </summary>
        /// <param name="assetPath">文件的完整路径</param>
        static void UpdateAssetBundleName(string assetPath)
        {
            // step 1. skip those files which are not meet specification
            if(AssetBuilderUtil.IsBlockedByExtension(assetPath))
                return;

            //////////////////////////////////////// 文件符合规范，设置bundle name
            // step 2. 清除文件或文件夹的ab name
            AssetImporter ai = AssetImporter.GetAtPath(assetPath);
            if (ai != null)
                ai.assetBundleName = string.Empty;

            // step 3. 找到文件所在的文件夹
            string directory = Directory.Exists(assetPath) ? assetPath : assetPath.Substring(0, assetPath.LastIndexOf("/"));
            AssetImporter ti = AssetImporter.GetAtPath(directory);
            if (ti == null)
            {
                return;             // 文件夹可能不存在
            }

            // step 4. generate bundle name according to the directory
            string[] folderNames = directory.Split('/');
            if (folderNames.Length < 2)
                return;     // 资源不可直接放置Assets/下

            if (AssetBuilderUtil.IsSpecialFolderName(folderNames[folderNames.Length - 1]))
            { // 处理特殊子文件夹bundle name与父文件夹保持一致的情况
                string parentDirectory = directory.Substring(0, directory.LastIndexOf("/"));
                AssetImporter parentTi = AssetImporter.GetAtPath(parentDirectory);
                if (parentTi != null)
                {
                    ti.assetBundleName = IsMatchBundleNameRule(parentDirectory) ? parentTi.assetBundleName : string.Empty;
                }
            }
            else
            {
                int count = 0;          // 统计非meta文件数量
                string[] files = Directory.GetFiles(directory);
                foreach (string file in files)
                {
                    if (file.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    ++count;
                }

                // meta不能算是文件
                if (count == 0)
                {
                    ti.assetBundleName = string.Empty;
                }
                else
                {
                    ti.assetBundleName = IsMatchBundleNameRule(assetPath) ? directory.ToLower() + ".ab" : string.Empty;
                }
            }
        }

        static private bool IsMatchBundleNameRule(string assetPath)
        {
            return AssetBuilderUtil.IsPassByWhiteList(assetPath) && !AssetBuilderUtil.IsBlockedByBlackList(assetPath);
        }
    }
}