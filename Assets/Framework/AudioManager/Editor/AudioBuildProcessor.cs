using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

namespace Framework.Core.Editor
{
    public class AudioBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 50; } }     // ��FMOD��Դ����StreamingAssets֮����ִ�У���FMOD.EventManager.CopyToStreamingAssets

        // ��������Ҫ�������Դ�㼯����streaming assets��ִ��
        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            // FMODĬ�ϰ���Դ���Ƶ�StreamingAssets/�£���Ҫ��һ�����Ƶ�ָ���ļ���
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
            }
        }
    }
}