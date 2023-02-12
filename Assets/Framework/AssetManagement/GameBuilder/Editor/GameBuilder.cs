using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.Core;
using UnityEditor.Build.Reporting;

namespace Framework.AssetManagement.AssetEditorWindow
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
            var buildResult = GameBuilderEx.Run(gameSetting);
            if (buildResult.Success)
            {
                EditorUtility.RevealInFinder(buildResult.OutputPackageDirectory);
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
            SetOverridePara(ref setting.playerSetting.il2CppCompilerConfiguration,  "CompilerConfiguration");
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

        static private void SetOverridePara(ref Il2CppCompilerConfiguration overridePara, string command)
        {
            string defaultValue = string.Empty;
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
    }
}