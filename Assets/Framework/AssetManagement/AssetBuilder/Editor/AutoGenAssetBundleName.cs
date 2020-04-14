using System.IO;
using UnityEditor;

namespace AssetManagement.AssetBuilder
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
            for (int i = 0; i < importedAssets.Length; ++i)
            {
                UpdateAssetBundleName(importedAssets[i]);
            }

            for (int i = 0; i < deletedAssets.Length; ++i)
            {
                UpdateAssetBundleName(deletedAssets[i]);
            }

            for (int i = 0; i < movedAssets.Length; ++i)
            {
                UpdateAssetBundleName(movedAssets[i]);
            }

            for (int i = 0; i < movedFromAssetPaths.Length; ++i)
            {
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
            // step 1. 屏蔽指定路径
            if (!AssetBuilderUtil.IsPassByWhiteList(assetPath) || AssetBuilderUtil.IsBlockedByBlackList(assetPath))
            {
                // cs不可以设置assetBundleName
                if (Path.GetExtension(assetPath) == ".cs")
                    return;

                // 清空屏蔽文件的ab name
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null)
                {
                    importer.assetBundleName = "";
                }

                return;
            }

            // step 2. skip directory
            bool isDirectory = string.IsNullOrEmpty(Path.GetExtension(assetPath));              // 依据后缀名判定是否是文件夹
            if (isDirectory)
            { // 无论增、删文件夹都不处理，只有当文件夹内有文件增、删等操作才处理
                return;
            }
            else
            {
                // 清除文件的ab name
                AssetImporter ai = AssetImporter.GetAtPath(assetPath);
                if (ai != null)
                    ai.assetBundleName = "";
            }

            // step 3. 统计非meta文件数量
            string directory = assetPath.Substring(0, assetPath.LastIndexOf("/"));              // 找到文件所在的文件夹

            AssetImporter ti = AssetImporter.GetAtPath(directory);
            if (ti == null)
                return;             // 文件夹可能不存在

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
                ti.assetBundleName = "";
            }
            else
            {
                ti.assetBundleName = directory.ToLower() + ".ab";
            }
        }
    }
}