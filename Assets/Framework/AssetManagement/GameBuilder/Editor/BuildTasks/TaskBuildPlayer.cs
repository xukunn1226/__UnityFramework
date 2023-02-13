using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Build.Reporting;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [TaskAttribute("Step9. 构建Player")]
    public class TaskBuildPlayer : IGameBuildTask
    {
        void IGameBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            BuildPipeline.BuildPlayer(GenerateBuildPlayerOptions(buildParametersContext.GetCachePlayerOutput(), buildParametersContext.gameBuilderSetting.playerSetting));
            BuildReport buildReport = OpenLastBuild();
            if (buildReport.summary.result != BuildResult.Succeeded)
            {
                throw new System.Exception($"构建Player过程中发生异常，请查看BuildReport");
            }
            BuildRunner.Log("引擎构建Player成功！");
        }

        private static BuildReport OpenLastBuild()
        {
            const string buildReportDir = "Assets/BuildReports";
            if (!Directory.Exists(buildReportDir))
                Directory.CreateDirectory(buildReportDir);

            var date = File.GetLastWriteTime("Library/LastBuild.buildreport");
            var assetPath = buildReportDir + "/Build_" + date.ToString("yyyy-dd-MMM-HH-mm-ss") + ".buildreport";
            File.Copy("Library/LastBuild.buildreport", assetPath, true);
            AssetDatabase.ImportAsset(assetPath);
            BuildReport report = AssetDatabase.LoadAssetAtPath<BuildReport>(assetPath);
            Selection.objects = new UnityEngine.Object[] { report };
            return report;
        }

        private BuildPlayerOptions GenerateBuildPlayerOptions(string output, PlayerBuilderSetting para)
        {
            BuildPlayerOptions opt = new BuildPlayerOptions();
            opt.locationPathName = GetLocalPathName(output, para);
            opt.scenes = GetBuildScenes(para);
            opt.target = EditorUserBuildSettings.activeBuildTarget;
            opt.targetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            opt.options = GenerateBuildOptions(para);
            return opt;
        }

        private string GetLocalPathName(string output, PlayerBuilderSetting para)
        {
            string extension = string.Empty;
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows64:
                    extension = "_64.exe";
                    break;
                case BuildTarget.StandaloneWindows:
                    extension = ".exe";
                    break;
                case BuildTarget.Android:
                    extension = para.buildAppBundle ? ".aab" : ".apk";
                    break;
                case BuildTarget.iOS:
                    extension = ".ipa";
                    break;
            }

            return string.Format("{0}/{1}{2}", output, para.projectName, extension);
        }

        private string[] GetBuildScenes(PlayerBuilderSetting para)
        {
            List<string> names = new List<string>();
            if (!para.development)
            { // NOTE: 发布release版本时仅发布Build Settings中第一个激活的场景
                foreach (var scene in EditorBuildSettings.scenes)
                {
                    if (scene == null || !scene.enabled)
                        continue;

                    if (AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(scene.guid.ToString())) == null)
                        continue;

                    names.Add(scene.path);
                    break;
                }
            }            
            else
            {
                foreach (var scene in EditorBuildSettings.scenes)
                {
                    if (scene == null || !scene.enabled)
                        continue;

                    if (AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(scene.guid.ToString())) == null)
                        continue;

                    names.Add(scene.path);
                }
            }
            return names.ToArray();
        }

        private BuildOptions GenerateBuildOptions(PlayerBuilderSetting para)
        {
            BuildOptions opt = BuildOptions.None;

            if (para.development)
                opt |= BuildOptions.Development;
            else
                opt &= ~BuildOptions.Development;

            if (para.compressWithLz4HC)
            {
                opt |= BuildOptions.CompressWithLz4HC;
                opt &= ~BuildOptions.CompressWithLz4;
            }
            else
                opt &= ~BuildOptions.CompressWithLz4HC;

            if (para.compressWithLz4 && !para.compressWithLz4HC)
                opt |= BuildOptions.CompressWithLz4;
            else
                opt &= ~BuildOptions.CompressWithLz4;

            if (para.strictMode)
                opt |= BuildOptions.StrictMode;
            else
                opt &= ~BuildOptions.StrictMode;

            return opt;
        }
    }
}