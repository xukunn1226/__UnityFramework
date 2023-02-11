using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.AssetEditorWindow
{
    public class GameBuilderEx
    {
        static private readonly BuildContext m_BuildContext = new BuildContext();

        static public GameBuildResult Run(GameBuilderSetting buildParameters)
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
            List<IGameBuildTask> tasks = new List<IGameBuildTask>
            {
                new TaskPrepare(),
            };
            switch(buildParameters.buildMode)
            {
                case GameBuilderSetting.BuildMode.Bundles:
                    SetBuildBundleTasks(tasks);
                    break;
                case GameBuilderSetting.BuildMode.Player:
                    SetBuildPlayerTasks(tasks);
                    break;
                case GameBuilderSetting.BuildMode.BundlesAndPlayer:
                    SetBuildBundleTasks(tasks);
                    SetBuildPlayerTasks(tasks);
                    break;
                default:
                    throw new System.Exception($"should never get here");
            }

            // 执行构建任务
            var buildResult = BuildRunner.Run(tasks, m_BuildContext);
            if (buildResult.Success)
            {
                //buildResult.OutputPackageDirectory = buildParametersContext.GetPackageOutputDirectory();
                Debug.Log($"{buildParameters.buildMode} pipeline build succeed !");
            }
            else
            {
                Debug.LogWarning($"{buildParameters.buildMode} pipeline build failed !");
                Debug.LogError($"Build task failed : {buildResult.FailedTask}");
                Debug.LogError($"Build task error : {buildResult.FailedInfo}");
            }
            return buildResult;
        }

        /// <summary>
        /// 设置构建资源包的任务列表
        /// </summary>
        /// <param name="tasks"></param>
        static private void SetBuildBundleTasks(List<IGameBuildTask> tasks)
        {
            List<IGameBuildTask> bundleTasks = new List<IGameBuildTask>
            {
                new TaskBuildBundleMap(),           // 准备构建的内容，分析资源的依赖关系
                new TaskBuildAssetBundles(),        // 执行构建
                new TaskVerifyBuildResult(),        // 验证打包结果
                new TaskUpdateBuildInfo(),          // 根据打包结果更新数据
                new TaskCreateManifest(),           // 创建清单
            };
            tasks.AddRange(bundleTasks);
        }

        /// <summary>
        /// 设置构建Player的任务列表
        /// </summary>
        /// <param name="tasks"></param>
        static private void SetBuildPlayerTasks(List<IGameBuildTask> tasks)
        {
            List<IGameBuildTask> playerTasks = new List<IGameBuildTask>
            {

            };
            tasks.AddRange(playerTasks);
        }
    }
}