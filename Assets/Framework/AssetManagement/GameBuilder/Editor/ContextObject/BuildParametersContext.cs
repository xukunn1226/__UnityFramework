using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using UnityEditor.Build.Pipeline;

namespace Framework.AssetManagement.AssetEditorWindow
{
    /// <summary>
    /// 构建参数
    /// </summary>
    public class BuildParametersContext : IContextObject
    {
        private readonly Stopwatch m_BuildWatch = new Stopwatch();
        public GameBuilderSetting gameBuilderSetting { get; private set; }

        public BuildParametersContext(GameBuilderSetting gameBuilderSetting)
        {
            this.gameBuilderSetting = gameBuilderSetting;
        }

        public float GetBuildingSeconds()
        {
            float seconds = m_BuildWatch.ElapsedMilliseconds / 1000f;
            return seconds;
        }
        public void BeginWatch()
        {
            m_BuildWatch.Start();
        }
        public void StopWatch()
        {
            m_BuildWatch.Stop();
        }

        public string GetBundlesOutput()
        {
            return AssetBundleBuilderHelper.GetTargetBundlesOutput(gameBuilderSetting.packageVersion, gameBuilderSetting.buildTarget);
        }

        public BundleBuildParameters GetSBPBuildParameters()
		{
            var buildParams = new BundleBuildParameters(gameBuilderSetting.buildTarget,
                                                        BuildPipeline.GetBuildTargetGroup(gameBuilderSetting.buildTarget),
                                                        GetBundlesOutput());

            buildParams.BundleCompression = gameBuilderSetting.bundleSetting.useLZ4Compress ? BuildCompression.LZ4 : BuildCompression.Uncompressed;
            buildParams.UseCache = !gameBuilderSetting.bundleSetting.rebuildBundles;
            if (gameBuilderSetting.bundleSetting.DisableWriteTypeTree)
                buildParams.ContentBuildFlags |= UnityEditor.Build.Content.ContentBuildFlags.DisableWriteTypeTree;

			return buildParams;
		}
    }
}