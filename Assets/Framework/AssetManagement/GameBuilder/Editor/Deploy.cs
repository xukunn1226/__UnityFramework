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
        static public string s_LatestAppPath        = "latest/player";                  // 当前编译版本的app
        static public string s_LatestBundlesPath    = "latest/assetbundles";            // 当前编译版本的bundles
        static public string s_BackupDirectoryPath  = "backup";                         // 
        static public string s_Cdn_DataPath         = "cdn/data";                       // 最新版本的资源数据
        static public string s_Cdn_ObbPath          = "cdn/obb";                        // 所有版本的obb
        static public string s_Cdn_PatchPath        = "cdn/patch";                      // 所有平台的补丁数据
        static public string s_BackdoorPath         = "cdn/backdoor.json";

        // backup "app" and "assetbundles"
        static public void cmdDeploy()
        {
            // source path
            string rootPath = s_DefaultRootPath;
            CommandLineReader.GetCommand("RootPath", ref rootPath);

            // determine backup folder
            string appDirectory = AppVersion.EditorLoad().ToString();
            CommandLineReader.GetCommand("AppDirectory", ref appDirectory);

            Run(rootPath, appDirectory);
        }

        /// <summary>
        /// 执行部署流程（备份、发布、生成diff）
        /// </summary>
        /// <param name="srcRootPath"></param>
        /// <param name="dstRootPath"></param>
        /// <param name="appDirectory"></param>
        static public bool Run(string srcRootPath, string appDirectory)
        {
            bool success;

            success = Backup(srcRootPath, appDirectory);

            if(success)
                success = PublishDataAndObb(srcRootPath, appDirectory);

            if(success)
                success = BuildPatch(srcRootPath, appDirectory);

            if(success)
            {
                Debug.Log($"Deploy operator is finished successfully");
            }
            else
            {
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                Debug.LogError($"Deploy operator occurs fetal error: {appDirectory}");
            }

            return success;
        }

        /// <summary>
        /// 从Latest中备份数据至指定文件夹，并计算其MD5
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="appDirectory"></param>
        /// <returns></returns>
        static private bool Backup(string rootPath, string appDirectory)
        {
            // source path
            string appSrcPath = string.Format($"{rootPath}/{s_LatestAppPath}/{Utility.GetPlatformName()}");
            string bundlesSrcPath = string.Format($"{rootPath}/{s_LatestBundlesPath}/{Utility.GetPlatformName()}");

            if(!Directory.Exists(appSrcPath))
            {
                Debug.LogError($"{appSrcPath} not found");
                return false;
            }
            if(!Directory.Exists(bundlesSrcPath))
            {
                Debug.LogError($"{bundlesSrcPath} not found");
                return false;
            }

            // clear and create directory
            string bakPath = string.Format($"{rootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{appDirectory}");
            string appDstPath = string.Format($"{bakPath}/app");
            string bundlesDstPath = string.Format($"{bakPath}/assetbundles");
            try
            {
                if (Directory.Exists(bakPath))
                    Directory.Delete(bakPath, true);
                Directory.CreateDirectory(appDstPath);
                Directory.CreateDirectory(bundlesDstPath);

                Framework.Core.Editor.EditorUtility.CopyDirectory(appSrcPath, appDstPath);
                Framework.Core.Editor.EditorUtility.CopyDirectory(bundlesSrcPath, bundlesDstPath);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }

            // 生成所有资源文件相关信息（FileList）
            return BundleFileList.BuildBundleFileList(bundlesDstPath,
                                                      string.Format($"{bakPath}/{BundleExtracter.FILELIST_NAME}"));
        }

        /// <summary>
        /// 发布指定版本至cdn/data
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="appDirectory"></param>
        /// <returns></returns>
        static private bool PublishDataAndObb(string rootPath, string appDirectory)
        {
            string appSrcPath = string.Format($"{rootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{appDirectory}/assetbundles");
            string appDstPath = string.Format($"{rootPath}/{s_Cdn_DataPath}/{Utility.GetPlatformName()}");
            try
            {
                if (Directory.Exists(appDstPath))
                {
                    Directory.Delete(appDstPath, true);
                }
                Directory.CreateDirectory(appDstPath);
                Framework.Core.Editor.EditorUtility.CopyDirectory(appSrcPath, appDstPath);
            }
            catch(System.Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 生成其他版本到当前版本（appDirectory）的差异数据
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
            try
            {
                if (Directory.Exists(targetDirectory))
                {
                    Directory.Delete(targetDirectory, true);
                }
                Directory.CreateDirectory(targetDirectory);
            }
            catch(System.Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }

            // 根据所有历史版本记录，生成最新版本与其他版本的差异数据
            AppVersion curVersion = ScriptableObject.CreateInstance<AppVersion>();
            curVersion.Set(appDirectory);
            foreach(var version in bd.VersionHistory)
            {
                AppVersion historyVer = ScriptableObject.CreateInstance<AppVersion>();
                historyVer.Set(version);
                if (historyVer.CompareTo(bd.MinVersion) >= 0 &&
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

                        // 序列号diff.json
                        Framework.Core.Diff.Serialize(string.Format($"{historyVerDirectory}/diff.json"), data);

                        // 传输补丁数据
                        string curAppPath = string.Format($"{rootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{appDirectory}/assetbundles");
                        foreach (var dfi in data.AddedFileList)
                        {
                            try
                            {
                                string dstFilename = string.Format($"{historyVerDirectory}/{dfi.BundleName}");
                                string dstDirectory = Path.GetDirectoryName(dstFilename);
                                if (!Directory.Exists(dstDirectory))
                                    Directory.CreateDirectory(dstDirectory);
                                File.Copy(string.Format($"{curAppPath}/{dfi.BundleName}"), dstFilename, true);
                            }
                            catch(System.Exception e)
                            {
                                Debug.LogError(e.Message);
                                return false;
                            }
                        }
                        foreach (var dfi in data.UpdatedFileList)
                        {
                            try
                            {
                                string dstFilename = string.Format($"{historyVerDirectory}/{dfi.BundleName}");
                                string dstDirectory = Path.GetDirectoryName(dstFilename);
                                if (!Directory.Exists(dstDirectory))
                                    Directory.CreateDirectory(dstDirectory);
                                File.Copy(string.Format($"{curAppPath}/{dfi.BundleName}"), dstFilename, true);
                            }
                            catch (System.Exception e)
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
                    data.PushAddedFile(new Core.Diff.DiffFileInfo() { BundleName = bfi.BundleName, FileHash = bfi.FileHash, Size = bfi.Size });
                }
                else
                {
                    if(string.Compare(findIt.FileHash, bfi.FileHash) != 0)
                    {
                        data.PushUpdatedFile(new Core.Diff.DiffFileInfo() { BundleName = bfi.BundleName, FileHash = bfi.FileHash, Size = bfi.Size });
                    }
                }
            }
            foreach(var bfi in baseBFL.FileList)
            {
                BundleFileInfo findIt = curBFL.FileList.Find(item => item.BundleName == bfi.BundleName);
                if(findIt == null)
                {
                    data.PushDeletedFile(new Core.Diff.DiffFileInfo() { BundleName = bfi.BundleName, FileHash = bfi.FileHash, Size = bfi.Size });
                }
            }
            return data;
        }
    }
}