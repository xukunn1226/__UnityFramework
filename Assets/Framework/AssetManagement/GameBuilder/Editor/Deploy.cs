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
        // backup "app" and "assetbundles"
        static public void cmdDeploy()
        {
            // source path
            string rootPath = VersionDefines.DEPLOYMENT_ROOT_PATH;
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
                success = PublishExtraAndPkgDataToCDN(srcRootPath, appDirectory);

            if (success)
                success = GeneratePatch(srcRootPath, appDirectory);

            if (success)
            {
                Debug.Log($"Deploy operator is finished successfully: [{appDirectory}]");
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
            string appSrcPath = string.Format($"{rootPath}/{VersionDefines.DEPLOYMENT_LATEST_APP_PATH}/{Utility.GetPlatformName()}");
            string bundlesSrcPath = string.Format($"{rootPath}/{VersionDefines.DEPLOYMENT_LATEST_BUNDLE_PATH}/{Utility.GetPlatformName()}");

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
            string bakPath = string.Format($"{rootPath}/{VersionDefines.DEPLOYMENT_BACKUP_FOLDER}/{Utility.GetPlatformName()}/{appDirectory}");
            string appDstPath = string.Format($"{bakPath}/{VersionDefines.DEPLOYMENT_BACKUP_APP_FOLDER}");
            string bundlesDstPath = string.Format($"{bakPath}/{VersionDefines.DEPLOYMENT_BACKUP_BUNDLE_FOLDER}");
            try
            {
                if (Directory.Exists(bakPath))
                    Directory.Delete(bakPath, true);
                Directory.CreateDirectory(appDstPath);
                Directory.CreateDirectory(bundlesDstPath);

                Framework.Core.Editor.EditorUtility.CopyDirectory(appSrcPath, appDstPath);
                Framework.Core.Editor.EditorUtility.CopyDirectory(bundlesSrcPath, bundlesDstPath);
                File.Copy(string.Format($"Assets/Temp/{Utility.GetPlatformName()}_manifest.zjson"), string.Format($"{bakPath}/manifest.zjson"), true);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }

            // 生成所有资源文件相关信息（FileList）
            return BundleFileList.BuildBundleFileList(bundlesDstPath, "", 
                                                      string.Format($"{bakPath}/{VersionDefines.DEPLOYMENT_FULL_FILELIST_NAME}"));
        }

        /// <summary>
        /// 发布extra, pkg_XXX(cdn/data/windows/0.0.2/extra,cdn/data/windows/0.0.2/pkg_XXX)
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="appDirectory"></param>
        /// <returns></returns>
        static private bool PublishExtraAndPkgDataToCDN(string rootPath, string appDirectory)
        {
            // deployment/backup/windows/0.0.2/assetbundles
            string appSrcPath = string.Format($"{rootPath}/{VersionDefines.DEPLOYMENT_BACKUP_FOLDER}/{Utility.GetPlatformName()}/{appDirectory}/{VersionDefines.DEPLOYMENT_BACKUP_BUNDLE_FOLDER}");
            // deployment/cdn/data/windows/0.0.2
            string appDstPath = string.Format($"{rootPath}/{VersionDefines.cdnExtraDataPath}/{Utility.GetPlatformName()}/{appDirectory}");
            if(!Directory.Exists(appDstPath))
            {
                Directory.CreateDirectory(appDstPath);
            }

            List<string> subFolders = new List<string>();
            if (Directory.Exists(string.Format($"{appSrcPath}/{VersionDefines.EXTRA_FOLDER}")))
            {
                subFolders.Add($"{VersionDefines.EXTRA_FOLDER}");
            }
            DirectoryInfo di = new DirectoryInfo(appSrcPath);
            DirectoryInfo[] dis = di.GetDirectories(string.Format($"{VersionDefines.PKG_FOLDER_PREFIX}*"), SearchOption.TopDirectoryOnly);
            foreach(DirectoryInfo d in dis)
            {
                subFolders.Add(d.Name);
            }

            foreach(var subFolder in subFolders)
            {
                try
                {
                    string dstFilePath = appDstPath + "/" + subFolder;
                    if (Directory.Exists(dstFilePath))
                    {
                        Directory.Delete(dstFilePath, true);
                    }
                    Directory.CreateDirectory(dstFilePath);
                    Framework.Core.Editor.EditorUtility.CopyDirectory(appSrcPath + "/" + subFolder, dstFilePath);
                }
                catch(System.Exception e)
                {
                    Debug.LogError(e.Message);
                    return false;
                }
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
            string path = VersionDefines.CACHED_BACKDOOR_PATH;
            Backdoor bd = Backdoor.Deserialize(path);
            if(bd == null)
            {
                Debug.LogError($"failed to load backdoor.json. {path}");
                return false;
            }

            // deployment/cdn/patch/windows/0.0.2
            string targetDirectory = string.Format($"{rootPath}/{VersionDefines.cdnPatchDataPath}/{Utility.GetPlatformName()}/{appDirectory}");
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
                        string subDirectory = string.Format($"{rootPath}/{VersionDefines.cdnPatchDataPath}/{Utility.GetPlatformName()}/{appDirectory}/{item.Key}");
                        string diffFilename = string.Format($"{subDirectory}/{VersionDefines.DIFF_FILENAME}");

                        using (FileStream fs = new FileStream(diffFilename, FileMode.Open))
                        {
                            string hash = EasyMD5.Hash(fs);
                            dc.VersionHashMap.Add(item.Key, hash);
                        }
                    }
                }
            }
            string dcFilename = string.Format($"{targetDirectory}/{VersionDefines.DIFFCOLLECTION_FILENAME}");
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
            File.Copy(VersionDefines.CACHED_BACKDOOR_PATH, string.Format($"{rootPath}/{VersionDefines.cdnBackdoorPath}"), true);

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
            string prevAppPath = string.Format($"{rootPath}/{VersionDefines.DEPLOYMENT_BACKUP_FOLDER}/{Utility.GetPlatformName()}/{prevApp}/{VersionDefines.DEPLOYMENT_BACKUP_BUNDLE_FOLDER}");
            string curAppPath = string.Format($"{rootPath}/{VersionDefines.DEPLOYMENT_BACKUP_FOLDER}/{Utility.GetPlatformName()}/{curApp}/{VersionDefines.DEPLOYMENT_BACKUP_BUNDLE_FOLDER}");
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

            string prevFileListPath = string.Format($"{rootPath}/{VersionDefines.DEPLOYMENT_BACKUP_FOLDER}/{Utility.GetPlatformName()}/{prevApp}/{VersionDefines.DEPLOYMENT_FULL_FILELIST_NAME}");
            if(!File.Exists(prevFileListPath))
            {
                Debug.LogError($"{prevFileListPath} not found");
                return null;
            }
            
            string curFileListPath = string.Format($"{rootPath}/{VersionDefines.DEPLOYMENT_BACKUP_FOLDER}/{Utility.GetPlatformName()}/{curApp}/{VersionDefines.DEPLOYMENT_FULL_FILELIST_NAME}");
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
            string targetDirectory = string.Format($"{rootPath}/{VersionDefines.cdnPatchDataPath}/{Utility.GetPlatformName()}/{curApp}/{prevApp}");
            Directory.CreateDirectory(targetDirectory);
            Framework.Core.Diff.Serialize(string.Format($"{targetDirectory}/{VersionDefines.DIFF_FILENAME}"), data);

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