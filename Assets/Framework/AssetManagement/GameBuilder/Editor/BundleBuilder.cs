using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using Framework.Core;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Content;
using UnityEditor.Build.Player;
using Framework.AssetManagement.Runtime;
using UnityEditor.Build.Pipeline.Tasks;
using Framework.AssetManagement.AssetEditorWindow;

namespace Framework.AssetManagement.GameBuilder
{
    public class BundleBuilder
    {
        public delegate void onPreprocessBundleBuild();
        public delegate void onPostprocessBundleBuild();

        static public event onPreprocessBundleBuild OnPreprocessBundleBuild;
        static public event onPostprocessBundleBuild OnPostprocessBundleBuild;

        /// <summary>
        /// 构建Bundles接口（唯一）
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>        
        static public bool BuildAssetBundles(BundleBuilderSetting para)
        {
            if (para == null)
            {
                Debug.LogError($"BundleBuilderSetting para == null");
                if (UnityEngine.Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                return false;
            }

            Debug.Log("Begin Build AssetBundles");

            OnPreprocessBundleBuild?.Invoke();

            // create "Assets/StreamingAssets"
            string targetPath = @"Assets/StreamingAssets";
            AssetDatabase.DeleteAsset(targetPath);
            AssetDatabase.CreateFolder("Assets", "StreamingAssets");
            //AssetDatabase.CreateFolder(targetPath, Utility.GetPlatformName());

            // build bundles to streaming assets
            // Debug.Log($"        BuildAssetBundleOptions: {para.GenerateOptions()}");
            if (!BuildBundleWithSBP("buildin", targetPath + "/" + AssetManagerSettings.StreamingAssetsBuildinFolder, para))
            {
                Debug.LogError($"End Build AssetBundles: Failed");
                if (UnityEngine.Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                return false;
            }

            OnPostprocessBundleBuild?.Invoke();

            
            Debug.Log($"        BundleSettings: {para.ToString()}");
            Debug.Log($"End Build AssetBundles: Succeeded");

            AssetDatabase.Refresh();

            return true;
        }

        //private class BuildProcessor : IPreprocessBuildWithReport
        //{
        //    public int callbackOrder { get { return 9999; } }     // 倒数第二步，见PlayerBuilder.BuildProcessor

        //    // 等所有需要打包的资源汇集到了streaming assets再执行
        //    public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        //    {
        //        // 计算base,extra资源的MD5，存储于Assets/Resources
        //        BuildBundleFileList();

        //        // step 1. create directory
        //        GameBuilderSetting setting = GameBuilderSettingCollection.GetDefault().GetData("Win64");

        //        string outputPath = setting.bundleSetting.outputPath + "/" + Utility.GetPlatformName();
        //        if (Directory.Exists(outputPath))
        //            Directory.Delete(outputPath, true);
        //        Directory.CreateDirectory(outputPath);
        //        Debug.Log($"        Bundles Output: {outputPath}");

        //        // 最后把所有StreamingAssets中的资源复制到发布目录（Deployment/Latest/AssetBundles）
        //        // 有些资源例如FMOD有自己的发布流程，等其发布完最后再执行
        //        string srcPath = "Assets/StreamingAssets/" + Utility.GetPlatformName();
        //        Framework.Core.Editor.EditorUtility.CopyDirectory(srcPath, outputPath);
        //        Debug.Log($"        Copy streaming assets to Deployment/Latest/AssetBundles");

        //        // 最后删除其他资源(extra,pkg_XXX)，只有base发布到母包
        //        string[] folders = AssetDatabase.GetSubFolders(srcPath);
        //        foreach(var folder in folders)
        //        {
        //            string dir = folder.Substring(folder.LastIndexOf("/") + 1);
        //            if(string.Compare(dir, VersionDefines.EXTRA_FOLDER, true) == 0 || dir.StartsWith(VersionDefines.PKG_FOLDER_PREFIX, StringComparison.OrdinalIgnoreCase))
        //            {
        //                AssetDatabase.DeleteAsset(folder);
        //            }
        //        }
        //    }
        //}

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

        static private bool BuildBundleWithSBP(string configName, string output, BundleBuilderSetting setting)
        {
            // step1. build map
            BuildMapContext mapContext = BuildMapCreator.CreateBuildMap(configName);

            // step2. build sbp
            var buildParams = new CustomBuildParameters(EditorUserBuildSettings.activeBuildTarget,
                                                        BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget),
                                                        output);

            buildParams.BundleCompression = setting.useLZ4Compress ? UnityEngine.BuildCompression.LZ4 : UnityEngine.BuildCompression.Uncompressed;
            buildParams.UseCache = !setting.rebuildBundles;
            if (setting.DisableWriteTypeTree)
                buildParams.ContentBuildFlags |= ContentBuildFlags.DisableWriteTypeTree;
            if (setting.DevelopmentBuild)
                buildParams.ScriptOptions |= ScriptCompilationOptions.DevelopmentBuild;
            buildParams.OutputFolder = output;

            IBundleBuildResults buildResults;
            ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParams, 
                                                                    new BundleBuildContent(mapContext.GetPipelineBuilds()), 
                                                                    out buildResults, 
                                                                    AssetBundleCompatible(true));
            if (exitCode < ReturnCode.Success)
            {
                Debug.LogError($"Failed to build bundles, ReturnCode is {exitCode}");
                ClearBundleRedundancy(output);
                return false;
            }
            ClearBundleRedundancy(output);

            // step3. update build info
            TaskUpdateBuildInfo.Run(output, mapContext, buildResults);

            // step4. create asset manifest
            SerializeAssetManifest(mapContext);

            CopyBuildinFilesToStreaming();

            return true;
        }

        static private void SerializeAssetManifest(BuildMapContext mapContext)
        {
            AssetManifest manifest = new AssetManifest();

            manifest.SerializedVersion = 1;
            manifest.PackageVersion = "0.0.1";
            manifest.OutputNameStyle = 1;

            foreach (var bundleInfo in mapContext.BuildBundleInfos)
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

            foreach (var bundleInfo in mapContext.BuildBundleInfos)
            {
                foreach(var assetInfo in bundleInfo.BuildinAssets)
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
                    foreach(var dependBundleName in assetInfo.AllDependBundleNames)
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

            AssetManifest.SerializeToBinary($"Assets/StreamingAssets/{AssetManagerSettings.StreamingAssetsBuildinFolder}/AssetManifest.bytes", manifest);
            AssetManifest.SerializeToJson($"Assets/Temp/AssetManifest.json", manifest);
            AssetDatabase.ImportAsset($"Assets/StreamingAssets/{AssetManagerSettings.StreamingAssetsBuildinFolder}/AssetManifest.bytes");
            AssetDatabase.ImportAsset($"Assets/Temp/AssetManifest.json");
        }

        static private void CopyBuildinFilesToStreaming()
        {

        }

        static IList<IBuildTask> AssetBundleCompatible(bool builtinTask)
        {
            var buildTasks = new List<IBuildTask>();

            // Setup
            buildTasks.Add(new SwitchToBuildPlatform());
            buildTasks.Add(new RebuildSpriteAtlasCache());

            // Player Scripts
            buildTasks.Add(new BuildPlayerScripts());
            buildTasks.Add(new PostScriptsCallback());

            // Dependency
            buildTasks.Add(new CalculateSceneDependencyData());
#if UNITY_2019_3_OR_NEWER
            buildTasks.Add(new CalculateCustomDependencyData());
#endif
            buildTasks.Add(new CalculateAssetDependencyData());
            buildTasks.Add(new StripUnusedSpriteSources());
            if (builtinTask)
                buildTasks.Add(new CreateBuiltInResourceBundle("builtin_shaders", "builtin_resources"));
            buildTasks.Add(new PostDependencyCallback());

            // Packing
            buildTasks.Add(new GenerateBundlePacking());
            if (builtinTask)
                buildTasks.Add(new UpdateBundleObjectLayout());
            buildTasks.Add(new GenerateBundleCommands());
            buildTasks.Add(new GenerateSubAssetPathMaps());
            buildTasks.Add(new GenerateBundleMaps());
            buildTasks.Add(new PostPackingCallback());

            // Writing
            buildTasks.Add(new WriteSerializedFiles());
            buildTasks.Add(new ArchiveAndCompressBundles());
            buildTasks.Add(new AppendBundleHash());
            buildTasks.Add(new GenerateLinkXml());
            buildTasks.Add(new PostWritingCallback());

            return buildTasks;
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
        
        [MenuItem("Tools/Check scripts compilation &r")]
        static public void BuildMinorBundle()
        {
            var options = BuildAssetBundleOptions.None;
            string outputPath = "Assets/Temp";
            if(!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            File.Delete(outputPath + "/test_script");
            File.Delete(outputPath + "/test_script.manifest");
            AssetBundleBuild[] abbs = new AssetBundleBuild[1];
            AssetBundleBuild abb = new AssetBundleBuild();
            abb.assetBundleName = "test_script";
            abb.assetNames = new string[1];
            abb.assetNames[0] = "Assets/Res/Core/PoolManager.prefab";
            abbs[0] = abb;
            var manifest = BuildPipeline.BuildAssetBundles(outputPath, abbs, options, EditorUserBuildSettings.activeBuildTarget);
            if(manifest != null)
            {
                Debug.Log($"Success to compile scripts");
            }
            else
            {
                Debug.LogError($"Failed to compile scripts");
            }
        }
    }
}