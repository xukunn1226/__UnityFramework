using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.AssetManagement.Runtime;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [TaskAttribute("Step8. 资源包拷贝到StreamingAssets")]
    public class TaskCopyToStreamingAssets : IGameBuildTask
    {
        void IGameBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            string cacheStreamingOutput = buildParametersContext.GetCacheStreamingOutput();
            string streamingAssetsDirectory = AssetBundleBuilderHelper.GetStreamingAssetsFolderPath();

            AssetBundleBuilderHelper.ClearStreamingAssetsFolder();

            EditorTools.CopyDirectory(cacheStreamingOutput, streamingAssetsDirectory);

            AssetDatabase.Refresh();
            BuildRunner.Log($"文件拷贝完成：{streamingAssetsDirectory}");
        }
    }
}