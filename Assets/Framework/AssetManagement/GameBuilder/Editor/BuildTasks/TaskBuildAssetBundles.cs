using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [TaskAttribute("构建资源包(SBP)")]
    public class TaskBuildAssetBundles : IGameBuildTask
    {
        void IGameBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();

            // 开始构建
            IBundleBuildResults buildResults;
            var buildParameters = buildParametersContext.GetSBPBuildParameters();
            ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParameters, 
                                                                    new BundleBuildContent(buildMapContext.GetPipelineBuilds()), 
                                                                    out buildResults,
                                                                    AssetBundleCompatible(true));
            if (exitCode < 0)
            {
                throw new Exception($"构建过程中发生错误 : {exitCode}");
            }

            BuildRunner.Log("Unity引擎打包成功！");
            BuildResultContext buildResultContext = new BuildResultContext();
            buildResultContext.Results = buildResults;
            context.SetContextObject(buildResultContext);
        }

        static IList<IBuildTask> AssetBundleCompatible(bool builtinTask)
        {
            var buildTasks = new List<IBuildTask>();

            // Setup
            buildTasks.Add(new SwitchToBuildPlatform());
            buildTasks.Add(new RebuildSpriteAtlasCache());

            // Player Scripts
            buildTasks.Add(new BuildPlayerScripts());
            buildTasks.Add(new PostScriptsCallback());

            // Dependency
            buildTasks.Add(new CalculateSceneDependencyData());
#if UNITY_2019_3_OR_NEWER
            buildTasks.Add(new CalculateCustomDependencyData());
#endif
            buildTasks.Add(new CalculateAssetDependencyData());
            buildTasks.Add(new StripUnusedSpriteSources());
            if (builtinTask)
                buildTasks.Add(new CreateBuiltInResourceBundle("builtin_shaders", "builtin_resources"));
            buildTasks.Add(new PostDependencyCallback());

            // Packing
            buildTasks.Add(new GenerateBundlePacking());
            if (builtinTask)
                buildTasks.Add(new UpdateBundleObjectLayout());
            buildTasks.Add(new GenerateBundleCommands());
            buildTasks.Add(new GenerateSubAssetPathMaps());
            buildTasks.Add(new GenerateBundleMaps());
            buildTasks.Add(new PostPackingCallback());

            // Writing
            buildTasks.Add(new WriteSerializedFiles());
            buildTasks.Add(new ArchiveAndCompressBundles());
            buildTasks.Add(new AppendBundleHash());
            buildTasks.Add(new GenerateLinkXml());
            buildTasks.Add(new PostWritingCallback());

            return buildTasks;
        }
    }
}