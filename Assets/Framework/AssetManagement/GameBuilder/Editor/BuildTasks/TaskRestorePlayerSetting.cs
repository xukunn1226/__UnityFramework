using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [TaskAttribute("Step10. 还原PlayerSetting")]
    public class TaskRestorePlayerSetting : IGameBuildTask
    {
        void IGameBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            RestorePlayerSetting(buildParametersContext.gameBuilderSetting.playerSetting);
        }

        /// <summary>
        /// 还原之前缓存的PlayerSetting参数
        /// </summary>
        /// <param name="para"></param>
        private void RestorePlayerSetting(PlayerBuilderSetting para)
        {
            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

            if (buildTargetGroup == BuildTargetGroup.Android || buildTargetGroup == BuildTargetGroup.iOS)
            {
                PlayerSettings.SetScriptingBackend(buildTargetGroup, para.cachedUseIL2CPP ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
                PlayerSettings.SetMobileMTRendering(buildTargetGroup, para.cachedUseMTRendering);
                PlayerSettings.MTRendering = para.cachedUseMTRendering;

                if (buildTargetGroup == BuildTargetGroup.Android)
                {
                    EditorUserBuildSettings.buildAppBundle = para.cachedBuildAppBundle;
                    EditorUserBuildSettings.androidCreateSymbols = para.cachedCreateSymbols ? AndroidCreateSymbols.Debugging : AndroidCreateSymbols.Disabled;

                    PlayerSettings.Android.useCustomKeystore = false;
                    PlayerSettings.Android.keystoreName = string.Empty;
                    PlayerSettings.Android.keystorePass = string.Empty;
                    PlayerSettings.Android.keyaliasName = string.Empty;
                    PlayerSettings.Android.keyaliasPass = string.Empty;
                }
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, para.cachedMacroDefines);

            RestoreQualityRenderPipelineAsset(para);

            AssetDatabase.SaveAssets();
        }

        private void RestoreQualityRenderPipelineAsset(PlayerBuilderSetting para)
        {
            // 编辑器下打包不执行
            if(!UnityEngine.Application.isBatchMode)
                return;

            if (!para.clearRenderPipelineAsset)
                return;

            GraphicsSettings.defaultRenderPipeline = para.cachedGraphicsRenderPipelineAsset;
            for (int i = 0; i < para.cachedRenderPipelineAsset.Count; ++i)
            {
                QualitySettings.SetQualityLevel(i);
                QualitySettings.renderPipeline = string.IsNullOrEmpty(para.cachedRenderPipelineAsset[i]) ? null : AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(para.cachedRenderPipelineAsset[i]);
            }
            QualitySettings.SetQualityLevel(para.curQualityLevel);
        }
    }
}