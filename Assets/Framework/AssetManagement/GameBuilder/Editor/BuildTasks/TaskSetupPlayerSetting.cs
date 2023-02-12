using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Linq;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [TaskAttribute("Step8. 设置PlayerSetting")]
    public class TaskSetupPlayerSetting : IGameBuildTask
    {
        void IGameBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            SetupPlayerSettings(buildParametersContext.gameBuilderSetting.playerSetting, 0);
        }

        /// <summary>
        /// 设置PlayerSetting参数
        /// </summary>
        /// <param name="para"></param>
        /// <param name="buildNumber"></param>
        private void SetupPlayerSettings(PlayerBuilderSetting para, int buildNumber)
        {
            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

            // cache player settings
            para.cachedUseIL2CPP = PlayerSettings.GetScriptingBackend(buildTargetGroup) == ScriptingImplementation.IL2CPP;
            para.cachedIl2CppCompilerConfigureation = PlayerSettings.GetIl2CppCompilerConfiguration(buildTargetGroup);
            para.cachedUseMTRendering = PlayerSettings.GetMobileMTRendering(buildTargetGroup);
            para.cachedBuildAppBundle = EditorUserBuildSettings.buildAppBundle;
            para.cachedCreateSymbols = EditorUserBuildSettings.androidCreateSymbols == AndroidCreateSymbols.Disabled;
            para.cachedMacroDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            PlayerSettings.SetScriptingBackend(buildTargetGroup, para.useIL2CPP ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);

            if (para.useIL2CPP)
            {
                PlayerSettings.SetIl2CppCompilerConfiguration(buildTargetGroup, para.il2CppCompilerConfiguration);
            }

            PlayerSettings.SetMobileMTRendering(buildTargetGroup, para.useMTRendering);
            PlayerSettings.MTRendering = para.useMTRendering;

            if (buildTargetGroup == BuildTargetGroup.Android)
            {
                PlayerSettings.Android.targetArchitectures = para.useIL2CPP ? AndroidArchitecture.All : AndroidArchitecture.ARMv7;
                EditorUserBuildSettings.buildAppBundle = para.buildAppBundle;
                EditorUserBuildSettings.androidCreateSymbols = para.createSymbols ? AndroidCreateSymbols.Debugging : AndroidCreateSymbols.Disabled;
                PlayerSettings.Android.bundleVersionCode = buildNumber;

                PlayerSettings.Android.useCustomKeystore = para.useCustomKeystore;
                PlayerSettings.Android.keystoreName = para.keystoreName;
                PlayerSettings.Android.keystorePass = para.keystorePass;
                PlayerSettings.Android.keyaliasName = para.keyaliasName;
                PlayerSettings.Android.keyaliasPass = para.keyaliasPass;
            }
            else if (buildTargetGroup == BuildTargetGroup.iOS)
            {
                PlayerSettings.iOS.buildNumber = buildNumber.ToString();
            }

            // final macro defines = PlayerSetting.ScriptingDefineSymbols + PlayerBuilderSetting.macroDefines - PlayerBuilderSetting.excludedDefines
            HashSet<string> macroSet = new HashSet<string>();
            macroSet.UnionWith(para.cachedMacroDefines.Split(new char[] { ';' }));
            macroSet.UnionWith(para.macroDefines.Split(new char[] { ';' }));

            string[] exDefines = para.excludedDefines.Split(new char[] { ';' });
            foreach (var ex in exDefines)
            {
                macroSet.Remove(ex);
            }

            string finalMacroDefines = string.Join(";", macroSet.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, finalMacroDefines.Trim(new char[] { ';' }));

            CacheQualityRenderPipelineAsset(para);
        }

        /// <summary>
        /// 缓存当前URP管线资产的设置，把其设置为empty_universalrp.asset
        /// </summary>
        /// <param name="para"></param>
        private void CacheQualityRenderPipelineAsset(PlayerBuilderSetting para)
        {
            if (!para.clearRenderPipelineAsset)
                return;

            para.cachedGraphicsRenderPipelineAsset = GraphicsSettings.defaultRenderPipeline;
            para.curQualityLevel = QualitySettings.GetQualityLevel();
            para.cachedRenderPipelineAsset = new List<string>();
            for (int i = 0; i < QualitySettings.names.Length; ++i)
            {
                RenderPipelineAsset asset = QualitySettings.GetRenderPipelineAssetAt(i);
                para.cachedRenderPipelineAsset.Add(asset == null ? null : AssetDatabase.GetAssetPath(asset));
            }

            GraphicsSettings.defaultRenderPipeline = null;
            RenderPipelineAsset rawPipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("assets/settings/empty_universalrp.asset");
            for (int i = 0; i < QualitySettings.names.Length; ++i)
            {
                QualitySettings.SetQualityLevel(i);
                QualitySettings.renderPipeline = rawPipelineAsset;
            }
        } 
    }
}