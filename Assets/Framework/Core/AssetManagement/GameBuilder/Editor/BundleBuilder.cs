using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Globalization;
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
        // static public AssetBundleManifest BuildAssetBundles(BundleBuilderSetting para)
        // {
        //     if (para == null)
        //     {
        //         Debug.LogError($"BundleBuilderSetting para == null");
        //         if (Application.isBatchMode)
        //         {
        //             EditorApplication.Exit(1);
        //         }
        //         return null;
        //     }

        //     Debug.Log("Begin Build AssetBundles");

        //     // step 1. create directory
        //     string outputPath = para.outputPath.TrimEnd(new char[] { '/' }) + "/" + Utility.GetPlatformName();
        //     if (Directory.Exists(outputPath))
        //         Directory.Delete(outputPath, true);
        //     Directory.CreateDirectory(outputPath);
        //     Debug.Log($"        Bundles Output: {outputPath}");

        //     // step 2. build bundles
        //     AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputPath, para.GenerateOptions(), EditorUserBuildSettings.activeBuildTarget);
        //     Debug.Log($"        BuildAssetBundleOptions: {para.GenerateOptions()}");

        //     // step 3. copy bundles to streamingAssets
        //     if (manifest != null)
        //     {
        //         CopyAssetBundlesToStreamingAssets(outputPath);
        //         Debug.Log($"        Copy bundles to streaming assets");
        //     }

        //     Debug.Log($"        BundleSettings: {para.ToString()}");

        //     if(manifest != null)
        //     {
        //         Debug.Log($"End Build AssetBundles: Succeeded");
        //     }
        //     else
        //     {
        //         Debug.LogError($"End Build AssetBundles: Failed");
        //         if (Application.isBatchMode)
        //         {
        //             EditorApplication.Exit(1);
        //         }
        //     }

        //     return manifest;
        // }

        static public bool BuildAssetBundles(BundleBuilderSetting para)
        {
            if (para == null)
            {
                Debug.LogError($"BundleBuilderSetting para == null");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                return false;
            }

            Debug.Log("Begin Build AssetBundles");

            // step 1. create directory
            string outputPath = para.outputPath.TrimEnd(new char[] { '/' }) + "/" + Utility.GetPlatformName();
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);
            Directory.CreateDirectory(outputPath);
            Debug.Log($"        Bundles Output: {outputPath}");

            // step 2. build bundles
            // Debug.Log($"        BuildAssetBundleOptions: {para.GenerateOptions()}");
            if(!BuildBundleWithSBP(outputPath, para))
            {
                Debug.LogError($"End Build AssetBundles: Failed");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                return false;
            }

            // step 3. copy bundles to streamingAssets
            CopyAssetBundlesToStreamingAssets(outputPath);
            Debug.Log($"        Copy bundles to streaming assets");
            Debug.Log($"        BundleSettings: {para.ToString()}");
            Debug.Log($"End Build AssetBundles: Succeeded");

            // step 4. 计算StreamingAssets下所有资源的MD5，存储于Assets/Resources
            BuildBundleFileList();

            return true;
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
            string ResourcePath = string.IsNullOrEmpty(setting.OverrideResourcePath) ? null : setting.OverrideResourcePath.ToLower();
            ResourcePath = string.IsNullOrEmpty(ResourcePath) ? ResourcePath : ResourcePath.TrimEnd(new char[] { '/' }) + "/";

            // 过滤不输出的资源
            AssetBundleBuild[] BuildList = ContentBuildInterface.GenerateAssetBundleBuilds();
            int count = BuildList.Length;
            if(!string.IsNullOrEmpty(ResourcePath))
            {
                count = 0;
                for (int i = 0; i < BuildList.Length; ++i)
                {
                    AssetBundleBuild abb = BuildList[i];
                    if (abb.assetBundleName.StartsWith(ResourcePath, true, CultureInfo.InvariantCulture))
                    {
                        ++count;
                        continue;
                    }
                }
            }

            AssetBundleBuild[] FinalBuildList = new AssetBundleBuild[count];
            int index = 0;
            for (int i = 0; i < BuildList.Length; ++i)
            {
                if(!string.IsNullOrEmpty(ResourcePath) && !BuildList[i].assetBundleName.StartsWith(ResourcePath))
                {
                    continue;
                }

                AssetBundleBuild abb = BuildList[i];
                abb.addressableNames = new string[abb.assetNames.Length];
                for (int j = 0; j < abb.assetNames.Length; ++j)
                    abb.addressableNames[j] = Path.GetFileName(abb.assetNames[j]).ToLower();

                FinalBuildList[index++] = abb;
            }
            var buildContent = new BundleBuildContent(FinalBuildList);

            // step2. Construct the new parameters class
            var buildParams = new CustomBuildParameters(EditorUserBuildSettings.activeBuildTarget, 
                                                        BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), 
                                                        output);
            
            buildParams.BundleCompression = setting.useLZ4Compress ? UnityEngine.BuildCompression.LZ4 : UnityEngine.BuildCompression.LZMA;
            buildParams.UseCache = !setting.rebuildBundles;
            if(setting.DisableWriteTypeTree)
                buildParams.ContentBuildFlags |= ContentBuildFlags.DisableWriteTypeTree;
            if(setting.DevelopmentBuild)
                buildParams.ScriptOptions |= ScriptCompilationOptions.DevelopmentBuild;
            // Populate the bundle specific compression data
            // buildParams.PerBundleCompression.Add("Bundle1", UnityEngine.BuildCompression.Uncompressed);
            // buildParams.PerBundleCompression.Add("Bundle2", UnityEngine.BuildCompression.LZMA);
            buildParams.OutputFolder = output;

            // step3. build bundles except Manifest
            IBundleBuildResults results;
            ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParams, buildContent, out results);
            if (exitCode < ReturnCode.Success)
            {
                Debug.LogError($"Failed to build bundles, ReturnCode is {exitCode}");
                ClearBundleRedundancy(output);
                return false;
            }
            ClearBundleRedundancy(output);

            // step4. build manifest
            string manifestOutput = "Assets/Temp/";          // manifest必须生成在Assets/下才能CreateAsset
            if(!Directory.Exists(manifestOutput))
            {
                Directory.CreateDirectory(manifestOutput);
            }
            var manifest = ScriptableObject.CreateInstance<CompatibilityAssetBundleManifest>();
            manifest.SetResults(results.BundleInfos);
            AssetDatabase.CreateAsset(manifest, manifestOutput + Utility.GetPlatformName() + "_manifest.asset");
            AssetDatabase.Refresh();
            if(!BuildManifestAsBundle(manifestOutput))
            {
                Debug.LogError("Failed to build manifest");
                // ClearManifestRedundancy(manifestOutput);
                return false;
            }

            // step5. copy manifest and clear temp files
            CopyManifestToOutput(manifestOutput, output);
            // ClearManifestRedundancy(manifestOutput);

            // 暂时省略此步
            // copy Assets/Temp/windows_manifest.asset to Assets/Resources/windows/windows_manifest.asset
            // string dstPath = "Assets/Resources/" + Utility.GetPlatformName();
            // if(!Directory.Exists(dstPath))
            //     Directory.CreateDirectory(dstPath);
            // File.Copy(manifestOutput + Utility.GetPlatformName() + "_manifest.asset", dstPath + "/" + Utility.GetPlatformName() + "_manifest.asset", true);
            
            return true;
        }

        static private bool BuildManifestAsBundle(string output)
        {
            AssetBundleBuild[] BuildList = new AssetBundleBuild[1];
            AssetBundleBuild abb = new AssetBundleBuild();
            abb.assetBundleName = "manifest";
            abb.assetNames = new string[1];
            abb.assetNames[0] = output + "/" + Utility.GetPlatformName() + "_manifest.asset";
            abb.addressableNames = new string[1];
            abb.addressableNames[0] = "manifest";
            BuildList[0] = abb;
            var buildContent = new BundleBuildContent(BuildList);
            var buildParams = new CustomBuildParameters(EditorUserBuildSettings.activeBuildTarget, 
                                                        BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), 
                                                        output);

            IBundleBuildResults results;
            ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParams, buildContent, out results);
            return exitCode >= ReturnCode.Success;
        }

        static private void CopyManifestToOutput(string srcPath, string dstPath)
        {
            srcPath = srcPath.TrimEnd(new char[] { '/' }) + "/";
            dstPath = dstPath.TrimEnd(new char[] { '/' }) + "/";
            File.Copy(srcPath + "manifest", dstPath + "manifest", true);
        }

        static private void ClearManifestRedundancy(string directory)
        {
            directory = directory.TrimEnd(new char[] { '/' }) + "/";
            AssetDatabase.DeleteAsset(directory + Utility.GetPlatformName() + "_Manifest.asset");
            AssetDatabase.DeleteAsset(directory + "manifest");
            AssetDatabase.DeleteAsset(directory + "buildlogtep.json");
        }

        static private void ClearBundleRedundancy(string directory)
        {
            directory = directory.TrimEnd(new char[] { '/' }) + "/";
            File.Delete(directory + "buildlogtep.json");
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
            Directory.CreateDirectory(targetPath);

            string finalPath = targetPath + "/" + Utility.GetPlatformName();
            Directory.CreateDirectory(finalPath);

            // 把源目录文件复制到目标目录
            if (Directory.Exists(srcPath))
            {
                Framework.Core.Editor.EditorUtility.CopyDirectory(srcPath, finalPath);
            }
            else
            {
                Debug.LogWarning("Source path does not exist!");
            }

            AssetDatabase.Refresh();
        }
        
        static private void BuildBundleFileList()
        {
            BundleFileList.BuildBundleFileList(string.Format($"{Application.streamingAssetsPath}/{Utility.GetPlatformName()}"), 
                                               string.Format($"{BundleExtracter.FILELIST_PATH}/{Utility.GetPlatformName()}/{BundleExtracter.FILELIST_NAME}"));

            AssetDatabase.Refresh();
        }
    }
}