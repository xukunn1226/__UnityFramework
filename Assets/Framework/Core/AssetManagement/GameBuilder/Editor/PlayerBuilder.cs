using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using Framework.Core;
using Google.Android.AppBundle.Editor;
using Google.Android.AppBundle.Editor.Internal;

namespace Framework.AssetManagement.GameBuilder
{
    public class PlayerBuilder
    {
        public delegate void onPreprocessPlayerBuild();
        public delegate void onPostprocessPlayerBuild();

        static public event onPreprocessPlayerBuild OnPreprocessPlayerBuild;
        static public event onPostprocessPlayerBuild OnPostprocessPlayerBuild;

        /// <summary>
        /// 构建Player接口（唯一）
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        static public BuildReport BuildPlayer(PlayerBuilderSetting para)
        {
            if (para == null)
            {
                Debug.LogError($"PlayerBuilderSetting para == null");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                return null;
            }

            // clear previous directory and create new one
            string outputPath = para.outputPath.TrimEnd(new char[] { '/' }) + "/" + Utility.GetPlatformName();
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);
            Directory.CreateDirectory(outputPath);
            Debug.Log($"        Player Output: {outputPath}");

            OnPreprocessPlayerBuild?.Invoke();

            // 先更新版本号
            AppVersion version = para.SetAppVersion();

            // setup PlayerSettings
            para.SetupPlayerSettings(version);

            InternalBuildPlayer(para);

            BuildReport report = OpenLastBuild();
            if(report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Begin Build Player");

                Debug.Log($"        Output: {report.summary.outputPath}");

                Debug.Log($"        Opt: {report.summary.options}");

                Debug.Log($"        PlayerSettings: {para.ToString()}");

                Debug.Log($"End Build Player: Succeeded     totalSize:{EditorUtility.FormatBytes((long)report.summary.totalSize)}     totalWarnings:{report.summary.totalWarnings}     totalErrors:{report.summary.totalErrors}");
            }
            else
            {
                Debug.Log("Begin Build Player");

                Debug.Log($"        Output: {report.summary.outputPath}");

                Debug.Log($"        Opt: {report.summary.options}");

                Debug.Log($"        PlayerSettings: {para.ToString()}");

                Debug.LogError($"End Build Player: Failed");

                if (Application.isBatchMode)
                {
                    para.RestorePlayerSettings();
                    EditorApplication.Exit(1);
                }
            }
            para.RestorePlayerSettings();

            OnPostprocessPlayerBuild?.Invoke();

            if (!Application.isBatchMode)
            {
                string appPath = Application.dataPath.Replace("Assets", "") + para.outputPath.TrimEnd(new char[] { '/' });
                System.Diagnostics.Process.Start("explorer", appPath.Replace('/', '\\'));
            }

            return report;
        }

        static private void InternalBuildPlayer(PlayerBuilderSetting para)
        {
            BuildPlayerOptions opt = para.GenerateBuildPlayerOptions();
            if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android && para.buildAppBundle)
            {
                AssetPackConfig config = new AssetPackConfig();
                config.SplitBaseModuleAssets = true;
                AppBundlePublisher.Build(opt, config, true);
            }
            else
            {
                BuildPipeline.BuildPlayer(opt);
            }
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
    }
}