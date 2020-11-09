using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using Framework.Core;

namespace Framework.AssetManagement.GameBuilder
{
    public class PlayerBuilder
    {
        static private string FILELIST_PATH = "Assets/Resources/FileList.bytes";

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

            // 先更新版本号
            AppVersion version = para.SetAppVersion();

            // 计算StreamingAssets下所有资源的MD5，存储于Assets/Resources
            

            // setup PlayerSettings
            para.SetupPlayerSettings(version);

            BuildReport report = BuildPipeline.BuildPlayer(para.GenerateBuildPlayerOptions());
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

            if (!Application.isBatchMode)
            {
                string appPath = Application.dataPath.Replace("Assets", "") + outputPath;
                System.Diagnostics.Process.Start("explorer", appPath.Replace('/', '\\'));
            }

            return report;
        }

        [MenuItem("Tests/Build FileList")]
        static private void BuildFileList()
        {
            string directory = Application.streamingAssetsPath + "/" + Utility.GetPlatformName();
            if(!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"{directory}");
            }

            var dir = new DirectoryInfo(directory);
            FileInfo[] fis = dir.GetFiles("*", SearchOption.AllDirectories);
            foreach(var fi in fis)
            {
                if(!string.IsNullOrEmpty(fi.Extension) && string.Compare(fi.Extension, ".meta", true) == 0)
                {
                    Debug.LogWarning(fi.Name);
                    continue;
                }
                Debug.Log(fi.FullName.Substring(Application.streamingAssetsPath.Length + 1));
            }
        }

        static private string GetHash(FileInfo fi)
        {
            string hash = null;
            try
            {
                FileStream fs = fi.Open(FileMode.Open);
                fs.Position = 0;
                hash = EasyMD5.Hash(fs);
                fs.Close();
            }
            catch (IOException e)
            {
                Debug.Log($"I/O Exception: {e.Message}");
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.Log($"Access Exception: {e.Message}");
            }
            return hash;
        }
    }
}