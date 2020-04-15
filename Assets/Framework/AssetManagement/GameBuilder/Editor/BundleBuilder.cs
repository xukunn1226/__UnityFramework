using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Framework.Core;

namespace Framework.GameBuilder
{
    public class BundleBuilder
    {
        /// <summary>
        /// 构建Bundles接口（唯一）
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        static public AssetBundleManifest BuildAssetBundles(BundleBuilderSetting para)
        {
            if (para == null)
            {
                Debug.LogError($"BundleBuilderSetting para == null");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                return null;
            }

            Debug.Log("Begin Build AssetBundles");

            // step 1. create directory
            string outputPath = para.outputPath.TrimEnd(new char[] { '/' }) + "/" + Utility.GetPlatformName();
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);
            Directory.CreateDirectory(outputPath);
            Debug.Log($"        Bundles Output: {outputPath}");

            // step 2. build bundles
            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputPath, para.GenerateOptions(), EditorUserBuildSettings.activeBuildTarget);
            Debug.Log($"        BuildAssetBundleOptions: {para.GenerateOptions()}");

            // step 3. copy bundles to streamingAssets
            if (manifest != null)
            {
                CopyAssetBundlesToStreamingAssets(outputPath);
                Debug.Log($"        Copy bundles to streaming assets");
            }

            Debug.Log($"        BundleSettings: {para.ToString()}");

            if(manifest != null)
            {
                Debug.Log($"End Build AssetBundles: Succeeded");
            }
            else
            {
                Debug.LogError($"End Build AssetBundles: Failed");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }

            return manifest;
        }

        static internal void CopyAssetBundlesToStreamingAssets(string output)
        {
            string srcPath = output;
            string targetPath = @"Assets/StreamingAssets";

            // 删除StreamingAssets
            if (Directory.Exists(targetPath))
            {
                Directory.Delete(targetPath, true);
            }
            else
            {
                Directory.CreateDirectory(targetPath);
            }
            Directory.CreateDirectory(targetPath + "/" + Utility.GetPlatformName());

            // 把源目录文件复制到目标目录
            if (Directory.Exists(srcPath))
            {
                CopyDirectory(srcPath, targetPath + "/" + Utility.GetPlatformName());
            }
            else
            {
                Debug.LogWarning("Source path does not exist!");
            }
            AssetDatabase.Refresh();
        }

        static private void CopyDirectory(string sourcePath, string destinationPath)
        {
            DirectoryInfo info = new DirectoryInfo(sourcePath);
            Directory.CreateDirectory(destinationPath);
            foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
            {
                string destName = Path.Combine(destinationPath, fsi.Name);

                if (fsi is System.IO.FileInfo)          //如果是文件，复制文件
                    File.Copy(fsi.FullName, destName);
                else                                    //如果是文件夹，新建文件夹，递归
                {
                    Directory.CreateDirectory(destName);
                    CopyDirectory(fsi.FullName, destName);
                }
            }
        }
    }
}