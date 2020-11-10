using System.Collections;
using System.Collections.Generic;
using System.IO;
using Framework.Core;
using UnityEngine;
using UnityEditor;

namespace Framework.AssetManagement.Deployment
{
    public class Deployment
    {
        static private string s_DefaultSourcePath = "Deployment/Latest";
        static private string s_DefaultDestPath = "Deployment/Backup";

        // backup "app" and "assetbundles"
        static public void cmdBackup()
        {
            // source path
            string srcPath = s_DefaultSourcePath;
            CommandLineReader.GetCommand("SrcPath", ref srcPath);

            // destination path
            string dstPath = s_DefaultDestPath;
            CommandLineReader.GetCommand("DstPath", ref dstPath);

            // determine backup folder
            string appDirectory = AppVersion.EditorLoad().ToString();
            CommandLineReader.GetCommand("AppDirectory", ref appDirectory);

            Backup(srcPath, dstPath, appDirectory);
        }

        // srcPath: Deployment/Latest
        // dstPath: Deployment/Backup
        // appDirectory: 0.1.2.1
        static public void Backup(string srcPath, string dstPath, string appDirectory)
        {
            // source path
            string appSrcPath = string.Format($"{srcPath}/player/{Utility.GetPlatformName()}");
            string bundlesSrcPath = string.Format($"{srcPath}/assetbundles/{Utility.GetPlatformName()}");

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
            string appDstPath = string.Format($"{dstPath}/{Utility.GetPlatformName()}/{appDirectory}/app");
            string bundlesDstPath = string.Format($"{dstPath}/{Utility.GetPlatformName()}/{appDirectory}/assetbundles");

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
    }
}