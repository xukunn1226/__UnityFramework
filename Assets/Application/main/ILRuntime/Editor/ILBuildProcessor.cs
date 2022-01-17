using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build;
using Framework.Core;

namespace Application.Runtime
{
    public class ILBuildProcessor : MonoBehaviour
    {
        private class BuildProcessor : IPreprocessBuildWithReport
        {
            public int callbackOrder { get { return 50; } }

            public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
            {
                ILRuntimeCLRBinding.GenerateCLRBindingByAnalysis();
                CopyLogicDLLToStreamingAssets();
            }
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void CopyLogicDLLToStreamingAssets()
        {
            // FromPersistent模式下禁用，因为会导致dll’s hash与FileList不一致，从而下载失败
            if(Launcher.GetLauncherMode() == Framework.AssetManagement.Runtime.LoaderType.FromPersistent)
            {
                return;
            }

            string srcPath = string.Format($"{UnityEngine.Application.dataPath}/../Library/ScriptAssemblies");
            string dstPath = string.Format($"{UnityEngine.Application.streamingAssetsPath}/{Utility.GetPlatformName()}");

            if(!System.IO.Directory.Exists(dstPath))
                System.IO.Directory.CreateDirectory(dstPath);
            System.IO.File.Copy(string.Format($"{srcPath}/{ILStartup.dllFilename}.dll"), string.Format($"{dstPath}/{ILStartup.dllFilename}.dll"), true);
            System.IO.File.Copy(string.Format($"{srcPath}/{ILStartup.dllFilename}.pdb"), string.Format($"{dstPath}/{ILStartup.dllFilename}.pdb"), true);
        }
    }
}