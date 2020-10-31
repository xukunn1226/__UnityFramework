using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

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
            string appSrcPath = string.Format($"{srcPath}/player/{Utility.GetPlatformName()}");
            string bundlesSrcPath = string.Format($"{srcPath}/assetbundles/{Utility.GetPlatformName()}");

            // destination path
            string dstPath = s_DefaultDestPath;
            CommandLineReader.GetCommand("DstPath", ref dstPath);

            // determine backup folder
            string dstDirectory = "";
            CommandLineReader.GetCommand("DstDirectory", ref dstDirectory);

            string appDstPath = string.Format($"{dstPath}/{Utility.GetPlatformName()}/{dstDirectory}/app");
            string bundlesDstPath = string.Format($"{dstPath}/{Utility.GetPlatformName()}/{dstDirectory}/assetbundles");
        }
    }
}