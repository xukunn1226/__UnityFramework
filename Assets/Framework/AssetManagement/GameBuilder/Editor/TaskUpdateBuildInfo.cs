using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build.Pipeline.Interfaces;
using Framework.AssetManagement.AssetEditorWindow;
using System;
using Framework.AssetManagement.Runtime;

namespace Framework.AssetManagement.AssetEditorWindow
{
    internal class TaskUpdateBuildInfo
    {
        static public void Run(string outputDirectory, BuildMapContext context, IBundleBuildResults results)
        {
            foreach (var bundleInfo in context.BuildBundleInfos)
            {
                string filePath = $"{outputDirectory}/{bundleInfo.BundleName}";
                if (filePath.Length >= 260)
                    throw new Exception($"输出的字符串长度过长 {filePath.Length}: {filePath}");
            }

            foreach (var bundleInfo in context.BuildBundleInfos)
            {
                bundleInfo.PatchInfo.BuildOutputFilePath = $"{outputDirectory}/{bundleInfo.BundleName}";
            }

            foreach (var bundleInfo in context.BuildBundleInfos)
            {
                string buildOutputFilePath = bundleInfo.PatchInfo.BuildOutputFilePath;
                bundleInfo.PatchInfo.ContentHash = GetBundleContentHash(bundleInfo, context, results);
                bundleInfo.PatchInfo.PatchFileHash = GetBundleFileHash(buildOutputFilePath);
                bundleInfo.PatchInfo.PatchFileCRC = GetBundleFileCRC(buildOutputFilePath);
                bundleInfo.PatchInfo.PatchFileSize = GetBundleFileSize(buildOutputFilePath);
            }

            int outputNameStyle = 1;
            foreach (var bundleInfo in context.BuildBundleInfos)
            {
                string patchFileName = AssetManifest.CreateBundleFileName(outputNameStyle, bundleInfo.BundleName, bundleInfo.PatchInfo.PatchFileHash);
                //bundleInfo.PatchInfo.PatchOutputFilePath = $"{packageOutputDirectory}/{patchFileName}";
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