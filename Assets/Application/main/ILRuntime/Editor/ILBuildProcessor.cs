using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build;

namespace Application.Runtime
{
    public class ILBuildProcessor : MonoBehaviour
    {
        private class BuildProcessor : IPreprocessBuildWithReport
        {
            public int callbackOrder { get { return 50; } }

            public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
            {
                //if(PlayerBuilder.m_Setting.releaseNative)
                //    return;     // 发布原生版本跳过IL相关设置及编译
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
            //if(Launcher.GetLauncherMode() == Framework.AssetManagement.Runtime.LoaderType.FromPersistent)
            //{
            //    return;
            //}


            AssembyBuilder.BuildAssembly(true, true);
        }
    }
}