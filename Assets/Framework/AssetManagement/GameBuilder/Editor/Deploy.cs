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
        static public string s_BackupDirectoryPath  = "backup";                         // 备份各平台下发布的资源（app & bundles）
        static public string s_CdnRootPath          = "cdn";                            // cdn path, base on s_DefaultRootPath
        static public string s_Cdn_DataPath         = "data";                           // 存储最新版本的资源数据
        static public string s_BackdoorPath         = s_CdnRootPath + "/" + Patcher.BACKDOOR_FILENAME;
        static private string s_SavedBackdoorPath   = "Assets/Framework/AssetManagement/GameBuilder/Data/" + Patcher.BACKDOOR_FILENAME;

        // 全量资源路径
        static public string baseDataPath
        {
            get
            {
                return string.Format($"{s_CdnRootPath}/{s_Cdn_DataPath}");
            }
        }

        // patch资源路径
        static public string patchPath
        {
            get
            {
                return string.Format($"{s_CdnRootPath}/{Patcher.PATCH_PATH}");
            }
        }

        // backup "app" and "assetbundles"
        static public void cmdDeploy()
        {
            // source path
            string rootPath = s_DefaultRootPath;
            CommandLineReader.GetCommand("RootPath", ref rootPath);

            // backup folder
            string appDirectory = AppVersion.EditorLoad().ToString();
            CommandLineReader.GetCommand("AppDirectory", ref appDirectory);

            Run(rootPath, appDirectory);
        }

        /// <summary>
        /// 执行部署流程（备份、发布、生成diff）
        /// </summary>
        /// <param name="srcRootPath">"Deployment/"</param>
        /// <param name="appDirectory">"0.0.2"</param>
        static public bool Run(string srcRootPath, string appDirectory)
        {
            bool success;

            success = Backup(srcRootPath, appDirectory);

            if (success)
                success = PublishDataToCDN(srcRootPath, appDirectory);

            if (success)
                success = GeneratePatch(srcRootPath, appDirectory);

            if (success)
            {
                Debug.Log($"Deploy operator is finished successfully");
            }
            else
            {
                if (UnityEngine.Application.isBatchMode)
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
        /// 发布指定版本的全量资源至cdn/data
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="appDirectory"></param>
        /// <returns></returns>
        static private bool PublishDataToCDN(string rootPath, string appDirectory)
        {
            // deployment/backup/windows/0.0.2/assetbundles
            string appSrcPath = string.Format($"{rootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{appDirectory}/assetbundles");
            // deployment/cdn/data/windows/0.0.2
            string appDstPath = string.Format($"{rootPath}/{baseDataPath}/{Utility.GetPlatformName()}/{appDirectory}");
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
        /// STEP1. 更新本地backdoor.zjon（）
        /// </summary>
        /// <param name="rootPath">Deployment</param>
        /// <param name="appDirectory">0.0.2</param>
        static private bool GeneratePatch(string rootPath, string appDirectory)
        {            
            string path = s_SavedBackdoorPath;
            Backdoor bd = Backdoor.Deserialize(path);
            if(bd == null)
            {
                Debug.LogError($"failed to load backdoor.json. {path}");
                return false;
            }

            // deployment/cdn/patch/windows/0.0.2
            string targetDirectory = string.Format($"{rootPath}/{patchPath}/{Utility.GetPlatformName()}/{appDirectory}");
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
            Dictionary<string, string> versionHistory = bd.GetVersionHistory();
            if (versionHistory != null)
            {
                foreach (var item in versionHistory)
                {
                    AppVersion historyVer = ScriptableObject.CreateInstance<AppVersion>();
                    historyVer.Set(item.Key);
                    // 历史版本小于当前版本或当前版本大于历史版本且历史版本大于等于最小强更版本
                    if ((string.IsNullOrEmpty(bd.MinVersion) && historyVer.CompareTo(curVersion) < 0)
                    || (!string.IsNullOrEmpty(bd.MinVersion) && historyVer.CompareTo(bd.MinVersion) >= 0 && historyVer.CompareTo(curVersion) < 0))
                    {
                        if (Diff(rootPath, item.Key, appDirectory) == null)
                        {
                            Debug.LogError($"failed to Diff between {item.Key} and {appDirectory}");
                            return false;
                        }
                    }
                }
            }

            // generate diffcollection.json
            DiffCollection dc = new DiffCollection();
            dc.BaseVersion = appDirectory;
            if (versionHistory != null)
            {
                foreach (var item in versionHistory)
                {
                    AppVersion historyVer = ScriptableObject.CreateInstance<AppVersion>();
                    historyVer.Set(item.Key);
                    if ((string.IsNullOrEmpty(bd.MinVersion) && historyVer.CompareTo(curVersion) < 0)
                    || (!string.IsNullOrEmpty(bd.MinVersion) && historyVer.CompareTo(bd.MinVersion) >= 0 && historyVer.CompareTo(curVersion) < 0))
                    {
                        string subDirectory = string.Format($"{rootPath}/{patchPath}/{Utility.GetPlatformName()}/{appDirectory}/{item.Key}");
                        string diffFilename = string.Format($"{subDirectory}/{Patcher.DIFF_FILENAME}");

                        using (FileStream fs = new FileStream(diffFilename, FileMode.Open))
                        {
                            string hash = EasyMD5.Hash(fs);
                            dc.VersionHashMap.Add(item.Key, hash);
                        }
                    }
                }
            }
            string dcFilename = string.Format($"{targetDirectory}/{Patcher.DIFFCOLLECTION_FILENAME}");
            DiffCollection.Serialize(dcFilename, dc);

            bd.CurVersion = appDirectory;
            FileStream dc_fs = new FileStream(dcFilename, FileMode.Open);
            if(!versionHistory.ContainsKey(dc.BaseVersion))
            {
                versionHistory.Add(dc.BaseVersion, EasyMD5.Hash(dc_fs));
            }
            else
            {
                versionHistory[dc.BaseVersion] = EasyMD5.Hash(dc_fs);
            }
            dc_fs.Dispose();
            dc_fs.Close();
            Backdoor.Serialize(path, bd);

            // copy backdoor.zjson from s_SavedBackdoorPath to cdn
            File.Copy(s_SavedBackdoorPath, string.Format($"{rootPath}/{s_BackdoorPath}"), true);

            return true;
        }

        /// <summary>
        /// 计算从版本prevApp升级到curApp需要的数据，输出至Deployment/cdn/patch/windows/curApp/prevApp/diff.json
        /// e.g     prevApp: 0.0.1.1  curApp: 0.0.2
        /// </summary>
        /// <param name="prevApp"></param>
        /// <param name="curApp"></param>
        /// <returns></returns>
        static private Diff Diff(string rootPath, string prevApp, string curApp)
        {
            string prevAppPath = string.Format($"{rootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{prevApp}/assetbundles");
            string curAppPath = string.Format($"{rootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{curApp}/assetbundles");
            if(!Directory.Exists(prevAppPath))
            {
                Debug.LogError($"{prevAppPath} is not exists");
                return null;
            }
            if(!Directory.Exists(curAppPath))
            {
                Debug.LogError($"{curAppPath} is not exists");
                return null;
            }

            string prevFileListPath = string.Format($"{rootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{prevApp}/{BundleExtracter.FILELIST_NAME}");
            if(!File.Exists(prevFileListPath))
            {
                Debug.LogError($"{prevFileListPath} not found");
                return null;
            }
            
            string curFileListPath = string.Format($"{rootPath}/{s_BackupDirectoryPath}/{Utility.GetPlatformName()}/{curApp}/{BundleExtracter.FILELIST_NAME}");
            if (!File.Exists(curFileListPath))
            {
                Debug.LogError($"{curFileListPath} not found");
                return null;
            }

            string prevFileListJson = File.ReadAllText(prevFileListPath);
            BundleFileList prevBFL = BundleFileList.DeserializeFromJson(prevFileListJson);
            if(prevBFL == null)
            {
                Debug.LogError($"can't parse to json. {prevFileListPath}");
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
            data.Desc = string.Format($"{prevApp}-{curApp}");
            foreach(var bfi in curBFL.FileList)
            {
                BundleFileInfo findIt = prevBFL.FileList.Find(item => item.BundleName == bfi.BundleName);
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
            foreach(var bfi in prevBFL.FileList)
            {
                BundleFileInfo findIt = curBFL.FileList.Find(item => item.BundleName == bfi.BundleName);
                if(findIt == null)
                {
                    data.PushDeletedFile(new Core.Diff.DiffFileInfo() { BundleName = bfi.BundleName, FileHash = bfi.FileHash, Size = bfi.Size });
                }
            }

            // 序列化diff.json
            string targetDirectory = string.Format($"{rootPath}/{patchPath}/{Utility.GetPlatformName()}/{curApp}/{prevApp}");
            Directory.CreateDirectory(targetDirectory);
            Framework.Core.Diff.Serialize(string.Format($"{targetDirectory}/{Patcher.DIFF_FILENAME}"), data);

            // 根据diff结果填充历史版本升级到当前版本需要的数据
            foreach (var dfi in data.AddedFileList)
            {
                if (!CopyFile(targetDirectory, curAppPath, dfi.BundleName))
                    return null;
            }
            foreach (var dfi in data.UpdatedFileList)
            {
                if (!CopyFile(targetDirectory, curAppPath, dfi.BundleName))
                    return null;
            }

            return data;
        }
        
        static private bool CopyFile(string prevAssetPath, string curAssetPath, string bundleName)
        {
            try
            {
                string dstFilename = string.Format($"{prevAssetPath}/{bundleName}");
                string dstDirectory = Path.GetDirectoryName(dstFilename);
                if (!Directory.Exists(dstDirectory))
                    Directory.CreateDirectory(dstDirectory);
                File.Copy(string.Format($"{curAssetPath}/{bundleName}"), dstFilename, true);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
            return true;
        }
    }
}