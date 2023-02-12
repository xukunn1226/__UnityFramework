using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.AssetManagement.Runtime;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [TaskAttribute("Step7. 资源包拷贝到StreamingAssets")]
    public class TaskCopyToStreamingAssets : IGameBuildTask
    {
        void IGameBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var manifestContext = context.GetContextObject<AssetManifestContext>();
            var packageVersion = buildParametersContext.GetPackageVersion();
            string cacheBundlesOutput = buildParametersContext.GetCacheBundlesOutput();
            string streamingAssetsDirectory = AssetBundleBuilderHelper.GetStreamingAssetsFolderPath();

            AssetBundleBuilderHelper.ClearStreamingAssetsFolder();

            // 拷贝补丁清单文件
            {
                string fileName = AssetManagerSettingsData.GetManifestBinaryFileName(packageVersion);
                string sourcePath = $"{cacheBundlesOutput}/{fileName}";
                string destPath = $"{streamingAssetsDirectory}/{fileName}";
                EditorTools.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝补丁清单哈希文件
            {
                string fileName = AssetManagerSettingsData.GetManifestHashFileName(packageVersion);
                string sourcePath = $"{cacheBundlesOutput}/{fileName}";
                string destPath = $"{streamingAssetsDirectory}/{fileName}";
                EditorTools.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝补丁清单版本文件
            {
                string fileName = AssetManagerSettingsData.GetManifestVersionFileName();
                string sourcePath = $"{cacheBundlesOutput}/{fileName}";
                string destPath = $"{streamingAssetsDirectory}/{fileName}";
                EditorTools.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝文件列表（所有文件）
            foreach (var patchBundle in manifestContext.Manifest.BundleList)
            {
                string sourcePath = $"{cacheBundlesOutput}/{patchBundle.fileName}";
                string destPath = $"{streamingAssetsDirectory}/{patchBundle.fileName}";
                EditorTools.CopyFile(sourcePath, destPath, true);
            }

            AssetDatabase.Refresh();
            BuildRunner.Log($"文件拷贝完成：{streamingAssetsDirectory}");
        }
    }
}