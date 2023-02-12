using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [TaskAttribute("Step2. 获取资源构建内容")]
    public class TaskBuildBundleMap : IGameBuildTask
    {
        void IGameBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = BuildMapCreator.CreateBuildMap(buildParametersContext.gameBuilderSetting.bundleSetting.bundleCollectorConfigName);
            context.SetContextObject(buildMapContext);
            BuildRunner.Log("构建内容准备完毕！");
        }
    }
}