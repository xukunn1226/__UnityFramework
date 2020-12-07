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
        static public string s_Cdn_DataPath         = "Cdn/data";                       // 放置最新版本的资源地址
        static public string s_Cdn_ObbPath          = "Cdn/obb";                        // 所有版本的obb地址
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
            BuildPatch(srcRootPath, dstRootPath, appDirectory);
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

            // clear directory
            string bakPath = string.Format($"{dstRootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{appDirectory}");
            if(Directory.Exists(bakPath))
                Directory.Delete(bakPath, true);

            // check destination path
            string appDstPath = string.Format($"{bakPath}/app");
            string bundlesDstPath = string.Format($"{bakPath}/assetbundles");
            try
            {
                Directory.CreateDirectory(appDstPath);
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

            // 生成file list，便于后续diff使用
            BundleFileList.BuildBundleFileList(bundlesDstPath,
                                               string.Format($"{bakPath}/{BundleExtracter.FILELIST_NAME}"));
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
            Directory.CreateDirectory(appDstPath);
            Framework.Core.Editor.EditorUtility.CopyDirectory(appSrcPath, appDstPath);
        }

        /// <summary>
        /// 根据backdoor.json生成所有历史版本与当前版本的差异数据（diff.json）
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="dstPath"></param>
        /// <param name="appDirectory"></param>
        static private void BuildPatch(string srcPath, string dstPath, string appDirectory)
        {

        }

        /// <summary>
        /// 计算两个版本的差异
        /// e.g     baseApp: 0.0.1  curApp: 0.0.1.1
        /// </summary>
        /// <param name="baseApp"></param>
        /// <param name="curApp"></param>
        /// <returns></returns>
        static private Diff Diff(string baseApp, string curApp)
        {
            string baseAppPath = string.Format($"{s_DefaultRootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{baseApp}/assetbundles");
            string curAppPath = string.Format($"{s_DefaultRootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{curApp}/assetbundles");
            if(!Directory.Exists(baseAppPath))
            {
                Debug.LogError($"{baseAppPath} is not exists");
                return null;
            }
            if(!Directory.Exists(curAppPath))
            {
                Debug.LogError($"{curAppPath} is not exists");
                return null;
            }

            Diff data = new Diff();
            data.Desc = string.Format($"{baseApp}-{curApp}");

            return null;
        }
    }
}