using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using Framework.Core;

namespace Application.Editor
{
    public class AudioBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 50; } }     // 在FMOD资源导入StreamingAssets之后再执行，见FMOD.EventManager.CopyToStreamingAssets

        // 等所有需要打包的资源汇集到了streaming assets再执行
        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            Debug.Log("AudioBuildProcessor: 11111111");
            // FMOD默认把资源复制到StreamingAssets/下，需要进一步复制到指定文件夹
            string srcFMOD = "Assets/StreamingAssets/" + FMODUnity.Settings.Instance.TargetSubFolder;
            if (System.IO.Directory.Exists(srcFMOD))
            {
                string newPath = "Assets/StreamingAssets/" + Utility.GetPlatformName() + "/" + FMODUnity.Settings.Instance.TargetSubFolder;
                if (System.IO.Directory.Exists(newPath))
                {
                    AssetDatabase.DeleteAsset(newPath);
                    //System.IO.Directory.CreateDirectory(newPath);
                }
                AssetDatabase.MoveAsset(srcFMOD, newPath);
                Debug.Log("AudioBuildProcessor: 2222222222");
            }
        }
    }
}