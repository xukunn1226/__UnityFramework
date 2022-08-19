using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Globalization;
using Framework.Core;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build;
using UnityEditor.Build.Content;
using UnityEditor.Build.Player;
using UnityEngine.Build.Pipeline;
using Framework.AssetManagement.AssetBuilder;
using Framework.AssetManagement.Runtime;

namespace Framework.AssetManagement.GameBuilder
{
    public class BundleBuilder
    {
        public delegate void onPreprocessBundleBuild();
        public delegate void onPostprocessBundleBuild();

        static public event onPreprocessBundleBuild OnPreprocessBundleBuild;
        static public event onPostprocessBundleBuild OnPostprocessBundleBuild;

        static private bool s_useHashToBundleName = true;
        static private BundleBuilderSetting m_Setting;

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

            m_Setting = para;

            OnPreprocessBundleBuild?.Invoke();

            // create "Assets/StreamingAssets"
            string targetPath = @"Assets/StreamingAssets";
            AssetDatabase.DeleteAsset(targetPath);
            AssetDatabase.CreateFolder("Assets", "StreamingAssets");
            AssetDatabase.CreateFolder(targetPath, Utility.GetPlatformName());

            // build bundles to streaming assets
            // Debug.Log($"        BuildAssetBundleOptions: {para.GenerateOptions()}");
            if(!BuildBundleWithSBP(targetPath + "/" + Utility.GetPlatformName(), para))
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

        private class BuildProcessor : IPreprocessBuildWithReport
        {
            public int callbackOrder { get { return 9999; } }     // 倒数第二步，见PlayerBuilder.BuildProcessor

            // 等所有需要打包的资源汇集到了streaming assets再执行
            public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
            {
                // 所有资源(bundle & raw data)汇集到streaming assets后再进行分包处理
                SplitBundles("Assets/StreamingAssets" + "/" + Utility.GetPlatformName(), m_Setting);

                // recreate manifest

                // 计算StreamingAssets下所有资源的MD5，存储于Assets/Resources
                BuildBundleFileList();

                // step 1. create directory
                GameBuilderSetting setting = GameBuilderSettingCollection.GetDefault().GetData("Win64");

                string outputPath = setting.bundleSetting.outputPath + "/" + Utility.GetPlatformName();
                if (Directory.Exists(outputPath))
                    Directory.Delete(outputPath, true);
                Directory.CreateDirectory(outputPath);
                Debug.Log($"        Bundles Output: {outputPath}");

                // 最后把所有StreamingAssets中的资源复制到发布目录（Deployment/Latest/AssetBundles）
                // 有些资源例如FMOD有自己的发布流程，等其发布完最后再执行
                Framework.Core.Editor.EditorUtility.CopyDirectory("Assets/StreamingAssets/" + Utility.GetPlatformName(), outputPath);
                Debug.Log($"        Copy streaming assets to Deployment/Latest/AssetBundles");
            }
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

        [Obsolete("use GenerateAssetBundleBuildsStrategically replace GenerateAssetBundleBuilds")]
        static private AssetBundleBuild[] GenerateAssetBundleBuilds()
        {
            // 收集所有需要打包的资源
            Dictionary<string, List<string>> BundleFileList = new Dictionary<string, List<string>>();       // assetBundleName : List<assetPath>
            string[] guids = AssetDatabase.FindAssets("*", AssetBuilderSetting.GetDefault().WhiteListOfPath);
            foreach(var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid).ToLower();
                string bundleName = AssetBuilderUtil.GetAssetBundleName(assetPath);
                if(string.IsNullOrEmpty(bundleName))
                    continue;

                bundleName = bundleName.Replace("/", "_");

                List<string> list;
                if(!BundleFileList.TryGetValue(bundleName, out list))
                {
                    list = new List<string>();
                    BundleFileList.Add(bundleName, list);
                }
                list.Add(assetPath);
            }

            // generate AssetBundleBuild
            AssetBundleBuild[] BuildList = new AssetBundleBuild[BundleFileList.Count];
            int index = 0;
            foreach(var bundleFile in BundleFileList)
            {
                AssetBundleBuild abb = new AssetBundleBuild();
                abb.assetBundleName = bundleFile.Key;

                abb.assetNames = new string[bundleFile.Value.Count];
                abb.addressableNames = new string[bundleFile.Value.Count];
                for(int i = 0; i < bundleFile.Value.Count; ++i)
                {
                    abb.assetNames[i] = bundleFile.Value[i];
                    abb.addressableNames[i] = Path.GetFileName(abb.assetNames[i]);
                }

                BuildList[index++] = abb;
            }
            return BuildList;
        }

        // 优化打包策略（by file, by size, by top folder, by folder）
        static private AssetBundleBuild[] GenerateAssetBundleBuildsStrategically()
        {
            // 收集所有需要打包的资源
            Dictionary<string, List<string>> BundleFileList = GenerateBundleFileList();     // assetBundleName : List<assetPath>

            // generate AssetBundleBuild
            AssetBundleBuild[] BuildList = new AssetBundleBuild[BundleFileList.Count];
            int index = 0;
            foreach (var bundleFile in BundleFileList)
            {
                AssetBundleBuild abb = new AssetBundleBuild();
                abb.assetBundleName = bundleFile.Key;
                abb.assetNames = new string[bundleFile.Value.Count];
                abb.addressableNames = new string[bundleFile.Value.Count];
                for (int i = 0; i < bundleFile.Value.Count; ++i)
                {
                    abb.assetNames[i] = bundleFile.Value[i];
                    abb.addressableNames[i] = Path.GetFileName(abb.assetNames[i]);
                }

                BuildList[index++] = abb;
            }
            return BuildList;
        }

        // 根据资源路径、打包策略生成bundle file list
        static private Dictionary<string, List<string>> GenerateBundleFileList()
        {
            // 收集所有需要打包的资源            
            Dictionary<string, List<string>> BundleFileList = new Dictionary<string, List<string>>();       // assetBundleName : List<assetPath>
            AssetBuilderSetting setting = AssetBuilderSetting.GetDefault();
            string[] guids = AssetDatabase.FindAssets("*", setting.WhiteListOfPath);
            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid).ToLower();
                if (AssetDatabase.IsValidFolder(assetPath))
                    continue;
                if (AssetBuilderUtil.IsBlockedByBlackList(assetPath))
                    continue;
                if (AssetBuilderUtil.IsBlockedByExtension(assetPath))
                    continue;

                // 根据路径判断打包策略
                string bundleName = null;
                string packPath = null;
                AssetBuilderSetting.PackType packType = setting.GetPackType(assetPath, out packPath);
                if (packType == AssetBuilderSetting.PackType.Pack_ByFolder)
                {
                    bundleName = packPath.TrimEnd(new char[] { '/' });
                }
                else if (packType == AssetBuilderSetting.PackType.Pack_ByFile)
                {
                    bundleName = packPath.TrimEnd(new char[] { '/' }) + "/" + Path.GetFileNameWithoutExtension(assetPath);
                }
                else if (packType == AssetBuilderSetting.PackType.Pack_BySize)
                {
                    bundleName = packPath.TrimEnd(new char[] { '/' });
                }
                else if (packType == AssetBuilderSetting.PackType.Pack_ByAllFolder)
                {
                    bundleName = packPath.TrimEnd(new char[] { '/' });
                }

                if (string.IsNullOrEmpty(bundleName))
                    continue;

                bundleName = bundleName.TrimEnd(new char[] { '/' }).Replace("/", "_").ToLower();

                List<string> list;
                if (!BundleFileList.TryGetValue(bundleName, out list))
                {
                    list = new List<string>();
                    BundleFileList.Add(bundleName, list);
                }
                list.Add(assetPath);
            }
            return BundleFileList;
        }

        static private bool BuildBundleWithSBP(string output, BundleBuilderSetting setting)
        {
            AssetBundleBuild[] abb = GenerateAssetBundleBuildsStrategically();
            var buildContent = new BundleBuildContent(abb);

            // step1. Construct the new parameters class
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

            // step2. build bundles except Manifest
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
            if (!Directory.Exists(manifestOutput))
            {
                Directory.CreateDirectory(manifestOutput);
            }

            if (!BuildManifestAsBundle(abb, results, manifestOutput, s_useHashToBundleName))
            {
                Debug.LogError("Failed to build manifest");
                // ClearManifestRedundancy(manifestOutput);
                return false;
            }
            
            AssetDatabase.Refresh();        // 改名处理前需要refresh

            // 把所有已输出的bundle改名
            if (s_useHashToBundleName)
            {                
                foreach (var item in abb)
                {
                    BundleDetails bundleDetails;
                    results.BundleInfos.TryGetValue(item.assetBundleName, out bundleDetails);

                    string oldPathName = output + "/" + item.assetBundleName;
                    string error = AssetDatabase.RenameAsset(oldPathName, bundleDetails.Hash.ToString());
                    if(!string.IsNullOrEmpty(error))
                        Debug.LogError($"RenameAsset:  {error}");
                }
            }

            // step5. copy manifest and clear temp files
            CopyManifestToOutput(manifestOutput, output);
            return true;
        }

        // 把streaming assets下的资源进行分包处理（base、extra、pkg。。。）
        static private void SplitBundles(string srcDirectory, BundleBuilderSetting setting)
        {

        }

        static private string CreateManifestAsset(AssetBundleBuild[] abb, IBundleBuildResults results, string manifestOutput, bool useHashToBundleName)
        {
            // 方法一：[obsolete]输出仅为debug
            var unityManifest = ScriptableObject.CreateInstance<CompatibilityAssetBundleManifest>();
            unityManifest.SetResults(results.BundleInfos);
            string unityManifestFilepath = manifestOutput + Utility.GetPlatformName() + "_manifest.asset";
            AssetDatabase.CreateAsset(unityManifest, unityManifestFilepath);

            // 方法二：写入自定义json格式文件
            CustomManifest manifest = new CustomManifest();
            foreach(var item in results.BundleInfos)
            {
                manifest.m_BundleDetails.Add(useHashToBundleName ? item.Value.Hash.ToString() : item.Key,
                                             new CustomManifest.BundleDetail()
                                             {
                                                 bundleName = useHashToBundleName ? item.Value.Hash.ToString() : item.Key,
                                                 bundlePath = item.Key,
                                                 isUnityBundle = true,
                                                 isStreamingAsset = true,
                                                 dependencies = useHashToBundleName ? ConvertDependenciesToHash(item.Value.Dependencies, results) : item.Value.Dependencies
                                             });
            }
            foreach(var bb in abb)
            {
                for(int i = 0; i < bb.assetNames.Length; ++i)
                {
                    BundleDetails bundleDetails;
                    results.BundleInfos.TryGetValue(bb.assetBundleName, out bundleDetails);

                    manifest.m_FileDetails.Add(bb.assetNames[i],
                                               new CustomManifest.FileDetail()
                                               {
                                                   bundleName = useHashToBundleName ? bundleDetails.Hash.ToString() : bb.assetBundleName,
                                                   fileName = bb.addressableNames[i]
                                               });
                }
            }

            string customManifestFilepath = manifestOutput + Utility.GetPlatformName() + "_manifest.zjson";
            CustomManifest.Serialize(customManifestFilepath, manifest);
            AssetDatabase.ImportAsset(customManifestFilepath);

            return customManifestFilepath;
        }

        static private string[] ConvertDependenciesToHash(string[] dependencies, IBundleBuildResults results)
        {
            for(int i = 0; i < dependencies.Length; ++i)
            {
                BundleDetails bundleDetails;
                results.BundleInfos.TryGetValue(dependencies[i], out bundleDetails);
                dependencies[i] = bundleDetails.Hash.ToString();
            }
            return dependencies;
        }

        static private bool BuildManifestAsBundle(AssetBundleBuild[] abb, IBundleBuildResults results, string manifestOutput, bool useHashToBundleName)
        {
            string manifestFilepath = CreateManifestAsset(abb, results, manifestOutput, useHashToBundleName);

            AssetBundleBuild[] BuildList = new AssetBundleBuild[1];
            AssetBundleBuild manifestAbb = new AssetBundleBuild();
            manifestAbb.assetBundleName = "manifest";
            manifestAbb.assetNames = new string[1];
            manifestAbb.assetNames[0] = manifestFilepath;
            manifestAbb.addressableNames = new string[1];
            manifestAbb.addressableNames[0] = "manifest";
            BuildList[0] = manifestAbb;
            var buildContent = new BundleBuildContent(BuildList);
            var buildParams = new CustomBuildParameters(EditorUserBuildSettings.activeBuildTarget, 
                                                        BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), 
                                                        manifestOutput);

            IBundleBuildResults manifestResults;
            ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParams, buildContent, out manifestResults);
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
            // 生成首包FileList到Assets/Resources
            string directory = string.Format($"{UnityEngine.Application.streamingAssetsPath}/{Utility.GetPlatformName()}");
            string savedFile = string.Format($"{BundleExtracter.BASE_FILELIST_PATH}/{Utility.GetPlatformName()}/{BundleExtracter.BASE_FILELIST_NAME}");
            BundleFileList.BuildBundleFileList(directory, savedFile);

            AssetDatabase.ImportAsset(savedFile);
        }
    }
}