using System.Collections;
using System.Collections.Generic;
using System.IO;
using Framework.Core;
using UnityEngine;
using UnityEditor;

namespace Framework.AssetManagement.GameBuilder
{
    public class Deployment
    {
        static public string s_DefaultRootPath      = "Deployment";
        static public string s_LatestAppPath        = "Latest/player";                  // ����app��ַ
        static public string s_LatestBundlesPath    = "Latest/assetbundles";            // ������Դ��ַ
        static public string s_BackupDirectoryPath  = "Backup";                         // ���ݵ�ַ
        static public string s_Cdn_DataPath         = "Cdn/data";                       // �������°汾����Դ��ַ
        static public string s_Cdn_ObbPath          = "Cdn/obb";                        // ���а汾��obb��ַ
        static public string s_Cdn_PatchPath        = "Cdn/patch";                      // ����ƽ̨��������ַ
        static public string s_BackdoorPath         = "Cdn/backdoor.json";

        // backup "app" and "assetbundles"
        static public void cmdDeploy()
        {
            // source path
            string rootPath = s_DefaultRootPath;
            CommandLineReader.GetCommand("RootPath", ref rootPath);

            // determine backup folder
            string appDirectory = AppVersion.EditorLoad().ToString3();
            CommandLineReader.GetCommand("AppDirectory", ref appDirectory);

            Run(rootPath, appDirectory);
        }

        /// <summary>
        /// ����汾�����ݡ����ɲ������ݵȣ�
        /// </summary>
        /// <param name="srcRootPath"></param>
        /// <param name="dstRootPath"></param>
        /// <param name="appDirectory"></param>
        static public void Run(string srcRootPath, string appDirectory)
        {
            // step1. �������°汾
            Backup(srcRootPath, appDirectory);

            // step2. ����app & obb
            PublishDataAndObb(srcRootPath, appDirectory);

            // step3. patch generator
            BuildPatch(srcRootPath, appDirectory);
        }

        // srcPath: Deployment/Latest
        // dstPath: Deployment/Backup
        // appDirectory: 0.1.2.1
        static private void Backup(string rootPath, string appDirectory)
        {
            // source path
            string appSrcPath = string.Format($"{rootPath}/{s_LatestAppPath}/{Utility.GetPlatformName()}");
            string bundlesSrcPath = string.Format($"{rootPath}/{s_LatestBundlesPath}/{Utility.GetPlatformName()}");

            if(!Directory.Exists(appSrcPath))
            {
                if(Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                throw new System.Exception($"{appSrcPath} not found");
            }
            if(!Directory.Exists(bundlesSrcPath))
            {
                if(Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                throw new System.Exception($"{bundlesSrcPath} not found");
            }

            // clear directory
            string bakPath = string.Format($"{rootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{appDirectory}");
            if(Directory.Exists(bakPath))
                Directory.Delete(bakPath, true);

            // check destination path
            string appDstPath = string.Format($"{bakPath}/app");
            string bundlesDstPath = string.Format($"{bakPath}/assetbundles");
            try
            {
                Directory.CreateDirectory(appDstPath);
                Directory.CreateDirectory(bundlesDstPath);
            }
            catch(System.Exception e)
            {
                if(Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                Debug.LogError(e.Message);
            }

            Framework.Core.Editor.EditorUtility.CopyDirectory(appSrcPath, appDstPath);
            Framework.Core.Editor.EditorUtility.CopyDirectory(bundlesSrcPath, bundlesDstPath);

            // ����file list�����ں���diffʹ��
            BundleFileList.BuildBundleFileList(bundlesDstPath,
                                               string.Format($"{bakPath}/{BundleExtracter.FILELIST_NAME}"));
        }

        // srcPath: Deployment/Backup/windows/0.0.1/assetbundles
        // dstPath: Deployment/CDN/data/windows/
        // appDirectory: 0.1.2.1
        static private void PublishDataAndObb(string rootPath, string appDirectory)
        {
            string appSrcPath = string.Format($"{rootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{appDirectory}/assetbundles");
            string appDstPath = string.Format($"{rootPath}/{s_Cdn_DataPath}/{Utility.GetPlatformName()}");
            if (Directory.Exists(appDstPath))
            {
                Directory.Delete(appDstPath, true);
            }
            Directory.CreateDirectory(appDstPath);
            Framework.Core.Editor.EditorUtility.CopyDirectory(appSrcPath, appDstPath);
        }

        /// <summary>
        /// ����backdoor.json����������ʷ�汾�뵱ǰ�汾�Ĳ������ݣ�diff.json��
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="appDirectory"></param>
        static private bool BuildPatch(string rootPath, string appDirectory)
        {
            string path = string.Format($"{rootPath}/{s_BackdoorPath}");
            Backdoor bd = Backdoor.Deserialize(path);
            if(bd == null)
            {
                Debug.LogError($"failed to load backdoor.json. {path}");
                return false;
            }

            string targetDirectory = string.Format($"{rootPath}/{s_Cdn_PatchPath}/{Utility.GetPlatformName()}/{appDirectory}");
            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            // 根据所有历史版本记录，生成最新版本与其他版本的差异数据
            AppVersion curVersion = new AppVersion(appDirectory);
            foreach(var version in bd.VersionHistory)
            {
                AppVersion historyVer = new AppVersion(version);
                if(historyVer.CompareTo(bd.MinVersion) >= 0 &&
                   historyVer.CompareTo(curVersion) < 0)
                {
                    Diff data = Diff(rootPath, version, appDirectory);
                    if(data == null)
                    {
                        Debug.LogError($"failed to Diff between {version} and {appDirectory}");
                        return false;
                    }
                    else
                    {
                        string historyVerDirectory = string.Format($"{targetDirectory}/{version}");
                        Directory.CreateDirectory(historyVerDirectory);

                        Framework.Core.Diff.Serialize(string.Format($"{historyVerDirectory}/diff.json"), data);

                        string curAppPath = string.Format($"{rootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{appDirectory}/assetbundles");
                        foreach (var dfi in data.AddedFileList)
                        {
                            try
                            {
                                File.Copy(string.Format($"{curAppPath}/{dfi.BundleName}"), string.Format($"{historyVerDirectory}/{dfi.BundleName}"), true);
                            }
                            catch(System.Exception e)
                            {
                                Debug.LogError(e.Message);
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 计算两个版本的差异数据
        /// e.g     baseApp: 0.0.1  curApp: 0.0.1.1
        /// </summary>
        /// <param name="baseApp"></param>
        /// <param name="curApp"></param>
        /// <returns></returns>
        static private Diff Diff(string rootPath, string baseApp, string curApp)
        {
            string baseAppPath = string.Format($"{rootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{baseApp}/assetbundles");
            string curAppPath = string.Format($"{rootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{curApp}/assetbundles");
            if(!Directory.Exists(baseAppPath))
            {
                Debug.LogError($"{baseAppPath} is not exists");
                return null;
            }
            if(!Directory.Exists(curAppPath))
            {
                Debug.LogError($"{curAppPath} is not exists");
                return null;
            }

            string baseFileListPath = string.Format($"{rootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{baseApp}/{BundleExtracter.FILELIST_NAME}");
            if(!File.Exists(baseFileListPath))
            {
                Debug.LogError($"{baseFileListPath} not found");
                return null;
            }
            
            string curFileListPath = string.Format($"{rootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{curApp}/{BundleExtracter.FILELIST_NAME}");
            if (!File.Exists(curFileListPath))
            {
                Debug.LogError($"{curFileListPath} not found");
                return null;
            }

            string baseFileListJson = File.ReadAllText(baseFileListPath);
            BundleFileList baseBFL = BundleFileList.DeserializeFromJson(baseFileListJson);
            if(baseBFL == null)
            {
                Debug.LogError($"can't parse to json. {baseFileListPath}");
                return null;
            }

            string curFileListJson = File.ReadAllText(curFileListPath);
            BundleFileList curBFL = BundleFileList.DeserializeFromJson(curFileListJson);
            if(curBFL == null)
            {
                Debug.LogError($"can't parse to json. {curFileListPath}");
                return null;
            }

            Diff data = new Diff();
            data.Desc = string.Format($"{baseApp}-{curApp}");
            foreach(var bfi in curBFL.FileList)
            {
                BundleFileInfo findIt = baseBFL.FileList.Find(item => item.BundleName == bfi.BundleName);
                if (findIt == null)
                {
                    data.AddedFileList.Add(new Core.Diff.DiffFileInfo() { BundleName = bfi.BundleName, FileHash = bfi.FileHash, Size = bfi.Size });
                }
                else
                {
                    if(string.Compare(findIt.FileHash, bfi.FileHash) != 0)
                    {
                        data.UpdatedFileList.Add(new Core.Diff.DiffFileInfo() { BundleName = bfi.BundleName, FileHash = bfi.FileHash, Size = bfi.Size });
                    }
                }
            }
            foreach(var bfi in baseBFL.FileList)
            {
                BundleFileInfo findIt = curBFL.FileList.Find(item => item.BundleName == bfi.BundleName);
                if(findIt == null)
                {
                    data.DeletedFileList.Add(new Core.Diff.DiffFileInfo() { BundleName = bfi.BundleName, FileHash = bfi.FileHash, Size = bfi.Size });
                }
            }
            return data;
        }
    }
}