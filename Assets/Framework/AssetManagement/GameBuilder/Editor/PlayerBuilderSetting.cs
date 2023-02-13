using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using UnityEditor;
using Framework.Core;

namespace Framework.AssetManagement.AssetEditorWindow
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

        public string                       projectName                         = "MyProject";        
        ////////////////////// BuildOptions
        public bool                         development;                        // includes symbols and enables the profiler
        public bool                         compressWithLz4;                    // Use chunk-based LZ4 compression when building the Player
        public bool                         compressWithLz4HC;                  // Use chunk-based LZ4 high-compression when building the Player.
        public bool                         strictMode;                         // Do not allow the build to succeed if any errors are reporting during it.
        [System.NonSerialized]
        public VersionChangedMode           versionChangedMode;                 // 版本号设定方式
        [System.NonSerialized] public int   mainVersion;
        [System.NonSerialized] public int   minorVersion;
        [System.NonSerialized] public int   revision;
        public bool                         releaseNative;                      // 发布原生模式，目前需要手动更改Application.Logic.asmdef支持android&ios平台
        public bool                         useIL2CPP;                          // Sets the scripting framework for a BuildTargetPlatformGroup
        public bool                         useMTRendering;                     // whether to use multithreaded rendering option for mobile platform
        public bool                         buildAppBundle;                     // Set to true to build an Android App Bundle (aab file) instead of an apk
        public bool                         createSymbols;
        public bool                         useCustomKeystore;
        public string                       keystoreName;
        public string                       keystorePass;
        public string                       keyaliasName;
        public string                       keyaliasPass;
        public string                       macroDefines;
        public string                       excludedDefines;
        public bool                         clearRenderPipelineAsset;



        public bool                         cachedUseIL2CPP                     { get; set; }
        public bool                         cachedUseMTRendering                { get; set; }
        public bool                         cachedBuildAppBundle                { get; set; }
        public bool                         cachedCreateSymbols                 { get; set; }
        public string                       cachedMacroDefines                  { get; set; }                
        public List<string>                 cachedRenderPipelineAsset           { get; set; }          // cache Quality Settings' Render Pipeline Asset
        public int                          curQualityLevel                     { get; set; }
        public RenderPipelineAsset          cachedGraphicsRenderPipelineAsset   { get; set; }
        [System.NonSerialized]
        public string                       changelist;

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(string.Format($"projectName: {projectName}  \n"));
            sb.Append(string.Format($"development: {development}  \n"));
            sb.Append(string.Format($"compressWithLz4: {compressWithLz4}  \n"));
            sb.Append(string.Format($"compressWithLz4HC: {compressWithLz4HC}  \n"));
            sb.Append(string.Format($"strictMode: {strictMode}  \n"));
            // sb.Append(string.Format($"bundleVersion: {bundleVersion}  \n"));
            sb.Append(string.Format($"releaseNative: {releaseNative}    \n"));
            sb.Append(string.Format($"useIL2CPP: {useIL2CPP}  \n"));
            sb.Append(string.Format($"useMTRendering: {useMTRendering}  \n"));
            sb.Append(string.Format($"buildAppBundle: {buildAppBundle}  \n"));
            sb.Append(string.Format($"createSymbols: {createSymbols}    \n"));
            sb.Append(string.Format($"useCustomKeystore: {useCustomKeystore}    \n"));
            sb.Append(string.Format($"keystoreName: {keystoreName}    \n"));
            sb.Append(string.Format($"keystorePass: {keystorePass}    \n"));
            sb.Append(string.Format($"keyaliasName: {keyaliasName}    \n"));
            sb.Append(string.Format($"keyaliasPass: {keyaliasPass}    \n"));
            sb.Append(string.Format($"macroDefines: {macroDefines}  \n"));
            sb.Append(string.Format($"excludedDefines: {excludedDefines}  \n"));
            sb.Append(string.Format($"clearRenderPipelineAsset: {clearRenderPipelineAsset}  \n"));
            return sb.ToString();
        }
    }
}