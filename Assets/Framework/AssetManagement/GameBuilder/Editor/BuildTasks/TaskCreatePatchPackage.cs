using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [TaskAttribute("Step7. 创建补丁包")]
    public class TaskCreatePatchPackage : IGameBuildTask
    {
        void IGameBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var manifestContext = context.GetContextObject<AssetManifestContext>();
            var packageVersion = buildParametersContext.GetPackageVersion();
            string cacheBundlesOutput = buildParametersContext.GetCacheBundlesOutput();
            string cacheStreamingOutput = buildParametersContext.GetCacheStreamingOutput();

            // 拷贝补丁清单文件
            {
                string fileName = AssetManagerSettingsData.GetManifestBinaryFileName(packageVersion);
                string sourcePath = $"{cacheBundlesOutput}/{fileName}";
                string destPath = $"{cacheStreamingOutput}/{fileName}";
                EditorTools.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝补丁清单哈希文件
            {
                string fileName = AssetManagerSettingsData.GetManifestHashFileName(packageVersion);
                string sourcePath = $"{cacheBundlesOutput}/{fileName}";
                string destPath = $"{cacheStreamingOutput}/{fileName}";
                EditorTools.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝补丁清单版本文件
            {
                string fileName = AssetManagerSettingsData.GetManifestVersionFileName();
                string sourcePath = $"{cacheBundlesOutput}/{fileName}";
                string destPath = $"{cacheStreamingOutput}/{fileName}";
                EditorTools.CopyFile(sourcePath, destPath, true);
            }

            // 拷贝文件列表（所有文件）
            foreach (var patchBundle in manifestContext.Manifest.BundleList)
            {
                string sourcePath = $"{cacheBundlesOutput}/{patchBundle.bundleName}";
                string destPath = $"{cacheStreamingOutput}/{patchBundle.fileName}";
                EditorTools.CopyFile(sourcePath, destPath, true);
            }

            BuildRunner.Log($"创建补丁包完成：{cacheStreamingOutput}");
        }
    }
}