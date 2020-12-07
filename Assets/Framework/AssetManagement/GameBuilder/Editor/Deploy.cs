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
        static public string s_LatestAppPath        = "Latest/player";                  // ����app��ַ
        static public string s_LatestBundlesPath    = "Latest/assetbundles";            // ������Դ��ַ
        static public string s_BackupDirectoryPath  = "Backup";                         // ���ݵ�ַ
        static public string s_Cdn_DataPath         = "Cdn/data";                       // �������°汾����Դ��ַ
        static public string s_Cdn_ObbPath          = "Cdn/obb";                        // ���а汾��obb��ַ
        static public string s_Cdn_PatchPath        = "Cdn/patch";                      // ����ƽ̨��������ַ

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
        /// ����汾�����ݡ����ɲ������ݵȣ�
        /// </summary>
        /// <param name="srcRootPath"></param>
        /// <param name="dstRootPath"></param>
        /// <param name="appDirectory"></param>
        static public void Run(string srcRootPath, string dstRootPath, string appDirectory)
        {
            // step1. �������°汾
            Backup(srcRootPath, dstRootPath, appDirectory);

            // step2. ����app & obb
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

            // ����file list�����ں���diffʹ��
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
        /// ����backdoor.json����������ʷ�汾�뵱ǰ�汾�Ĳ������ݣ�diff.json��
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="dstPath"></param>
        /// <param name="appDirectory"></param>
        static private void BuildPatch(string srcPath, string dstPath, string appDirectory)
        {

        }

        /// <summary>
        /// ���������汾�Ĳ���
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