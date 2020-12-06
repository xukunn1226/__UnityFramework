using System.Collections;
using System.Collections.Generic;
using System.IO;
using Framework.Core;
using UnityEngine;
using UnityEditor;

namespace Framework.AssetManagement.GameBuilder
{
    public class Deployment
    {
        static public string s_DefaultRootPath      = "Deployment";
        static public string s_LatestAppPath        = "Latest/player";                  // 最新app地址
        static public string s_LatestBundlesPath    = "Latest/assetbundles";            // 最新资源地址
        static public string s_BackupDirectoryPath  = "Backup";                         // 备份地址
        static public string s_Cdn_DataPath         = "Cdn/data";                       // 放置cdn最新资源地址
        static public string s_Cdn_ObbPath          = "Cdn/obb";                        // 所有obb地址
        static public string s_Cdn_PatchPath        = "Cdn/patch";                      // 所有平台补丁包地址

        // backup "app" and "assetbundles"
        static public void cmdDeploy()
        {
            // source path
            string srcRootPath = s_DefaultRootPath;
            CommandLineReader.GetCommand("SrcPath", ref srcRootPath);

            // destination path
            string dstRootPath = s_DefaultRootPath;
            CommandLineReader.GetCommand("DstPath", ref dstRootPath);

            // determine backup folder
            string appDirectory = AppVersion.EditorLoad().ToString3();
            CommandLineReader.GetCommand("AppDirectory", ref appDirectory);

            Run(srcRootPath, dstRootPath, appDirectory);
        }

        /// <summary>
        /// 部署版本（备份、生成补丁数据等）
        /// </summary>
        /// <param name="srcRootPath"></param>
        /// <param name="dstRootPath"></param>
        /// <param name="appDirectory"></param>
        static public void Run(string srcRootPath, string dstRootPath, string appDirectory)
        {
            // step1. 备份最新版本
            Backup(srcRootPath, dstRootPath, appDirectory);

            // step2. 发布app & obb
            PublishDataAndObb(srcRootPath, dstRootPath, appDirectory);

            // step3. patch generator
        }

        // srcPath: Deployment/Latest
        // dstPath: Deployment/Backup
        // appDirectory: 0.1.2.1
        static private void Backup(string srcRootPath, string dstRootPath, string appDirectory)
        {
            // source path
            string appSrcPath = string.Format($"{srcRootPath}/{s_LatestAppPath}/{Utility.GetPlatformName()}");
            string bundlesSrcPath = string.Format($"{srcRootPath}/{s_LatestBundlesPath}/{Utility.GetPlatformName()}");

            if(!Directory.Exists(appSrcPath))
            {
                if(Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                throw new System.Exception($"{appSrcPath} not found");
            }
            if(!Directory.Exists(bundlesSrcPath))
            {
                if(Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                throw new System.Exception($"{bundlesSrcPath} not found");
            }
            
            // destination path
            string appDstPath = string.Format($"{dstRootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{appDirectory}/app");
            string bundlesDstPath = string.Format($"{dstRootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{appDirectory}/assetbundles");

            try
            {
                if(Directory.Exists(appDstPath))
                {
                    Directory.Delete(appDstPath, true);
                }
                Directory.CreateDirectory(appDstPath);

                if (Directory.Exists(bundlesDstPath))
                {
                    Directory.Delete(bundlesDstPath, true);
                }
                Directory.CreateDirectory(bundlesDstPath);
            }
            catch(System.Exception e)
            {
                if(Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                Debug.LogError(e.Message);
            }

            Framework.Core.Editor.EditorUtility.CopyDirectory(appSrcPath, appDstPath);
            Framework.Core.Editor.EditorUtility.CopyDirectory(bundlesSrcPath, bundlesDstPath);
        }

        // srcPath: Deployment/Backup/windows/0.0.1/assetbundles
        // dstPath: Deployment/CDN/data/windows/
        // appDirectory: 0.1.2.1
        static private void PublishDataAndObb(string srcRootPath, string dstRootPath, string appDirectory)
        {
            string appSrcPath = string.Format($"{srcRootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{appDirectory}/assetbundles");
            string appDstPath = string.Format($"{dstRootPath}/{s_Cdn_DataPath}/{Utility.GetPlatformName()}");
            if (Directory.Exists(appDstPath))
            {
                Directory.Delete(appDstPath, true);
            }
            Framework.Core.Editor.EditorUtility.CopyDirectory(appSrcPath, appDstPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="dstPath"></param>
        /// <param name="appDirectory"></param>
        static private void BuildPatch(string srcPath, string dstPath, string appDirectory)
        {

        }
    }
}