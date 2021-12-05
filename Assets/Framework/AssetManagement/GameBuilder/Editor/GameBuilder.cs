using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.Core;
using UnityEditor.Build.Reporting;

namespace Framework.AssetManagement.GameBuilder
{
    public class GameBuilder
    {
        /// <summary>
        /// 构建app接口
        /// 允许仅构建bundles或player
        /// </summary>
        /// <param name="bundleSetting"></param>
        /// <param name="playerSetting"></param>
        static public void BuildGame(GameBuilderSetting gameSetting)
        {
            if(gameSetting == null)
            {
                if(UnityEngine.Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                throw new System.ArgumentNullException("GameBuilderSetting", "gameSetting == null");
            }

            if (gameSetting.bundleSetting == null)
            {
                if(UnityEngine.Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                throw new System.ArgumentNullException("BundleBuilderSetting", "bundleSetting == null");
            }

            if (gameSetting.playerSetting == null)
            {
                if (UnityEngine.Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                throw new System.ArgumentNullException("PlayerBuilderSetting", "playerSetting == null");
            }

            if(gameSetting.buildTarget != EditorUserBuildSettings.activeBuildTarget)
            {
                if (UnityEngine.Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                throw new System.InvalidOperationException($"build target  [{gameSetting.buildTarget}] not match the active build target  [{EditorUserBuildSettings.activeBuildTarget}]");
            }

            switch(gameSetting.buildMode)
            {
                case GameBuilderSetting.BuildMode.Bundles:
                    BundleBuilder.BuildAssetBundles(gameSetting.bundleSetting);
                    break;
                case GameBuilderSetting.BuildMode.Player:
                    PlayerBuilder.BuildPlayer(gameSetting.playerSetting);
                    break;
                case GameBuilderSetting.BuildMode.BundlesAndPlayer:
                    if(BundleBuilder.BuildAssetBundles(gameSetting.bundleSetting))
                    {
                        PlayerBuilder.BuildPlayer(gameSetting.playerSetting);
                    }
                    break;
            }
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
            SetOverridePara(ref setting.bundleSetting.outputPath,                   "BundlesOutput",            "Deployment/AssetBundles");
            SetOverridePara(ref setting.bundleSetting.useLZ4Compress,               "UseLZ4Compress",           true);
            SetOverridePara(ref setting.bundleSetting.appendHash,                   "AppendHash",               false);
            SetOverridePara(ref setting.bundleSetting.rebuildBundles,               "RebuildBundles",           false);

            // override the player setting parameters
            SetOverridePara(ref setting.playerSetting.outputPath,                   "PlayerOutput",             "Deployment/Player");            
            SetOverridePara(ref setting.playerSetting.projectName,                  "ProjectName",              "MyProject");
            SetOverridePara(ref setting.playerSetting.autoRunPlayer,                "AutoRunPlayer",            true);
            // batch mode没有连接设备会出包失败
            if (UnityEngine.Application.isBatchMode)
                setting.playerSetting.autoRunPlayer = false;
            SetOverridePara(ref setting.playerSetting.development,                  "Development",              true);
            SetOverridePara(ref setting.playerSetting.connectWithProfiler,          "ConnectWithProfiler",      false);
            SetOverridePara(ref setting.playerSetting.allowDebugging,               "AllowDebugging",           false);
            SetOverridePara(ref setting.playerSetting.buildScriptsOnly,             "BuildScriptsOnly",         false);
            SetOverridePara(ref setting.playerSetting.compressWithLz4,              "CompressWithLz4",          false);
            SetOverridePara(ref setting.playerSetting.compressWithLz4HC,            "CompressWithLz4HC",        false);
            SetOverridePara(ref setting.playerSetting.strictMode,                   "PlayerStrictMode",         true);
            // SetOverridePara(ref setting.playerSetting.bundleVersion,                "BundleVersion",            "0.1");
            SetOverridePara(ref setting.playerSetting.useIL2CPP,                    "useIL2CPP",                true);
            SetOverridePara(ref setting.playerSetting.il2CppCompilerConfiguration,  "CompilerConfiguration",    "Master");
            SetOverridePara(ref setting.playerSetting.useMTRendering,               "UseMTRendering",           true);
            SetOverridePara(ref setting.playerSetting.buildAppBundle,               "BuildAppBundle",           false);
            SetOverridePara(ref setting.playerSetting.createSymbols,                "CreateSymbols",            false);
            SetOverridePara(ref setting.playerSetting.macroDefines,                 "MacroDefines",             "");
            SetOverridePara(ref setting.playerSetting.excludedDefines,              "ExcludedDefines",          "");

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

        static private void SetOverridePara(ref string overridePara, string command, string defaultValue)
        {
            if (CommandLineReader.GetCommand(command, ref defaultValue))
                overridePara = defaultValue;
        }

        static private void SetOverridePara(ref bool overridePara, string command, bool defaultValue)
        {
            if (CommandLineReader.GetCommand(command, ref defaultValue))
                overridePara = defaultValue;
        }

        static private void SetOverridePara(ref Il2CppCompilerConfiguration overridePara, string command, string defaultValue)
        {
            if (CommandLineReader.GetCommand(command, ref defaultValue))
            {
                string lowerValue = defaultValue.ToLower();
                if(lowerValue == "debug")
                {
                    overridePara = Il2CppCompilerConfiguration.Debug;
                }
                else if(lowerValue == "release")
                {
                    overridePara = Il2CppCompilerConfiguration.Release;
                }
                else if(lowerValue == "master")
                {
                    overridePara = Il2CppCompilerConfiguration.Master;
                }
            }
        }

        static private void SetOverridePara(ref GameBuilderSetting.BuildMode overridePara, string command, GameBuilderSetting.BuildMode defaultValue)
        {
            overridePara = defaultValue;

            int buildMode = 0;
            if(CommandLineReader.GetCommand(command, ref buildMode))
            {
                overridePara = (GameBuilderSetting.BuildMode)buildMode;
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
    }
}