using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using UnityEditor.Build.Pipeline;
using UnityEngine.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline.Injector;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [TaskAttribute("Step3. 构建资源包(SBP)")]
    public class TaskBuildAssetBundles : IGameBuildTask
    {
        void IGameBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();

            // 开始构建
            IBundleBuildResults buildResults;
            var buildParameters = GetSBPBuildParameters(buildParametersContext);
            ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParameters, 
                                                                    new BundleBuildContent(buildMapContext.GetPipelineBuilds()), 
                                                                    out buildResults,
                                                                    AssetBundleCompatible(false));      // TODO: 暂时不生成内置资源的资源包
            if (exitCode < 0)
            {
                throw new Exception($"构建过程中发生错误 : {exitCode}");
            }

            BuildRunner.Log("Unity引擎打包成功！");
            BuildResultContext buildResultContext = new BuildResultContext();
            buildResultContext.Results = buildResults;
            context.SetContextObject(buildResultContext);

            CopyRawBundle(buildMapContext, buildParametersContext);

            // 输出UnityManifest，调试用
            var manifest = ScriptableObject.CreateInstance<CompatibilityAssetBundleManifest>();
            manifest.SetResults(buildResults.BundleInfos);            
            string unityManifestPath = $"Assets/Temp/UnityManifest.asset";
            EditorTools.CreateFileDirectory(unityManifestPath);
            AssetDatabase.DeleteAsset(unityManifestPath);
            AssetDatabase.CreateAsset(manifest, unityManifestPath);
        }

        private void CopyRawBundle(BuildMapContext buildMapContext, BuildParametersContext buildParametersContext)
		{
			string cacheBundleOutput = buildParametersContext.GetCacheBundlesOutput();
			foreach (var bundleInfo in buildMapContext.BuildBundleInfos)
			{
				if (bundleInfo.IsRawFile)
				{
					string dest = $"{cacheBundleOutput}/{bundleInfo.BundleName}";
					foreach (var buildAsset in bundleInfo.BuildinAssets)
					{
						if (buildAsset.IsRawAsset)
							EditorTools.CopyFile(buildAsset.AssetPath, dest, true);
					}
				}
			}
		}

        private BundleBuildParameters GetSBPBuildParameters(BuildParametersContext buildParametersContext)
        {
            var buildParams = new BundleBuildParameters(buildParametersContext.gameBuilderSetting.buildTarget,
                                                        BuildPipeline.GetBuildTargetGroup(buildParametersContext.gameBuilderSetting.buildTarget),
                                                        buildParametersContext.GetCacheBundlesOutput());

            buildParams.BundleCompression = buildParametersContext.gameBuilderSetting.bundleSetting.useLZ4Compress ? UnityEngine.BuildCompression.LZ4 : UnityEngine.BuildCompression.Uncompressed;
            buildParams.UseCache = !buildParametersContext.gameBuilderSetting.bundleSetting.rebuildBundles;
            if (buildParametersContext.gameBuilderSetting.bundleSetting.disableWriteTypeTree)
                buildParams.ContentBuildFlags |= ContentBuildFlags.DisableWriteTypeTree;
            buildParams.AppendHash = buildParametersContext.gameBuilderSetting.bundleSetting.appendHash;
            return buildParams;
        }

        private IList<IBuildTask> AssetBundleCompatible(bool builtinTask)
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

    public class CreateBuiltInResourceBundle : IBuildTask
    {
        static readonly GUID k_BuiltInGuid = new GUID("0000000000000000f000000000000000");
        /// <inheritdoc />
        public int Version { get { return 1; } }

#pragma warning disable 649
        [InjectContext(ContextUsage.In)]
        IDependencyData m_DependencyData;

        [InjectContext(ContextUsage.InOut, true)]
        IBundleExplictObjectLayout m_Layout;
#pragma warning restore 649

        /// <summary>
        /// Stores the name for the built-in shaders bundle.
        /// </summary>
        public string BundleName { get; set; }
        public string ShaderBundleName { get; set; }

        /// <summary>
        /// Create the built-in shaders bundle.
        /// </summary>
        /// <param name="shaderBundleName">The name of the shader bundle</param>
        /// <param name="bundleName">The name of the other builtin resources bundle.</param>
        public CreateBuiltInResourceBundle(string shaderBundleName, string bundleName)
        {
            ShaderBundleName = shaderBundleName;
            BundleName = bundleName;
        }

        /// <inheritdoc />
        public ReturnCode Run()
        {
            HashSet<ObjectIdentifier> buildInObjects = new HashSet<ObjectIdentifier>();
            foreach (AssetLoadInfo dependencyInfo in m_DependencyData.AssetInfo.Values)
                buildInObjects.UnionWith(dependencyInfo.referencedObjects.Where(x => x.guid == k_BuiltInGuid));

            foreach (SceneDependencyInfo dependencyInfo in m_DependencyData.SceneInfo.Values)
                buildInObjects.UnionWith(dependencyInfo.referencedObjects.Where(x => x.guid == k_BuiltInGuid));

            ObjectIdentifier[] usedSet = buildInObjects.ToArray();
            Type[] usedTypes = ContentBuildInterface.GetTypeForObjects(usedSet);

            if (m_Layout == null)
                m_Layout = new BundleExplictObjectLayout();

            Type shader = typeof(Shader);
            for (int i = 0; i < usedTypes.Length; i++)
            {
                m_Layout.ExplicitObjectLocation.Add(usedSet[i], usedTypes[i] == shader ? ShaderBundleName : BundleName);
            }

            if (m_Layout.ExplicitObjectLocation.Count == 0)
                m_Layout = null;

            return ReturnCode.Success;
        }
    }
}