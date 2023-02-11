using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Framework.AssetManagement.AssetEditorWindow
{
    public class TaskPrepare : IGameBuildTask
    {
        void IGameBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            buildParametersContext.BeginWatch();

            var gameBuilderSetting = buildParametersContext.gameBuilderSetting;

            // 检测构建参数合法性
            if (gameBuilderSetting.buildTarget != BuildTarget.StandaloneWindows64 && gameBuilderSetting.buildTarget != BuildTarget.Android && gameBuilderSetting.buildTarget != BuildTarget.iOS)
                throw new Exception("请选择如下三个目标平台：Win64, Android, IOS");
            if (gameBuilderSetting.buildTarget != EditorUserBuildSettings.activeBuildTarget)
                throw new Exception($"选择的发布平台与当前平台不符: {gameBuilderSetting.buildTarget} != {EditorUserBuildSettings.activeBuildTarget}");
            if (BuildPipeline.isBuildingPlayer)
                throw new Exception("当前正在构建资源包，请结束后再试");
            if (EditorTools.HasDirtyScenes())
                throw new Exception("检测到未保存的场景文件");

            // 保存改动的资源
            AssetDatabase.SaveAssets();

            {
                // 删除平台总目录
                //string platformDirectory = $"{buildParameters.OutputRoot}/{buildParameters.PackageName}/{buildParameters.BuildTarget}";
                //if (EditorTools.DeleteDirectory(platformDirectory))
                //{
                //    BuildRunner.Log($"删除平台总目录：{platformDirectory}");
                //}
            }

            // 创建输出目录
            string pipelineOutputDirectory = AssetBundleBuilderHelper.GetTargetBundlesOutput("v0.0.1", gameBuilderSetting.buildTarget);
            if (EditorTools.CreateDirectory(pipelineOutputDirectory))
            {
                BuildRunner.Log($"创建输出目录：{pipelineOutputDirectory}");
            }
        }
    }
}