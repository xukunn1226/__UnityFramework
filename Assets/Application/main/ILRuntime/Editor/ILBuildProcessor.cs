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
            Debug.Log("CopyLogicDLLToStreamingAssets======");
            // 启动游戏会重新编译脚本，dll将编译并复制至streamingAsset，这在FromPersistent模式下会导致打版本时的dll’s hash与streamingAsset下的dll’hash不一致，从而下载失败
            // 故禁用
            if(Launcher.GetLauncherMode() == Framework.AssetManagement.Runtime.LoaderType.FromPersistent)
            {
                return;
            }

            AssembyBuilder.BuildAssembly(true, true);

            // string srcPath = string.Format($"{UnityEngine.Application.dataPath}/../Library/ScriptAssemblies");
            // string dstPath = string.Format($"{UnityEngine.Application.streamingAssetsPath}/{Utility.GetPlatformName()}");

            // if(!System.IO.Directory.Exists(dstPath))
            //     System.IO.Directory.CreateDirectory(dstPath);
            // System.IO.File.Copy(string.Format($"{srcPath}/{ILStartup.dllFilename}.dll"), string.Format($"{dstPath}/{ILStartup.dllFilename}.dll"), true);
            // System.IO.File.Copy(string.Format($"{srcPath}/{ILStartup.dllFilename}.pdb"), string.Format($"{dstPath}/{ILStartup.dllFilename}.pdb"), true);
        }
    }
}