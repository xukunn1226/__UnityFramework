using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.AssetManagement.Runtime;
using System;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [TaskAttribute("Step6. 创建清单文件")]
    public class TaskCreateManifest : IGameBuildTask
    {
        void IGameBuildTask.Run(BuildContext context)
        {
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            string cacheBundlesOutput = buildParametersContext.GetCacheBundlesOutput();

            // 创建清单文件
            AssetManifest manifest = new AssetManifest();
            manifest.SerializedVersion = 1;
            manifest.PackageVersion = buildParametersContext.GetPackageVersion();
            manifest.OutputNameStyle = 1;

            foreach (var bundleInfo in buildMapContext.BuildBundleInfos)
            {
                BundleDescriptor desc = new BundleDescriptor();
                desc.bundleName = bundleInfo.BundleName;
                desc.fileHash = bundleInfo.PatchInfo.PatchFileHash;
                desc.fileCRC = bundleInfo.PatchInfo.PatchFileCRC;
                desc.fileSize = bundleInfo.PatchInfo.PatchFileSize;
                desc.isRawFile = bundleInfo.IsRawFile;
                desc.loadMethod = 0;
                manifest.BundleList.Add(desc);
            }

            foreach (var bundleInfo in buildMapContext.BuildBundleInfos)
            {
                foreach (var assetInfo in bundleInfo.BuildinAssets)
                {
                    var assetDesc = new AssetDescriptor();
                    assetDesc.assetPath = assetInfo.AssetPath.ToLower();

                    // find main bundle
                    int index = manifest.BundleList.FindIndex(item => item.bundleName == assetInfo.MainBundleName);
                    if (index == -1)
                        throw new Exception($"should never get here! Can't find bundle {assetInfo.MainBundleName} from BundleList");
                    assetDesc.bundleID = index;

                    // fill depend bundles
                    List<int> dependIDs = new List<int>();
                    foreach (var dependBundleName in assetInfo.AllDependBundleNames)
                    {
                        index = manifest.BundleList.FindIndex(item => item.bundleName == dependBundleName);
                        if (index == -1)
                            throw new Exception($"should never get here! Can't find bundle {dependBundleName} from BundleList");
                        dependIDs.Add(index);
                    }
                    assetDesc.dependIDs = dependIDs.ToArray();
                    manifest.AssetList.Add(assetDesc);
                }
            }

            // 创建清单文件文本
            string fileName = AssetManagerSettingsData.GetManifestJsonFileName(buildParametersContext.GetPackageVersion());
            string filePath = $"{cacheBundlesOutput}/{fileName}";
            AssetManifest.SerializeToJson(filePath, manifest);
            BuildRunner.Log($"创建补丁清单文件：{filePath}");

            // 创建清单二进制文件
            fileName = AssetManagerSettingsData.GetManifestBinaryFileName(buildParametersContext.GetPackageVersion());
            filePath = $"{cacheBundlesOutput}/{fileName}";
            AssetManifest.SerializeToBinary(filePath, manifest);
            string packageHash = HashUtility.FileMD5(filePath);
            BuildRunner.Log($"创建补丁清单文件：{filePath}");

            AssetManifestContext patchManifestContext = new AssetManifestContext();
            byte[] bytesData = FileUtility.ReadAllBytes(filePath);
            patchManifestContext.Manifest = AssetManifest.DeserializeFromBinary(bytesData);
            context.SetContextObject(patchManifestContext);

            // 创建清单的哈希文件
            fileName = AssetManagerSettingsData.GetManifestHashFileName(buildParametersContext.GetPackageVersion());
            filePath = $"{cacheBundlesOutput}/{fileName}";
            FileUtility.CreateFile(filePath, packageHash);
            BuildRunner.Log($"创建补丁清单哈希文件：{filePath}");

            // 创建清单版本文件
            fileName = AssetManagerSettingsData.GetManifestVersionFileName();
            filePath = $"{cacheBundlesOutput}/{fileName}";
            FileUtility.CreateFile(filePath, buildParametersContext.GetPackageVersion());
            BuildRunner.Log($"创建补丁清单版本文件：{filePath}");
        }
    }
}