using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [TaskAttribute("Step1. 构建准备工作")]
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

            // 创建输出目录
            string bundleOutputDirectory = AssetBundleBuilderHelper.GetCacheBundlesOutput(gameBuilderSetting.buildTarget);
            string playerOutputDirectory = AssetBundleBuilderHelper.GetCachePlayerOutput(gameBuilderSetting.buildTarget);
            string streamingOutputDirectory = AssetBundleBuilderHelper.GetCacheStreamingOutput(gameBuilderSetting.buildTarget);
            EditorTools.DeleteDirectory(bundleOutputDirectory);
            EditorTools.DeleteDirectory(playerOutputDirectory);
            EditorTools.DeleteDirectory(streamingOutputDirectory);
            EditorTools.CreateDirectory(bundleOutputDirectory);
            EditorTools.CreateDirectory(playerOutputDirectory);
            EditorTools.CreateDirectory(streamingOutputDirectory);
            BuildRunner.Log($"创建输出目录：{bundleOutputDirectory}、{playerOutputDirectory}、{streamingOutputDirectory}");
        }
    }
}