using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using System;
using UnityEditor.Build.Pipeline.Interfaces;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [TaskAttribute("Step4. 更新构建数据")]
    public class TaskUpdateBuildInfo : IGameBuildTask
    {
        void IGameBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var buildBundleResults = context.GetContextObject<BuildResultContext>().Results;

            var bundleOutput = buildParametersContext.GetCacheBundlesOutput();
            foreach (var bundleInfo in buildMapContext.BuildBundleInfos)
            {
                string filePath = $"{bundleOutput}/{bundleInfo.BundleName}";
                if (filePath.Length >= 260)
                    throw new Exception($"输出的字符串长度过长 {filePath.Length}: {filePath}");
            }

            foreach (var bundleInfo in buildMapContext.BuildBundleInfos)
            {
                bundleInfo.PatchInfo.BuildOutputFilePath = $"{bundleOutput}/{bundleInfo.BundleName}";
            }

            foreach (var bundleInfo in buildMapContext.BuildBundleInfos)
            {
                string buildOutputFilePath = bundleInfo.PatchInfo.BuildOutputFilePath;
                bundleInfo.PatchInfo.ContentHash = GetBundleContentHash(bundleInfo, buildMapContext, buildBundleResults);
                bundleInfo.PatchInfo.PatchFileHash = GetBundleFileHash(buildOutputFilePath);
                bundleInfo.PatchInfo.PatchFileCRC = GetBundleFileCRC(buildOutputFilePath);
                bundleInfo.PatchInfo.PatchFileSize = GetBundleFileSize(buildOutputFilePath);
            }

            int outputNameStyle = (int)buildParametersContext.gameBuilderSetting.bundleSetting.nameStyle;
            string cacheStreaingOutput = buildParametersContext.GetCacheStreamingOutput();
            foreach (var bundleInfo in buildMapContext.BuildBundleInfos)
            {
                string patchFileName = AssetManifest.CreateBundleFileName(outputNameStyle, bundleInfo.BundleName, bundleInfo.PatchInfo.PatchFileHash);
                bundleInfo.PatchInfo.PatchOutputFilePath = $"{cacheStreaingOutput}/{patchFileName}";
            }
        }

        static private string GetBundleContentHash(BuildBundleInfo bundleInfo, BuildMapContext context, IBundleBuildResults results)
        {
            if (bundleInfo.IsRawFile)
            {
                string filePath = bundleInfo.PatchInfo.BuildOutputFilePath;
                return HashUtility.FileMD5(filePath);
            }

            if (results.BundleInfos.TryGetValue(bundleInfo.BundleName, out var value))
                return value.Hash.ToString();
            else
                throw new Exception($"Not found bundle in build result : {bundleInfo.BundleName}");
        }

        static private string GetBundleFileHash(string filePath)
        {
            return HashUtility.FileMD5(filePath);
        }
        static private string GetBundleFileCRC(string filePath)
        {
            return HashUtility.FileCRC32(filePath);
        }
        static private long GetBundleFileSize(string filePath)
        {
            return FileUtility.GetFileSize(filePath);
        }
    }
}