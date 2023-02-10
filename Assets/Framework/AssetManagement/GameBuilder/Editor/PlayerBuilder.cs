using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Framework.Core;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif


namespace Framework.AssetManagement.AssetEditorWindow
{
    public class PlayerBuilder
    {
        public delegate void onPreprocessPlayerBuild();
        public delegate void onPostprocessPlayerBuild();

        static public event onPreprocessPlayerBuild OnPreprocessPlayerBuild;
        static public event onPostprocessPlayerBuild OnPostprocessPlayerBuild;

        static public PlayerBuilderSetting m_Setting;

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
                if (UnityEngine.Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                return null;
            }

            m_Setting = para;

            // clear previous directory and create new one
            string outputPath = para.outputPath.TrimEnd(new char[] { '/' });
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);
            Directory.CreateDirectory(outputPath);
            Debug.Log($"        Player Output: {outputPath}");
            Debug.Log($"{para.ToString()}");

            OnPreprocessPlayerBuild?.Invoke();

            // 先更新版本号
            AppVersion version = para.SetAppVersion();

            // setup PlayerSettings
            para.SetupPlayerSettings(version);

            BuildPipeline.BuildPlayer(para.GenerateBuildPlayerOptions());

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

                if (UnityEngine.Application.isBatchMode)
                {
                    para.RestorePlayerSettings();
                    EditorApplication.Exit(1);
                }
            }
            para.RestorePlayerSettings();

            OnPostprocessPlayerBuild?.Invoke();

            if (!UnityEngine.Application.isBatchMode)
            {
                string appPath = UnityEngine.Application.dataPath.Replace("Assets", "") + para.outputPath.TrimEnd(new char[] { '/' });
                System.Diagnostics.Process.Start("explorer", appPath.Replace('/', '\\'));
            }

            return report;
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

        private class BuildProcessor : IPreprocessBuildWithReport
        {
            public int callbackOrder { get { return 10000000; } }     // 最后一步，在所有OnPreprocessBuild之后执行

            // 等所有需要打包的资源汇集到了streaming assets再执行
            public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
            {
                if(m_Setting.buildAppBundle)
                {
                    CopyStreamingAssetsToCustomPackage();
                }
            }
        }

        private static void CopyStreamingAssetsToCustomPackage()
        {
            string targetPath = @"Assets/CustomAssetPacks.androidpack";
            string streamingAssetPath = @"Assets/StreamingAssets";

            // step1. 重新创建CustomAssetPacks.androidpack
            if (System.IO.Directory.Exists(targetPath))
            {
                System.IO.Directory.Delete(targetPath, true);
            }
            System.IO.Directory.CreateDirectory(targetPath);

            // step2. 重新创建build.gradle
            string text = @"apply plugin: 'com.android.asset-pack'
assetPack {
    packName = ""CustomAssetPacks""
    dynamicDelivery {
        deliveryType = ""install-time""
    }
}";
            System.IO.File.WriteAllText($"{targetPath}/build.gradle", text);

            // step3. 把StreamingAssets移至CustomAssetPacks.androidpack
            Framework.Core.Editor.EditorUtility.CopyDirectory(streamingAssetPath, targetPath);
            System.IO.Directory.Delete(streamingAssetPath, true);

            AssetDatabase.Refresh();
        }

#if UNITY_IOS
            [PostProcessBuildAttribute(1)]
            public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
            {
                //为ios xcode工程增加编译选项-lz，-w
                if (target == BuildTarget.iOS)
                {
                    PBXProject project = new PBXProject();
                    string sPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
                    project.ReadFromFile(sPath);
                    var frameworkGUID = project.GetUnityFrameworkTargetGuid();
                    project.AddBuildProperty(frameworkGUID, "OTHER_LDFLAGS", "-lz");
                    project.AddBuildProperty(frameworkGUID, "OTHER_LDFLAGS", "-w");
                    File.WriteAllText(sPath, project.WriteToString());
                }
            }
#endif

    }
}