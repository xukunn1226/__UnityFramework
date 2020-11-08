using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using Framework.Core;

namespace Framework.AssetManagement.GameBuilder
{
    public class PlayerBuilder
    {
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




        static private string GetHash(HashAlgorithm hashAlgorithm, byte[] data)
        {
            byte[] hash = hashAlgorithm.ComputeHash(data);

            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < hash.Length; ++i)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        static private string GetHash(HashAlgorithm hashAlgorithm, Stream data)
        {
            byte[] hash = hashAlgorithm.ComputeHash(data);

            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < hash.Length; ++i)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        static private bool VerifyHash(HashAlgorithm hashAlgorithm, byte[] data, string hash)
        {
            string hashOfData = GetHash(hashAlgorithm, data);
            return StringComparer.OrdinalIgnoreCase.Compare(hashOfData, hash) == 0;
        }

        static private bool VerifyHash(HashAlgorithm hashAlgorithm, Stream data, string hash)
        {
            string hashOfData = GetHash(hashAlgorithm, data);
            return StringComparer.OrdinalIgnoreCase.Compare(hashOfData, hash) == 0;
        }

        static public string Hash(string data)
        {
            // using(SHA256 alg = SHA256.Create())
            using(MD5 md5 = MD5.Create())
            {
                return GetHash(md5, Encoding.UTF8.GetBytes(data));
            }
        }
        
        static public bool Verify(string data, string hash)
        {
            using(MD5 md5 = MD5.Create())
            {
                return VerifyHash(md5, Encoding.UTF8.GetBytes(data), hash);
            }
        }

        static public string Hash(Stream data)
        {
            using(MD5 md5 = MD5.Create())
            {
                return GetHash(md5, data);
            }
        }

        static public bool Verify(Stream data, string hash)
        {
            using(MD5 md5 = MD5.Create())
            {
                return VerifyHash(md5, data, hash);
            }
        }
    }
}