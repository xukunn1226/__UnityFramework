using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Framework.Core;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Content;
using UnityEditor.Build.Player;
using UnityEngine.Build.Pipeline;

namespace Framework.AssetManagement.GameBuilder
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

        static public AssetBundleManifest BuildAssetBundlesEx(BundleBuilderSetting para)
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
            // AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputPath, para.GenerateOptions(), EditorUserBuildSettings.activeBuildTarget);
            Debug.Log($"        BuildAssetBundleOptions: {para.GenerateOptions()}");
            if(!BuildBundleWithSBP(outputPath, para))
            {
                Debug.LogError($"End Build AssetBundles: Failed");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                return null;
            }

            // step 3. copy bundles to streamingAssets
            CopyAssetBundlesToStreamingAssets(outputPath);
            Debug.Log($"        Copy bundles to streaming assets");
            Debug.Log($"        BundleSettings: {para.ToString()}");
            Debug.Log($"End Build AssetBundles: Succeeded");

            return null;
        }


        class CustomBuildParameters : BundleBuildParameters
        {
            public Dictionary<string, UnityEngine.BuildCompression> PerBundleCompression { get; set; }

            public CustomBuildParameters(BuildTarget target, BuildTargetGroup group, string outputFolder) : base(target, group, outputFolder)
            {
                PerBundleCompression = new Dictionary<string, UnityEngine.BuildCompression>();
            }

            // Override the GetCompressionForIdentifier method with new logic
            public override UnityEngine.BuildCompression GetCompressionForIdentifier(string identifier)
            {
                UnityEngine.BuildCompression value;
                if (PerBundleCompression.TryGetValue(identifier, out value))
                    return value;
                return BundleCompression;
            }
        }

        static private bool BuildBundleWithSBP(string output, BundleBuilderSetting setting)
        {
            // step1. construct the new BundleBuildContent class
            AssetBundleBuild[] BuildList = ContentBuildInterface.GenerateAssetBundleBuilds();
            for (int i = 0; i < BuildList.Length; ++i)
            {
                AssetBundleBuild abb = BuildList[i];
                abb.addressableNames = new string[abb.assetNames.Length];
                for (int j = 0; j < abb.assetNames.Length; ++j)
                    abb.addressableNames[j] = Path.GetFileName(abb.assetNames[j]);
                BuildList[i] = abb;
            }
            var buildContent = new BundleBuildContent(BuildList);

            // step2. Construct the new parameters class
            var buildParams = new CustomBuildParameters(EditorUserBuildSettings.activeBuildTarget, 
                                                        BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), 
                                                        output);
            
            buildParams.BundleCompression = setting.useLZ4Compress ? UnityEngine.BuildCompression.LZ4 : UnityEngine.BuildCompression.LZMA;
            buildParams.ContentBuildFlags = ContentBuildFlags.DisableWriteTypeTree;
            buildParams.ScriptOptions = ScriptCompilationOptions.DevelopmentBuild;
            // Populate the bundle specific compression data
            buildParams.PerBundleCompression.Add("Bundle1", UnityEngine.BuildCompression.Uncompressed);
            buildParams.PerBundleCompression.Add("Bundle2", UnityEngine.BuildCompression.LZMA);
            buildParams.OutputFolder = output;

            IBundleBuildResults results;
            ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParams, buildContent, out results);
            if (exitCode < ReturnCode.Success)
            {
                Debug.LogError($"Failed to build bundles, ReturnCode is {exitCode}");
                return false;
            }

            var manifest = ScriptableObject.CreateInstance<CompatibilityAssetBundleManifest>();
            manifest.SetResults(results.BundleInfos);
            File.WriteAllText(buildParams.GetOutputFilePathForIdentifier(Path.GetFileName(output) + "_Text.manifest"), manifest.ToString());

            BuildManifestAsBundle(output);
            return true;
        }

        static private void BuildManifestAsBundle(string output)
        {
            AssetBundleBuild[] BuildList = new AssetBundleBuild[1];
            AssetBundleBuild abb = new AssetBundleBuild();
            // abb.assetBundleName = Utility.GetPlatformName() + "_XX.manifest";
            abb.assetBundleName = "manifest";
            abb.assetNames = new string[1];
            abb.assetNames[0] = output + "/" + Path.GetFileName(output) + "_Text.manifest";
            // abb.assetNames[0] = "Assets/Windows_1.manifest";
            BuildList[0] = abb;
            // var buildContent = new BundleBuildContent(BuildList);
            // var buildParams = new CustomBuildParameters(EditorUserBuildSettings.activeBuildTarget, 
            //                                             BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), 
            //                                             output);


            // IBundleBuildResults results;
            // ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParams, buildContent, out results);

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(output, BuildList, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
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