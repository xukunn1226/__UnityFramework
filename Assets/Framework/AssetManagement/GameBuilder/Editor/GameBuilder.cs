using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.Core;
using System.IO;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace Framework.AssetManagement.AssetEditorWindow
{
    public class GameBuilder
    {
        static private readonly BuildContext m_BuildContext = new BuildContext();

        /// <summary>
        /// 构建app接口
        /// 允许仅构建bundles或player
        /// </summary>
        /// <param name="bundleSetting"></param>
        /// <param name="playerSetting"></param>
        static public void BuildGame(GameBuilderSetting gameSetting)
        {
            var buildResult = Run(gameSetting);
            if (buildResult.Success)
            {
                EditorUtility.RevealInFinder(buildResult.OutputPackageDirectory);
            }
        }
        
        static private GameBuildResult Run(GameBuilderSetting buildParameters)
        {
            if (buildParameters == null)
                throw new System.Exception($"{nameof(GameBuilderSetting)} is null");
            if (buildParameters.bundleSetting == null)
                throw new System.Exception($"{nameof(BundleBuilderSetting)} is null");
            if (buildParameters.playerSetting == null)
                throw new System.Exception($"{nameof(PlayerBuilderSetting)} is null");

            m_BuildContext.ClearAllContext();

            // 传递构建参数
            var buildParametersContext = new BuildParametersContext(buildParameters);
            m_BuildContext.SetContextObject(buildParametersContext);

            // 创建构建任务
            List<IGameBuildTask> tasks = new List<IGameBuildTask>
            {                
                new TaskPrepare(),
            };
            switch(buildParameters.buildMode)
            {
                case GameBuilderSetting.BuildMode.Bundles:
                    SetBuildBundleTasks(tasks);
                    break;
                case GameBuilderSetting.BuildMode.Player:
                    SetBuildPlayerTasks(tasks);
                    break;
                case GameBuilderSetting.BuildMode.BundlesAndPlayer:
                    SetBuildBundleTasks(tasks);
                    SetBuildPlayerTasks(tasks);
                    break;
                default:
                    throw new System.Exception($"should never get here");
            }

            // 执行构建任务
            var buildResult = BuildRunner.Run(tasks, m_BuildContext);
            if (buildResult.Success)
            {
                buildResult.OutputPackageDirectory = AssetBundleBuilderHelper.GetDefaultOutputRoot();
                Debug.Log($"{buildParameters.buildMode} pipeline build succeed !");
            }
            else
            {
                Debug.LogWarning($"{buildParameters.buildMode} pipeline build failed !");
                Debug.LogError($"Build task failed : {buildResult.FailedTask}");
                Debug.LogError($"Build task error : {buildResult.FailedInfo}");
            }
            return buildResult;
        }

        /// <summary>
        /// 设置构建资源包的任务列表
        /// </summary>
        /// <param name="tasks"></param>
        static private void SetBuildBundleTasks(List<IGameBuildTask> tasks)
        {
            List<IGameBuildTask> bundleTasks = new List<IGameBuildTask>
            {
                new TaskPreprocessBuildBundles(),   // 执行自定义的构建任务
                new TaskBuildBundleMap(),           // 准备构建的内容，分析资源的依赖关系
                new TaskBuildAssetBundles(),        // 执行构建
                new TaskVerifyBuildResult(),        // 验证打包结果
                new TaskUpdateBuildInfo(),          // 根据打包结果更新数据
                new TaskCreateManifest(),           // 创建清单
                new TaskCreateReport(),             // 创建构建报告
                new TaskCopyToStreamingAssets(),    // 拷贝文件到Streaming
            };
            tasks.AddRange(bundleTasks);
        }

        /// <summary>
        /// 设置构建Player的任务列表
        /// </summary>
        /// <param name="tasks"></param>
        static private void SetBuildPlayerTasks(List<IGameBuildTask> tasks)
        {
            List<IGameBuildTask> playerTasks = new List<IGameBuildTask>
            {
                new TaskPreprocessBuildPlayer(),    // 执行自定义的构建任务
                new TaskSetupPlayerSetting(),       // 设置PlayerSetting
                new TaskBuildPlayer(),              // 构建Player
                new TaskRestorePlayerSetting(),     // 还原PlayerSetting
            };
            tasks.AddRange(playerTasks);
        }

        static public void cmdBuildGame()
        {
            if (!UnityEngine.Application.isBatchMode)
            {
                return;
            }

            // get the named GameBuilder
            string whichSetting = "win64";
            if(!CommandLineReader.GetCommand("BuilderProfile", ref whichSetting))
            {
                Debug.Log(@"     Error: Missing Command: BuilderProfile ");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("     BuilderProfile: " + whichSetting);
            GameBuilderSetting setting = GameBuilderSettingCollection.GetDefault().GetData(whichSetting);
            if(setting == null)
            {
                Debug.Log($"     Error: Can't find the GameBuilderSetting {whichSetting}");
                EditorApplication.Exit(1);
                return;
            }

            // override the game setting parameters
            SetOverridePara(ref setting.buildMode,                                  "BuildMode",                GameBuilderSetting.BuildMode.BundlesAndPlayer);

            // override the bundle setting parameters
            SetOverridePara(ref setting.bundleSetting.useLZ4Compress,               "UseLZ4Compress");
            SetOverridePara(ref setting.bundleSetting.appendHash,                   "AppendHash");
            SetOverridePara(ref setting.bundleSetting.rebuildBundles,               "RebuildBundles");

            // override the player setting parameters
            SetOverridePara(ref setting.playerSetting.projectName,                  "ProjectName");            
            SetOverridePara(ref setting.playerSetting.development,                  "Development");
            SetOverridePara(ref setting.playerSetting.compressWithLz4,              "CompressWithLz4");
            SetOverridePara(ref setting.playerSetting.compressWithLz4HC,            "CompressWithLz4HC");
            SetOverridePara(ref setting.playerSetting.strictMode,                   "PlayerStrictMode");
            SetOverridePara(ref setting.playerSetting.useIL2CPP,                    "useIL2CPP");
            SetOverridePara(ref setting.playerSetting.useMTRendering,               "UseMTRendering");
            SetOverridePara(ref setting.playerSetting.buildAppBundle,               "BuildAppBundle");
            SetOverridePara(ref setting.playerSetting.createSymbols,                "CreateSymbols");
            SetOverridePara(ref setting.playerSetting.macroDefines,                 "MacroDefines");
            SetOverridePara(ref setting.playerSetting.excludedDefines,              "ExcludedDefines");
            SetOverridePara(ref setting.playerSetting.clearRenderPipelineAsset,     "clearRenderPipelineAsset");
            SetOverridePara(ref setting.playerSetting.changelist,                   "Changelist");

            // 版本号修改指令
            bool isVersionNoChanged = false;
            SetOverridePara(ref isVersionNoChanged,                                 "VersionNoChanged",         false);
            bool isVersionIncrease = false;
            SetOverridePara(ref isVersionIncrease,                                  "VersionIncrease",          false);
            string versionSpecific = "";
            SetOverridePara(ref versionSpecific,                                    "VersionSpecific",          "");

            // 指令处理优先级：VersionSpecific > VersionIncrease > VersionNoChanged
            bool isDetermined = false;
            if(!string.IsNullOrEmpty(versionSpecific))
            {
                isDetermined = true;

                string[] splits = versionSpecific.Split(new char[] {'.'}, 3, System.StringSplitOptions.RemoveEmptyEntries);
                
                if(splits.Length != 3 || !int.TryParse(splits[0], out setting.playerSetting.mainVersion)
                || !int.TryParse(splits[1], out setting.playerSetting.minorVersion) || !int.TryParse(splits[2], out setting.playerSetting.revision))
                {
                    Debug.LogError("failed to parse version specific parameter");
                    EditorApplication.Exit(1);
                }
                setting.playerSetting.versionChangedMode = PlayerBuilderSetting.VersionChangedMode.Specific;
            }
            if(!isDetermined && isVersionIncrease)
            {
                isDetermined = true;
                setting.playerSetting.versionChangedMode = PlayerBuilderSetting.VersionChangedMode.Increase;
            }
            if(!isDetermined)
            {
                setting.playerSetting.versionChangedMode = PlayerBuilderSetting.VersionChangedMode.NoChanged;
            }

            BuildGame(setting);
        }

        // 优先使用command传入的参数，否则使用defaultValue
        static private void SetOverridePara(ref string overridePara, string command, string defaultValue)
        {
            CommandLineReader.GetCommand(command, ref defaultValue);
            overridePara = defaultValue;
        }

        // 优先使用command传入的参数，没有则不改变overridePara
        static private void SetOverridePara(ref string overridePara, string command)
        {
            string defaultValue = overridePara;
            SetOverridePara(ref overridePara, command, defaultValue);
        }

        // 优先使用command传入的参数，否则使用defaultValue
        static private void SetOverridePara(ref bool overridePara, string command, bool defaultValue)
        {
            CommandLineReader.GetCommand(command, ref defaultValue);
            overridePara = defaultValue;
        }

        // 优先使用command传入的参数，没有则不改变overridePara
        static private void SetOverridePara(ref bool overridePara, string command)
        {
            bool defaultValue = overridePara;
            SetOverridePara(ref overridePara, command, defaultValue);
        }

        static private void SetOverridePara(ref GameBuilderSetting.BuildMode overridePara, string command, GameBuilderSetting.BuildMode defaultValue)
        {
            int buildMode = (int)defaultValue;
            if(CommandLineReader.GetCommand(command, ref buildMode))
            {
                overridePara = (GameBuilderSetting.BuildMode)buildMode;
            }
            else
            {
                overridePara = defaultValue;
            }
        }

        // static private void SetOverridePara(ref PlayerBuilderSetting.VersionChangedMode overridePara, string command, PlayerBuilderSetting.VersionChangedMode defaultValue)
        // {
        //     overridePara = defaultValue;

        //     int versionMode = 0;
        //     if(CommandLineReader.GetCommand(command, ref versionMode))
        //     {
        //         overridePara = (PlayerBuilderSetting.VersionChangedMode)versionMode;
        //     }
        // }

        private static void CopyStreamingAssetsToCustomPackage()
        {
            string targetPath = @"Assets/CustomAssetPacks.androidpack";
            string streamingAssetPath = @"Assets/StreamingAssets";

            // step1. 重新创建CustomAssetPacks.androidpack
            if (System.IO.Directory.Exists(targetPath))
            {
                System.IO.Directory.Delete(targetPath, true);
            }
            System.IO.Directory.CreateDirectory(targetPath);

            // step2. 重新创建build.gradle
            string text = @"apply plugin: 'com.android.asset-pack'
assetPack {
    packName = ""CustomAssetPacks""
    dynamicDelivery {
        deliveryType = ""install-time""
    }
}";
            System.IO.File.WriteAllText($"{targetPath}/build.gradle", text);

            // step3. 把StreamingAssets移至CustomAssetPacks.androidpack
            Framework.Core.Editor.EditorUtility.CopyDirectory(streamingAssetPath, targetPath);
            System.IO.Directory.Delete(streamingAssetPath, true);

            AssetDatabase.Refresh();
        }

#if UNITY_IOS
            [PostProcessBuildAttribute(1)]
            public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
            {
                //为ios xcode工程增加编译选项-lz，-w
                if (target == BuildTarget.iOS)
                {
                    PBXProject project = new PBXProject();
                    string sPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
                    project.ReadFromFile(sPath);
                    var frameworkGUID = project.GetUnityFrameworkTargetGuid();
                    project.AddBuildProperty(frameworkGUID, "OTHER_LDFLAGS", "-lz");
                    project.AddBuildProperty(frameworkGUID, "OTHER_LDFLAGS", "-w");
                    File.WriteAllText(sPath, project.WriteToString());
                }
            }
#endif
        
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
            abb.assetNames[0] = "assets/res/tech/core/gamedebug.prefab";
            abbs[0] = abb;
            var manifest = BuildPipeline.BuildAssetBundles(outputPath, abbs, options, EditorUserBuildSettings.activeBuildTarget);
            if(manifest != null)
            {
                Debug.Log($"Success to compile scripts");
            }
            else
            {
                Debug.LogError($"Failed to compile scripts");
                if(UnityEngine.Application.isBatchMode)
                    UnityEditor.EditorApplication.Exit(-1);
            }
        }
    }
}