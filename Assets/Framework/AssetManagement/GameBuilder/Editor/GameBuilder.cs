using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.Core;

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
                if(Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                throw new System.ArgumentNullException("GameBuilderSetting", "gameSetting == null");
            }

            if (gameSetting.bundleSetting == null)
            {
                if(Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                throw new System.ArgumentNullException("BundleBuilderSetting", "bundleSetting == null");
            }

            if (gameSetting.playerSetting == null)
            {
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                throw new System.ArgumentNullException("PlayerBuilderSetting", "playerSetting == null");
            }

            if(BundleBuilder.BuildAssetBundles(gameSetting.bundleSetting))
            {
                PlayerBuilder.BuildPlayer(gameSetting.playerSetting);
            }
        }

        static public void cmdBuildGame()
        {
            if (!Application.isBatchMode)
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

            // override the bundle setting parameters
            SetOverridePara(ref setting.bundleSetting.outputPath,                   "BundlesOutput",            "Assets/Deployment/AssetBundles");
            SetOverridePara(ref setting.bundleSetting.useLZ4Compress,               "UseLZ4Compress",           true);
            SetOverridePara(ref setting.bundleSetting.appendHash,                   "AppendHash",               false);
            // SetOverridePara(ref setting.bundleSetting.rebuildBundles,               "RebuildBundles",           false);

            // override the player setting parameters
            SetOverridePara(ref setting.playerSetting.outputPath,                   "PlayerOutput",             "Assets/Deployment/Player");            
            SetOverridePara(ref setting.playerSetting.projectName,                  "ProjectName",              "MyProject");
            SetOverridePara(ref setting.playerSetting.autoRunPlayer,                "AutoRunPlayer",            true);
            // batch mode没有连接设备会出包失败
            if (Application.isBatchMode)
                setting.playerSetting.autoRunPlayer = false;
            SetOverridePara(ref setting.playerSetting.development,                  "Development",              true);
            SetOverridePara(ref setting.playerSetting.connectWithProfiler,          "ConnectWithProfiler",      false);
            SetOverridePara(ref setting.playerSetting.allowDebugging,               "AllowDebugging",           false);
            SetOverridePara(ref setting.playerSetting.buildScriptsOnly,             "BuildScriptsOnly",         false);
            SetOverridePara(ref setting.playerSetting.compressWithLz4,              "CompressWithLz4",          false);
            SetOverridePara(ref setting.playerSetting.compressWithLz4HC,            "CompressWithLz4HC",        false);
            SetOverridePara(ref setting.playerSetting.strictMode,                   "PlayerStrictMode",         true);
            SetOverridePara(ref setting.playerSetting.bundleVersion,                "BundleVersion",            "0.1");
            SetOverridePara(ref setting.playerSetting.useIL2CPP,                    "useIL2CPP",                true);
            SetOverridePara(ref setting.playerSetting.il2CppCompilerConfiguration,  "CompilerConfiguration",    "Master");
            SetOverridePara(ref setting.playerSetting.useMTRendering,               "UseMTRendering",           true);
            SetOverridePara(ref setting.playerSetting.useAPKExpansionFiles,         "UseAPKExpansionFiles",     true);
            SetOverridePara(ref setting.playerSetting.macroDefines,                 "MacroDefines",             "");
            SetOverridePara(ref setting.playerSetting.excludedDefines,              "ExcludedDefines",          "");

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
    }
}