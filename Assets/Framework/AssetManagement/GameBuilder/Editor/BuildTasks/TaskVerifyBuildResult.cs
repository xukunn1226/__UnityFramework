using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [TaskAttribute("验证构建结果")]
    public class TaskVerifyBuildResult : IGameBuildTask
    {
        void IGameBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var buildResults = context.GetContextObject<BuildResultContext>().Results;

            List<string> buildedBundles = buildResults.BundleInfos.Keys.ToList();

            // 过滤掉原生Bundle
            List<string> expectBundles = buildMapContext.BuildBundleInfos.Where(t => t.IsRawFile == false).Select(t => t.BundleName).ToList();

            // 验证Bundle
            List<string> exceptBundleList1 = buildedBundles.Except(expectBundles).ToList();
            if (exceptBundleList1.Count > 0)
            {
                foreach (var exceptBundle in exceptBundleList1)
                {
                    Debug.LogWarning($"差异资源包: {exceptBundle}");
                }
                throw new System.Exception("存在差异资源包！请查看警告信息！");
            }

            // 验证Bundle
            List<string> exceptBundleList2 = expectBundles.Except(buildedBundles).ToList();
            if (exceptBundleList2.Count > 0)
            {
                foreach (var exceptBundle in exceptBundleList2)
                {
                    Debug.LogWarning($"差异资源包: {exceptBundle}");
                }
                throw new System.Exception("存在差异资源包！请查看警告信息！");
            }

            BuildRunner.Log("构建结果验证成功！");
        }
    }
}