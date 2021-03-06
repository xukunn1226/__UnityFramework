using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using Framework.Core;

namespace Framework.AssetManagement.GameBuilder
{
    /// <summary>
    /// Player打包配置数据
    /// </summary>
    public class PlayerBuilderSetting : ScriptableObject
    {
        public enum VersionChangedMode
        {
            NoChanged,      // 无变化
            Increase,       // 自增，末位+1
            Specific,       // 指定版本号
        }

        public string                       outputPath                          = "Deployment/Player";

        public string                       projectName                         = "MyProject";
        
        ////////////////////// BuildOptions
        public bool                         autoRunPlayer;                      // Run the built player

        public bool                         development;                        // includes symbols and enables the profiler

        public bool                         connectWithProfiler;                // When the build is started, an open Profiler Window will automatically connect to the Player and start profiling

        public bool                         allowDebugging;                     // Allow script debuggers to attach to the player remotely

        public bool                         buildScriptsOnly;                   // 仅编译脚本，需与development使用，首次build player时不可使用

        public bool                         compressWithLz4;                    // Use chunk-based LZ4 compression when building the Player

        public bool                         compressWithLz4HC;                  // Use chunk-based LZ4 high-compression when building the Player.

        public bool                         strictMode;                         // Do not allow the build to succeed if any errors are reporting during it.

        ////////////////////// PlayerSettings
        public string                       bundleVersion;                      // Application bundle version shared between iOS & Android platforms
        
        [System.NonSerialized]
        public VersionChangedMode           versionChangedMode;                 // 版本号设定方式
        [System.NonSerialized] public int   mainVersion;
        [System.NonSerialized] public int   minorVersion;
        [System.NonSerialized] public int   revision;

        public bool                         useIL2CPP;                          // Sets the scripting framework for a BuildTargetPlatformGroup

        public Il2CppCompilerConfiguration  il2CppCompilerConfiguration;        // C++ compiler configuration used when compiling IL2CPP generated code.

        public bool                         useMTRendering;                     // whether to use multithreaded rendering option for mobile platform

        public bool                         useAPKExpansionFiles;               // When enabled the player executable and data will be split up

        public string                       macroDefines;

        public string                       excludedDefines;

        public string                       cachedBundleVersion                 { get; set; }

        public bool                         cachedUseIL2CPP                     { get; set; }

        public Il2CppCompilerConfiguration  cachedIl2CppCompilerConfigureation  { get; set; }

        public bool                         cachedUseMTRendering                { get; set; }

        public bool                         cachedUseAPKExpansionFiles          { get; set; }

        public string                       cachedMacroDefines                  { get; set; }

        ////////////////////// Override Build Scenes
        public bool                         bOverrideBuildScenes;               // 

        public List<string>                 overrideBuildScenes;

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(string.Format($"outputPath: {outputPath}  \n"));
            sb.Append(string.Format($"projectName: {projectName}  \n"));
            sb.Append(string.Format($"autoRunPlayer: {autoRunPlayer}  \n"));
            sb.Append(string.Format($"development: {development}  \n"));
            sb.Append(string.Format($"connectWithProfiler: {connectWithProfiler}  \n"));
            sb.Append(string.Format($"allowDebugging: {allowDebugging}  \n"));
            sb.Append(string.Format($"buildScriptsOnly: {buildScriptsOnly}  \n"));
            sb.Append(string.Format($"compressWithLz4: {compressWithLz4}  \n"));
            sb.Append(string.Format($"compressWithLz4HC: {compressWithLz4HC}  \n"));
            sb.Append(string.Format($"strictMode: {strictMode}  \n"));
            sb.Append(string.Format($"bundleVersion: {bundleVersion}  \n"));
            sb.Append(string.Format($"useIL2CPP: {useIL2CPP}  \n"));
            sb.Append(string.Format($"il2CppCompilerConfiguration: {il2CppCompilerConfiguration}  \n"));
            sb.Append(string.Format($"useMTRendering: {useMTRendering}  \n"));
            sb.Append(string.Format($"useAPKExpansionFiles: {useAPKExpansionFiles}  \n"));
            sb.Append(string.Format($"macroDefines: {macroDefines}  \n"));
            sb.Append(string.Format($"excludedDefines: {excludedDefines}  \n"));
            return sb.ToString();
        }
    }

    static public class PlayerBuilderSettingExtension
    {
        static internal void SetupPlayerSettings(this PlayerBuilderSetting para, AppVersion version)
        {
            BuildTargetGroup buildTargetGroup = GameBuilderUtil.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

            // cache player settings
            para.cachedBundleVersion                = PlayerSettings.bundleVersion;
            para.cachedUseIL2CPP                    = PlayerSettings.GetScriptingBackend(buildTargetGroup) == ScriptingImplementation.IL2CPP;
            para.cachedIl2CppCompilerConfigureation = PlayerSettings.GetIl2CppCompilerConfiguration(buildTargetGroup);
            para.cachedUseMTRendering               = PlayerSettings.GetMobileMTRendering(buildTargetGroup);
            para.cachedUseAPKExpansionFiles         = PlayerSettings.Android.useAPKExpansionFiles;
            para.cachedMacroDefines                 = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            // setup new settings
            // PlayerSettings.bundleVersion = para.bundleVersion;
            PlayerSettings.bundleVersion = version.ToString();

            if(buildTargetGroup == BuildTargetGroup.Android || buildTargetGroup == BuildTargetGroup.iOS)
            {
                PlayerSettings.SetScriptingBackend(buildTargetGroup, para.useIL2CPP ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);

                if(para.useIL2CPP)
                {
                    PlayerSettings.SetIl2CppCompilerConfiguration(buildTargetGroup, para.il2CppCompilerConfiguration);
                }

                PlayerSettings.SetMobileMTRendering(buildTargetGroup, para.useMTRendering);
                PlayerSettings.MTRendering = para.useMTRendering;

                if(buildTargetGroup == BuildTargetGroup.Android)
                {
                    PlayerSettings.Android.useAPKExpansionFiles = para.useAPKExpansionFiles;
                    PlayerSettings.Android.bundleVersionCode = version.BuildNumber;
                }
                else
                {
                    PlayerSettings.iOS.buildNumber = version.BuildNumber.ToString();
                }
            }

            // final macro defines = PlayerSetting.ScriptingDefineSymbols + PlayerBuilderSetting.macroDefines - PlayerBuilderSetting.excludedDefines
            HashSet<string> macroSet = new HashSet<string>();
            macroSet.UnionWith(para.cachedMacroDefines.Split(new char[]{';'}));
            macroSet.UnionWith(para.macroDefines.Split(new char[]{';'}));

            string[] exDefines = para.excludedDefines.Split(new char[] {';'});
            foreach(var ex in exDefines)
            {
                macroSet.Remove(ex);
            }

            // 分包发布时加上特定macro以控制obb流程
            if(para.useAPKExpansionFiles && buildTargetGroup == BuildTargetGroup.Android)
            {
                macroSet.Add("USE_APK_EXPANSIONFILES");
            }

            string finalMacroDefines = string.Join(";", macroSet.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, finalMacroDefines.Trim(new char[] {';'}));
        }

        static internal void RestorePlayerSettings(this PlayerBuilderSetting para)
        {
            BuildTargetGroup buildTargetGroup = GameBuilderUtil.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

            PlayerSettings.bundleVersion = para.cachedBundleVersion;

            if (buildTargetGroup == BuildTargetGroup.Android || buildTargetGroup == BuildTargetGroup.iOS)
            {
                PlayerSettings.SetScriptingBackend(buildTargetGroup, para.cachedUseIL2CPP ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);

                if(para.cachedUseIL2CPP)
                {
                    PlayerSettings.SetIl2CppCompilerConfiguration(buildTargetGroup, para.cachedIl2CppCompilerConfigureation);
                }

                PlayerSettings.SetMobileMTRendering(buildTargetGroup, para.cachedUseMTRendering);
                PlayerSettings.MTRendering = para.cachedUseMTRendering;

                if(buildTargetGroup == BuildTargetGroup.Android)
                {
                    PlayerSettings.Android.useAPKExpansionFiles = para.cachedUseAPKExpansionFiles;
                }
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, para.cachedMacroDefines);
        }

        static internal BuildPlayerOptions GenerateBuildPlayerOptions(this PlayerBuilderSetting para)
        {
            BuildPlayerOptions opt = new BuildPlayerOptions();
            opt.locationPathName = para.GetLocalPathName();
            opt.scenes = GetBuildScenes(para);
            opt.target = EditorUserBuildSettings.activeBuildTarget;
            opt.options = para.GenerateBuildOptions();
            return opt;
        }

        static string[] GetBuildScenes(PlayerBuilderSetting para)
        {
            List<string> names = new List<string>();
            if (para.bOverrideBuildScenes)
            {
                foreach(var scenePath in para.overrideBuildScenes)
                {
                    if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
                    {
                        Debug.LogWarning($"Can't find SceneAsset at [{scenePath}]");
                        continue;
                    }

                    names.Add(scenePath);
                }
            }
            else
            {
                foreach (var scene in EditorBuildSettings.scenes)
                {
                    if (scene == null || !scene.enabled)
                        continue;

                    if (AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(scene.guid.ToString())) == null)
                        continue;

                    names.Add(scene.path);
                }
            }
            return names.ToArray();
        }

        static internal string GetLocalPathName(this PlayerBuilderSetting para)
        {
            string extension = string.Empty;
            switch(EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows64:
                    extension = ".exe";
                    break;
                case BuildTarget.Android:
                    extension = ".apk";
                    break;
                case BuildTarget.iOS:
                    extension = ".ipa";
                    break;
            }

            return string.Format("{0}/{1}/{2}{3}", para.outputPath, Utility.GetPlatformName(), para.projectName, extension);
        }

        static private BuildOptions GenerateBuildOptions(this PlayerBuilderSetting para)
        {
            BuildOptions opt = BuildOptions.None;

            if (para.autoRunPlayer)
                opt |= BuildOptions.AutoRunPlayer;
            else
                opt &= ~BuildOptions.AutoRunPlayer;

            if (para.development)
                opt |= BuildOptions.Development;
            else
                opt &= ~BuildOptions.Development;

            if (para.connectWithProfiler && para.development)
                opt |= BuildOptions.ConnectWithProfiler;
            else
                opt &= ~BuildOptions.ConnectWithProfiler;

            if (para.allowDebugging && para.development)
                opt |= BuildOptions.AllowDebugging;
            else
                opt &= ~BuildOptions.AllowDebugging;

            if (para.buildScriptsOnly && para.development)
                opt |= BuildOptions.BuildScriptsOnly;
            else
                opt &= ~BuildOptions.BuildScriptsOnly;

            if (para.compressWithLz4HC)
            {
                opt |= BuildOptions.CompressWithLz4HC;
                opt &= ~BuildOptions.CompressWithLz4;
            }
            else
                opt &= ~BuildOptions.CompressWithLz4HC;

            if (para.compressWithLz4 && !para.compressWithLz4HC)
                opt |= BuildOptions.CompressWithLz4;
            else
                opt &= ~BuildOptions.CompressWithLz4;

            if (para.strictMode)
                opt |= BuildOptions.StrictMode;
            else
                opt &= ~BuildOptions.StrictMode;

            return opt;
        }

        static internal AppVersion SetAppVersion(this PlayerBuilderSetting para)
        {
            AppVersion version = AppVersion.EditorLoad();
            if(version == null)
                throw new System.Exception("can't find AppVersion.asset in Assets/Resources");

            switch(para.versionChangedMode)
            {
                case PlayerBuilderSetting.VersionChangedMode.NoChanged:
                    break;
                case PlayerBuilderSetting.VersionChangedMode.Increase:
                    version.Grow();
                    EditorUtility.SetDirty(version);
                    break;
                case PlayerBuilderSetting.VersionChangedMode.Specific:
                    version.Set(para.mainVersion, para.minorVersion, para.revision);
                    EditorUtility.SetDirty(version);
                    break;
            }
            AssetDatabase.SaveAssets();
            return version;
        }
    }
}