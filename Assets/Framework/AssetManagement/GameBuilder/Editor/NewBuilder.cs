using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Content;
using UnityEditor.Build.Player;
using UnityEngine.Build.Pipeline;
using System.IO;

public class NewBuilder
{
    public static bool BuildAssetBundles(string outputPath, bool useChunkBasedCompression, BuildTarget buildTarget, BuildTargetGroup buildGroup)
    {
        var buildContent = new BundleBuildContent(ContentBuildInterface.GenerateAssetBundleBuilds());
        var buildParams = new BundleBuildParameters(buildTarget, buildGroup, outputPath);
        // Set build parameters for connecting to the Cache Server
        buildParams.UseCache = true;
        buildParams.CacheServerHost = "buildcache.unitygames.com";
        buildParams.CacheServerPort = 8126;

        if (useChunkBasedCompression)
            buildParams.BundleCompression = UnityEngine.BuildCompression.LZ4;

        IBundleBuildResults results;
        ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParams, buildContent, out results);
        return exitCode == ReturnCode.Success;
    }






    class CustomBuildParameters : BundleBuildParameters
    {
        public Dictionary<string, UnityEngine.BuildCompression> PerBundleCompression { get; set; }

        public CustomBuildParameters(BuildTarget target, BuildTargetGroup group, string outputFolder) : base(target, group, outputFolder)
        {
            PerBundleCompression = new Dictionary<string, UnityEngine.BuildCompression>();
        }

        // Override the GetCompressionForIdentifier method with new logic
        public override UnityEngine.BuildCompression GetCompressionForIdentifier(string identifier)
        {
            UnityEngine.BuildCompression value;
            if (PerBundleCompression.TryGetValue(identifier, out value))
                return value;
            return BundleCompression;
        }
    }
    public static bool BuildAssetBundles_2(string outputPath, bool useChunkBasedCompression, BuildTarget buildTarget, BuildTargetGroup buildGroup)
    {
        AssetBundleBuild[] BuildList = ContentBuildInterface.GenerateAssetBundleBuilds();
        for(int i = 0; i < BuildList.Length; ++i)
        {
            AssetBundleBuild abb = BuildList[i];
            abb.addressableNames = new string[abb.assetNames.Length];
            for(int j = 0; j < abb.assetNames.Length; ++j)
                abb.addressableNames[j] = System.IO.Path.GetFileName(abb.assetNames[j]);
            BuildList[i] = abb;
        }
        var buildContent = new BundleBuildContent(BuildList);
        // Construct the new parameters class
        var buildParams = new CustomBuildParameters(buildTarget, buildGroup, outputPath);
        buildParams.BundleCompression = UnityEngine.BuildCompression.LZ4;
        buildParams.ContentBuildFlags = ContentBuildFlags.DisableWriteTypeTree;
        buildParams.ScriptOptions = ScriptCompilationOptions.DevelopmentBuild;
        // Populate the bundle specific compression data
        buildParams.PerBundleCompression.Add("Bundle1", UnityEngine.BuildCompression.Uncompressed);
        buildParams.PerBundleCompression.Add("Bundle2", UnityEngine.BuildCompression.LZMA);
        buildParams.OutputFolder = "Deployment/Ex";

        // if (m_Settings.compressionType == CompressionType.None)
        //     buildParams.BundleCompression = BuildCompression.DefaultUncompressed;
        // else if (m_Settings.compressionType == CompressionType.Lzma)
        //     buildParams.BundleCompression = BuildCompression.DefaultLZMA;
        // else if (m_Settings.compressionType == CompressionType.Lz4 || m_Settings.compressionType == CompressionType.Lz4HC)
        //     buildParams.BundleCompression = BuildCompression.DefaultLZ4;

        IBundleBuildResults results;
        ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParams, buildContent, out results);
        if(exitCode < ReturnCode.Success)
            return false;

        var manifest = ScriptableObject.CreateInstance<CompatibilityAssetBundleManifest>();
            manifest.SetResults(results.BundleInfos);
            File.WriteAllText(buildParams.GetOutputFilePathForIdentifier(Path.GetFileName(outputPath) + ".manifest"), manifest.ToString());
            
        return exitCode == ReturnCode.Success;
    }

    [MenuItem("Tools/Build AB")]
    static void BuildAB()
    {
        BuildAssetBundles_2("Deployment", true, BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone);
    }

    [MenuItem("Tools/Display GUID")]
    static void DisplayGUID()
    {
        Debug.Log($"{AssetDatabase.GUIDToAssetPath("25d5b8ce0e9654646ae1ff96222c7aaa")}");
        Debug.Log($"{AssetDatabase.GUIDToAssetPath("ea51d4abbd79dbe4684c910b4b11cfe5")}");
        Debug.Log($"{AssetDatabase.GUIDToAssetPath("fd76295b7725e284cb11b334d456f0a1")}");
        Debug.Log($"{AssetDatabase.GUIDToAssetPath("f14436493e490444fbf64c9f5569d86f")}");
        Debug.Log($"{AssetDatabase.GUIDToAssetPath("e28b2d65b12b59c4b8bec50caa9a92f6")}");
    }





    public static bool BuildAssetBundles_3(string outputPath, bool forceRebuild, bool useChunkBasedCompression, BuildTarget buildTarget)
    {
        var options = BuildAssetBundleOptions.None;
        if (useChunkBasedCompression)
            options |= BuildAssetBundleOptions.ChunkBasedCompression;

        if (forceRebuild)
            options |= BuildAssetBundleOptions.ForceRebuildAssetBundle;

        // Get the set of bundle to build
        var bundles = ContentBuildInterface.GenerateAssetBundleBuilds();
        // Update the addressableNames to load by the file name without extension
        for (var i = 0; i < bundles.Length; i++)
            bundles[i].addressableNames = bundles[i].assetNames.Select(System.IO.Path.GetFileNameWithoutExtension).ToArray();

        var manifest = CompatibilityBuildPipeline.BuildAssetBundles("Deployment/", bundles, options, BuildTarget.StandaloneWindows64);
        return manifest != null;
    }
}
