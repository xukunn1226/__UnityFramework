using System.Collections;
using System.Collections.Generic;
using Framework.AssetManagement.AssetEditorWindow;
using System.IO;
using UnityEditor;
using UnityEngine;
using Framework.AssetManagement.Runtime;

namespace Framework.AssetManagement.AssetEditorWindow
{
    public static class AssetBundleBuilderHelper
    {
        /// <summary>
        /// 获取默认的输出根目录
        /// </summary>
        public static string GetDefaultOutputRoot()
        {
            string projectPath = EditorTools.GetProjectPath();
            return $"{projectPath}/Deployment";
        }

        /// <summary>
        /// 获取默认资源包输出目录
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultBundlesOutputRoot()
        {
            return $"{GetDefaultOutputRoot()}/Cache/Bundles";
        }

        /// <summary>
        /// 根据包名、平台等获取最终资源包的输出目录
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public static string GetCacheBundlesOutput(BuildTarget buildTarget)
        {
            return $"{GetDefaultBundlesOutputRoot()}/{buildTarget}";
        }

        /// <summary>
        /// 获取默认APP输出目录
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultPlayerOutputRoot()
        {
            return $"{GetDefaultOutputRoot()}/Cache/Player";
        }

        /// <summary>
        /// 根据包名、平台等获取最终APP的输出目录
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public static string GetCachePlayerOutput(BuildTarget buildTarget)
        {
            return $"{GetDefaultPlayerOutputRoot()}/{buildTarget}";
        }

        /// <summary>
        /// 获取流文件夹路径
        /// </summary>
        public static string GetStreamingAssetsFolderPath()
        {
            return $"{UnityEngine.Application.dataPath}/StreamingAssets/{AssetManagerSettings.StreamingAssetsBuildinFolder}/";
        }

        /// <summary>
        /// 清空流文件夹
        /// </summary>
        public static void ClearStreamingAssetsFolder()
        {
            string streamingFolderPath = GetStreamingAssetsFolderPath();
            EditorTools.ClearFolder(streamingFolderPath);
        }

        /// <summary>
        /// 删除流文件夹内无关的文件
        /// 删除.manifest文件和.meta文件
        /// </summary>
        public static void DeleteStreamingAssetsIgnoreFiles()
        {
            string streamingFolderPath = GetStreamingAssetsFolderPath();
            if (Directory.Exists(streamingFolderPath))
            {
                string[] files = Directory.GetFiles(streamingFolderPath, "*.manifest", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    FileInfo info = new FileInfo(file);
                    info.Delete();
                }

                files = Directory.GetFiles(streamingFolderPath, "*.meta", SearchOption.AllDirectories);
                foreach (var item in files)
                {
                    FileInfo info = new FileInfo(item);
                    info.Delete();
                }
            }
        }

        /// <summary>
        /// 获取构建管线的输出目录
        /// </summary>
        public static string MakePipelineOutputDirectory(string outputRoot, string buildPackage, BuildTarget buildTarget)
        {
            string outputDirectory = $"{outputRoot}/{buildPackage}/{buildTarget}/{AssetManagerSettings.OutputFolderName}";
            return outputDirectory;
        }
    }
}