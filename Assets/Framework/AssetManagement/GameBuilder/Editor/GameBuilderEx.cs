using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.AssetEditorWindow
{
    public class GameBuilderEx
    {
        static private readonly BuildContext m_BuildContext = new BuildContext();

        static GameBuildResult Run(GameBuilderSetting buildParameters)
        {
            if (buildParameters == null)
                throw new System.Exception($"{nameof(GameBuilderSetting)} is null");
            if (buildParameters.bundleSetting == null)
                throw new System.Exception($"{nameof(BundleBuilderSetting)} is null");
            if (buildParameters.playerSetting == null)
                throw new System.Exception($"{nameof(PlayerBuilderSetting)} is null");

            // 传递构建参数
            var buildParametersContext = new BuildParametersContext(buildParameters);
            m_BuildContext.SetContextObject(buildParametersContext);

            // 创建构建任务

            return null;
        }
    }
}