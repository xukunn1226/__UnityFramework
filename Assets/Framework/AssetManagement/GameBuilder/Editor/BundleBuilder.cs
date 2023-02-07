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
using Framework.AssetManagement.AssetPackageEditor.Editor;
using UnityEditor.Build.Pipeline.Tasks;
using Framework.AssetManagement.AssetEditorWindow;
using System.Security.Cryptography;

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

        static private Dictionary<string, string> s_BPInfo = new Dictionary<string, string>();       // KEY: bundle name; VALUE: package name
        static private AssetBundleBuild[] s_abb;
        static private IBundleBuildResults s_buildResults;

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
            //AssetDatabase.CreateFolder(targetPath, Utility.GetPlatformName());

            // build bundles to streaming assets
            // Debug.Log($"        BuildAssetBundleOptions: {para.GenerateOptions()}");
            if (!BuildBundleWithSBPEx("buildin", targetPath + "/" + AssetManagerSettings.StreamingAssetsBuildinFolder, para))
                //if (!BuildBundleWithSBP(targetPath + "/" + Utility.GetPlatformName(), para))
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

            // build URP bundle build list
            AssetBundleBuild[] BuildList_URP = GenerateURPBuiltinBundleBuild();

            List<AssetBundleBuild> result = new List<AssetBundleBuild>();
            result.AddRange(BuildList);
            result.AddRange(BuildList_URP);
            return result.ToArray();
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
                AssetPackageSettingItem pkgItem = m_Setting.packageEditorSetting.GetBuildBundleType(assetPath);
                if (pkgItem.buildBundleType == AssetPackageBuildBundleType.ByFile)
                {
                    bundleName = pkgItem.path.TrimEnd(new char[] { '/' }) + "/" + Path.GetFileNameWithoutExtension(assetPath);
                }
                else
                {
                    bundleName = pkgItem.path.TrimEnd(new char[] { '/' });
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
            s_abb = GenerateAssetBundleBuildsStrategically();
            var buildContent = new BundleBuildContent(s_abb);

            // step1. Construct the new parameters class
            var buildParams = new CustomBuildParameters(EditorUserBuildSettings.activeBuildTarget, 
                                                        BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), 
                                                        output);
            
            buildParams.BundleCompression = setting.useLZ4Compress ? UnityEngine.BuildCompression.LZ4 : UnityEngine.BuildCompression.Uncompressed;
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
            var taskList = AssetBundleCompatible(true);
            ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParams, buildContent, out s_buildResults, taskList);
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

            // if (!BuildManifestAsBundle(s_abb, s_buildResults, manifestOutput, s_useHashToBundleName))
            // {
            //     Debug.LogError("Failed to build manifest");
            //     return false;
            // }

            AssetDatabase.Refresh();        // 改名处理前需要refresh，生成meta，才能执行rename asset

            // 把所有已输出的bundle改名
            if (s_useHashToBundleName)
            {
                AssetBuilderSetting assetSetting = AssetBuilderSetting.GetDefault();
                s_BPInfo.Clear();
                foreach (var item in s_abb)
                {
                    BundleDetails bundleDetails;
                    s_buildResults.BundleInfos.TryGetValue(item.assetBundleName, out bundleDetails);

                    string oldPathName = output + "/" + item.assetBundleName;
                    string newBundleName = bundleDetails.Hash.ToString();
                    string error = AssetDatabase.RenameAsset(oldPathName, newBundleName);
                    if(!string.IsNullOrEmpty(error))
                        Debug.LogError($"RenameAsset:  {error}");

                    // update s_BPInfo
                    if(!s_BPInfo.ContainsKey(newBundleName))
                    {
                        s_BPInfo.Add(newBundleName, m_Setting.packageEditorSetting.GetPackageID(item.assetNames[0]));
                    }
                }

                // s_abb没有内置资源的打包信息，故这里额外记录
                BundleDetails builtinBundleDetails;
                s_buildResults.BundleInfos.TryGetValue("builtin_shaders", out builtinBundleDetails);
                string oldPathName2 = output + "/" + "builtin_shaders";
                string newBundleName2 = builtinBundleDetails.Hash.ToString();
                string err = AssetDatabase.RenameAsset(oldPathName2, newBundleName2);
                if (!string.IsNullOrEmpty(err))
                    Debug.LogError($"RenameAsset:  {err}");
                if(!s_BPInfo.ContainsKey(newBundleName2))
                {
                    s_BPInfo.Add(newBundleName2, "base");   // 内置资源放入base
                }

                s_buildResults.BundleInfos.TryGetValue("builtin_resources", out builtinBundleDetails);
                oldPathName2 = output + "/" + "builtin_resources";
                newBundleName2 = builtinBundleDetails.Hash.ToString();
                err = AssetDatabase.RenameAsset(oldPathName2, newBundleName2);
                if (!string.IsNullOrEmpty(err))
                    Debug.LogError($"RenameAsset:  {err}");
                if (!s_BPInfo.ContainsKey(newBundleName2))
                {
                    s_BPInfo.Add(newBundleName2, "base");   // 内置资源放入base
                }
            }

            // 分包处理
            SplitBundles(output, m_Setting);

            // rebuild manifest
            if (!RebuildManifestAsBundle(s_abb, s_buildResults, s_BPInfo, manifestOutput, s_useHashToBundleName))
            {
                Debug.LogError("Failed to rebuild manifest");
            }

            // step5. copy manifest and clear temp files
            CopyManifestToOutput(manifestOutput, output);

            return true;
        }

        static private bool BuildBundleWithSBPEx(string configName, string output, BundleBuilderSetting setting)
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
            SerializeAssetManifestEx(mapContext);

            CopyBuildinFilesToStreaming();


            // step4. build manifest
            //string manifestOutput = "Assets/Temp/";          // manifest必须生成在Assets/下才能CreateAsset
            //if (!Directory.Exists(manifestOutput))
            //{
            //    Directory.CreateDirectory(manifestOutput);
            //}

            // rebuild manifest
            //if (!RebuildManifestAsBundle(s_abb, s_buildResults, s_BPInfo, manifestOutput, s_useHashToBundleName))
            //{
            //    Debug.LogError("Failed to rebuild manifest");
            //}

            // step5. copy manifest and clear temp files
            //CopyManifestToOutput(manifestOutput, output);

            return true;
        }

        static private void SerializeAssetManifestEx(BuildMapContext mapContext)
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

        // 把streaming assets下的资源进行分包处理（base、extra、pkg。。。）
        static private void SplitBundles(string srcDirectory, BundleBuilderSetting setting)
        {
            foreach(var item in s_BPInfo)
            {
                string bundleName = item.Key;
                string pkgName = item.Value;

                string oldPath = srcDirectory.TrimEnd('/') + "/" + bundleName;
                string newPath = srcDirectory.TrimEnd('/') + "/" + pkgName;
                if(!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(srcDirectory, pkgName);
                }
                string err = AssetDatabase.MoveAsset(oldPath, newPath + "/" + bundleName);
                if(!string.IsNullOrEmpty(err))
                {
                    Debug.LogError(err);
                }
            }
            AssetDatabase.Refresh();
        }

        static private bool RebuildManifestAsBundle(AssetBundleBuild[] abb, IBundleBuildResults results, Dictionary<string, string> BPInfo, string manifestOutput, bool useHashToBundleName)
        {
            string manifestFilepath = RecreateManifestAsset(abb, results, BPInfo, manifestOutput, useHashToBundleName);

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

        static private string RecreateManifestAsset(AssetBundleBuild[] abb, IBundleBuildResults results, Dictionary<string, string> BPInfo, string manifestOutput, bool useHashToBundleName)
        {
            // 方法二：写入自定义json格式文件
            CustomManifest manifest = new CustomManifest();
            foreach (var item in results.BundleInfos)
            {
                manifest.m_BundleDetails.Add(useHashToBundleName ? item.Value.Hash.ToString() : item.Key,
                                             new CustomManifest.BundleDetail()
                                             {
                                                 bundleName = BPInfo[item.Value.Hash.ToString()] + "/" + (useHashToBundleName ? item.Value.Hash.ToString() : item.Key),
                                                 bundlePath = item.Key,
                                                 isUnityBundle = true,
                                                 isStreamingAsset = true,
                                                 dependencies = useHashToBundleName ? ConvertDependenciesToHash(item.Value.Dependencies, results) : item.Value.Dependencies
                                             });
            }
            foreach (var bb in abb)
            {
                for (int i = 0; i < bb.assetNames.Length; ++i)
                {
                    BundleDetails bundleDetails;
                    results.BundleInfos.TryGetValue(bb.assetBundleName, out bundleDetails);

                    manifest.m_FileDetails.Add(bb.assetNames[i],
                                               new CustomManifest.FileDetail()
                                               {
                                                   bundleHash = useHashToBundleName ? bundleDetails.Hash.ToString() : bb.assetBundleName,
                                                   fileName = bb.addressableNames[i]
                                               });
                }
            }

            string customManifestFilepath = manifestOutput + Utility.GetPlatformName() + "_manifest.zjson";
            CustomManifest.Serialize(customManifestFilepath, manifest);
            AssetDatabase.ImportAsset(customManifestFilepath);

            // 临时方案，把CustomManifest translate to AssetManifest
            SerializeAssetManifest(manifest);

            return customManifestFilepath;
        }

        static private void SerializeAssetManifest(CustomManifest cusManifest)
        {
            AssetManifest manifest = new AssetManifest();

            manifest.SerializedVersion = 1;
            manifest.PackageVersion = "0.0.1";
            manifest.OutputNameStyle = 1;

            foreach(var valuePair in cusManifest.m_BundleDetails)
            {
                BundleDescriptor desc = new BundleDescriptor();
                desc.bundleName = valuePair.Value.bundleName;
                desc.fileHash = "";
                desc.fileCRC = "";
                desc.fileSize = 1;
                desc.isRawFile = false;
                desc.loadMethod = 0;
                manifest.BundleList.Add(desc);
            }

            foreach(var valuePair in cusManifest.m_FileDetails)
            {
                var desc = new AssetDescriptor();
                desc.assetPath = valuePair.Key;

                // main bundle
                if (!cusManifest.m_BundleDetails.TryGetValue(valuePair.Value.bundleHash, out var bundleDetail))
                    throw new Exception($"can't find bundle hash {valuePair.Value.bundleHash} from m_BundleDetails");

                int index = manifest.BundleList.FindIndex(item => item.bundleName == bundleDetail.bundleName);
                if (index == -1)
                    throw new Exception($"can't find bundle {bundleDetail.bundleName} from BundleList");

                desc.bundleID = index;
                List<int> dependIDs = new List<int>();
                foreach(var depend in bundleDetail.dependencies)
                {
                    if (!cusManifest.m_BundleDetails.TryGetValue(depend, out var bd))
                        throw new Exception($"can't find bundle hash {depend} from m_BundleDetails");

                    index = manifest.BundleList.FindIndex(item => item.bundleName == bd.bundleName);
                    if(index == -1)
                        throw new Exception($"can't find bundle {bd.bundleName} from BundleList");

                    dependIDs.Add(index);
                }
                desc.dependIDs = dependIDs.ToArray();

                manifest.AssetList.Add(desc);
            }

            AssetManifest.SerializeToBinary($"Assets/StreamingAssets/{Utility.GetPlatformName()}/AssetManifest.bytes", manifest);
            AssetManifest.SerializeToJson($"Assets/Temp/AssetManifest.json", manifest);
            AssetDatabase.ImportAsset($"Assets/StreamingAssets/{Utility.GetPlatformName()}/AssetManifest.bytes");
            AssetDatabase.ImportAsset($"Assets/Temp/AssetManifest.json");
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
                                                   bundleHash = useHashToBundleName ? bundleDetails.Hash.ToString() : bb.assetBundleName,
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
            string savedFile = string.Format($"{VersionDefines.BASE_FILELIST_PATH}/{Utility.GetPlatformName()}/{VersionDefines.BASE_FILELIST_NAME}");
            AssetDatabase.DeleteAsset(savedFile);
            if(BundleFileList.BuildBaseAndRawDataBundleFileList(directory, VersionDefines.BASE_FOLDER, savedFile))
                AssetDatabase.ImportAsset(savedFile);

            // 生成二次下载包FileList到Assets/Resources
            directory = string.Format($"{UnityEngine.Application.streamingAssetsPath}/{Utility.GetPlatformName()}");
            savedFile = string.Format($"{VersionDefines.EXTRA_FILELIST_PATH}/{Utility.GetPlatformName()}/{VersionDefines.EXTRA_FILELIST_NAME}");
            AssetDatabase.DeleteAsset(savedFile);
            if(BundleFileList.BuildBundleFileList(directory, VersionDefines.EXTRA_FOLDER, savedFile))
                AssetDatabase.ImportAsset(savedFile);
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

        static private AssetBundleBuild[] GenerateURPBuiltinBundleBuild()
        {
            AssetBundleBuild[] abbs = new AssetBundleBuild[3];

            // build urp resources
            abbs[0] = GenerateBundleBuild("assets_urp_builtin_materials", "t:Material", new string[] { "Packages/com.unity.render-pipelines.universal/Runtime/Materials" });
            abbs[1] = GenerateBundleBuild("assets_urp_builtin_textures", "t:Texture", new string[] { "Packages/com.unity.render-pipelines.universal/Textures" });
            abbs[2] = GenerateBundleBuild("assets_urp_builtin_shaders", "t:Shader", new string[] { "Packages/com.unity.render-pipelines.universal/Shaders" });

            return abbs;
        }

        static private AssetBundleBuild GenerateBundleBuild(string bundleName, string filter, string[] searchInFolders)
        {
            AssetBundleBuild abb = new AssetBundleBuild();
            abb.assetBundleName = bundleName;
            string[] guids = AssetDatabase.FindAssets(filter, searchInFolders);
            abb.assetNames = new string[guids.Length];
            abb.addressableNames = new string[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                abb.assetNames[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
                abb.addressableNames[i] = Path.GetFileName(abb.assetNames[i]);
            }
            return abb;
        }
    }
}