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
            string srcPath = string.Format($"{UnityEngine.Application.dataPath}/../Library/ScriptAssemblies");
            string dstPath = string.Format($"{UnityEngine.Application.streamingAssetsPath}/{Utility.GetPlatformName()}");

            System.IO.File.Copy(string.Format($"{srcPath}/{ILStartup.dllFilename}.dll"), string.Format($"{dstPath}/{ILStartup.dllFilename}.dll"), true);
            System.IO.File.Copy(string.Format($"{srcPath}/{ILStartup.dllFilename}.pdb"), string.Format($"{dstPath}/{ILStartup.dllFilename}.pdb"), true);
        }
    }
}