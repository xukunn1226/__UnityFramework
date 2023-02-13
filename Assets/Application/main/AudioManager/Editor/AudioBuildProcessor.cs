using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using Framework.Core;
using FMODUnity;
using Framework.AssetManagement.Runtime;

namespace Application.Editor
{
    public class AudioBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 50; } }     // 在FMOD资源导入StreamingAssets之后再执行，见FMOD.EventManager.CopyToStreamingAssets

        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            if(UnityEngine.Application.isBatchMode)
            { // batch mode模式下FMOD有bug，需要主动调用CopyToStreamingAssets
                FMODUnity.EventManager.RefreshBanks();
                FMODUnity.EventManager.CopyToStreamingAssets(EditorUserBuildSettings.activeBuildTarget);
            }

            // FMOD默认把资源复制到StreamingAssets/下，需要进一步复制到指定文件夹
            string srcFMOD = "Assets/StreamingAssets/" + FMODUnity.Settings.Instance.TargetSubFolder;
            if (System.IO.Directory.Exists(srcFMOD))
            {
                string newPath = "Assets/StreamingAssets/" + AssetManagerSettings.StreamingAssetsBuildinFolder + FMODUnity.Settings.Instance.TargetSubFolder;
                if (srcFMOD == newPath)
                    return;
                if (System.IO.Directory.Exists(newPath))
                {
                    AssetDatabase.DeleteAsset(newPath);
                    //System.IO.Directory.CreateDirectory(newPath);
                }
                AssetDatabase.MoveAsset(srcFMOD, newPath);
            }
        }
    }
}