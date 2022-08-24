using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.Core
{
    /// <summary>
    /// 1、下载backdoor.json
    /// 2、获取PlayerPrefs与backdoor中最新版本号对比，判断是否是最新版本或补丁已下载完成
    /// 3、初始化下载环境(下载diffcollection.json & diff.json)
    /// 4、下载补丁
    /// 5、标记PlayerPrefs完成
    /// </summary>
    public class Patcher : MonoBehaviour
    {
        private int                         m_WorkerCount;
        private List<DownloadTask>          m_TaskWorkerList;
        private List<byte[]>                m_CachedBufferList          = new List<byte[]>();
        private const int                   m_BufferSize                = 1024 * 1024;
        private List<Diff.DiffFileInfo>     m_DownloadFileList          = new List<Diff.DiffFileInfo>();
        private int                         m_PendingDownloadFileIndex;

        private DownloadTask                m_SingleFileTask;

        private Backdoor                    m_Backdoor;                 // 服务器下载的backdoor
        private DiffCollection              m_DiffCollection;           // 对应最新版本的diffcollection.json
        private Diff                        m_Diff;                     // 补丁包差异配置数据
        private AppVersion                  m_RemoteVersion;            // 远程版本号（记录在backdoor）
        private AppVersion                  m_CurVersion;               // 当前版本号（记录在PlayerPrefs）
        private AppVersion                  m_AppVersion;               // 当前引擎版本号，记录在Resources/

        private string                      m_CdnURL;
        private string                      m_Error;

        private IPatcherListener            m_Listener;

        private string localBackdoorURL
        {
            get
            {
                return string.Format($"{Application.persistentDataPath}/{VersionDefines.BACKDOOR_FILENAME}");
            }
        }

        private string remoteBackdoorURL
        {
            get
            {
                return string.Format($"{m_CdnURL}/{VersionDefines.BACKDOOR_FILENAME}");
            }
        }

        /// <summary>
        /// 获取远程版本号（可能三位或四位）
        /// </summary>
        /// <returns></returns>
        public AppVersion remoteCurVersion
        {
            get
            {
                if(m_RemoteVersion == null && m_Backdoor != null)
                {
                    m_RemoteVersion = AppVersion.CreateInstance<AppVersion>();
                    m_RemoteVersion.Set(m_Backdoor.CurVersion);
                }
                return m_RemoteVersion;
            }
        }

        /// <summary>
        /// 获取本地当前版本号（可能三位或四位）
        /// </summary>
        /// <returns></returns>
        public AppVersion localCurVersion
        {
            get
            {
                if(m_CurVersion == null)
                {
                    m_CurVersion = AppVersion.CreateInstance<AppVersion>();
                }
                string cachedCurVersion = PlayerPrefs.GetString(VersionDefines.CUR_APPVERSION);
                if(string.IsNullOrEmpty(cachedCurVersion))
                { // 首次安装或尚未下载完，会没有CUR_APPVERSION标记，此时使用母包的版本号
                    cachedCurVersion = localBaseVersion.ToString();
                }
                m_CurVersion.Set(cachedCurVersion);
                return m_CurVersion;
            }
        }

        /// <summary>
        /// 本地母包的引擎版本号（三位）
        /// </summary>
        public AppVersion localBaseVersion
        {
            get
            {
                if(m_AppVersion == null)
                {
                    m_AppVersion = AppVersion.CreateInstance<AppVersion>();
                    AppVersion version = AppVersion.Load();
                    m_AppVersion.Set(version.ToString());
                    AppVersion.Unload(version);
                }
                return m_AppVersion;
            }
        }

        public void StartWork(string cdnURL, int workerCount, IPatcherListener listener = null)
        {
            m_Listener = listener;
            m_CdnURL = cdnURL;
            m_WorkerCount = workerCount;

            StartCoroutine(Run());
        }
        
        private IEnumerator Run()
        {
            if(m_SingleFileTask == null)
                m_SingleFileTask = new DownloadTask(new byte[m_BufferSize]);

            // step1. download backdoor config
            yield return StartCoroutine(DownloadBackdoor());

            bool isContinue = m_Listener?.OnError_DownloadBackdoor(m_Error, m_Backdoor) ?? true;
            if(!string.IsNullOrEmpty(m_Error) || !isContinue)
            {
                yield break;
            }

            // step2. check that the current version is up to date
            if (IsLatestVersion())
            {
                m_Listener?.OnCheck_IsLatestVersion(true);
                yield break;
            }
            else
            {
                m_Listener?.OnCheck_IsLatestVersion(false);
            }

            // step3. download the diff collection of the latest version
            yield return StartCoroutine(DownloadDiffCollection());
            if (!string.IsNullOrEmpty(m_Error))
            {
                m_Listener?.OnError_DownloadDiffCollection(m_Error);
                yield break;
            }

            // step4. download the diff.json for upgrade from local version to latest version
            yield return StartCoroutine(DownloadDiff());
            if (!string.IsNullOrEmpty(m_Error))
            {
                m_Listener?.OnError_DownloadDiff(m_Error);
                yield break;
            }
            
            // prepare for downloading
            long size = Prepare();
            m_Listener?.Prepare(m_DownloadFileList.Count, size);

            // step5. downloading...
            if(size > 0)
                yield return StartCoroutine(Downloading());

            DownloadingFinished();
        }

        private IEnumerator DownloadBackdoor()
        {
            if (File.Exists(localBackdoorURL))
                File.Delete(localBackdoorURL);

            DownloadTaskInfo info = new DownloadTaskInfo();
            info.srcUri = new Uri(remoteBackdoorURL);
            info.dstURL = localBackdoorURL;
            info.verifiedHash = null;
            info.retryCount = 3;
            yield return m_SingleFileTask.Run(info);

            m_Backdoor = null;
            if (string.IsNullOrEmpty(m_SingleFileTask.error))
            {
                m_Backdoor = Backdoor.Deserialize(localBackdoorURL);
            }
            else
            {
                m_Error = string.Format($"Occurs fetal error when download backdoor.json from {info.srcUri}. {m_SingleFileTask.error}");
            }
        }

        /// <summary>
        /// 根据本地版本号与远端最新版本号判断是否是最新版本
        /// </summary>
        /// <returns></returns>
        private bool IsLatestVersion()
        {
            return !string.IsNullOrEmpty(localCurVersion.ToString()) && remoteCurVersion.CompareTo(localCurVersion.ToString()) == 0;
        }

        private IEnumerator DownloadDiffCollection()
        {
            string hash = m_Backdoor.GetDiffCollectionFileHash(m_Backdoor.CurVersion);
            if (string.IsNullOrEmpty(hash))
            {
                m_Error = string.Format($"can't find the hash of diffcollection.json, plz check backdoor's version history.  CurVersion is [{m_Backdoor.CurVersion}]");
                yield break;
            }

            string localDiffCollectionURL = string.Format($"{Application.persistentDataPath}/{VersionDefines.DIFFCOLLECTION_FILENAME}");
            if (File.Exists(localDiffCollectionURL))
                File.Delete(localDiffCollectionURL);

            DownloadTaskInfo info = new DownloadTaskInfo();
            info.srcUri = new Uri(string.Format($"{m_CdnURL}/{VersionDefines.PATCH_FOLDER}/{Utility.GetPlatformName()}/{m_Backdoor.CurVersion}/{VersionDefines.DIFFCOLLECTION_FILENAME}"));
            info.dstURL = localDiffCollectionURL;
            info.verifiedHash = hash;
            info.retryCount = 3;
            yield return m_SingleFileTask.Run(info);

            m_DiffCollection = null;
            if (string.IsNullOrEmpty(m_SingleFileTask.error))
            {
                m_DiffCollection = DiffCollection.Deserialize(localDiffCollectionURL);
            }
            else
            {
                m_Error = string.Format($"Occurs fetal error when download diffcollection.json from {info.srcUri}. {m_SingleFileTask.error}");
            }
        }

        private IEnumerator DownloadDiff()
        {
            string hash = m_DiffCollection.GetDiffFileHash(localCurVersion.ToString());
            if (string.IsNullOrEmpty(hash))
            {
                m_Error = string.Format($"can't find the version [{localCurVersion.ToString()}] hash of diff.json, plz check diffcollection.json's VersionHashMap");
                yield break;
            }

            string localDiffURL = string.Format($"{Application.persistentDataPath}/{VersionDefines.DIFF_FILENAME}");
            if (File.Exists(localDiffURL))
                File.Delete(localDiffURL);

            DownloadTaskInfo info = new DownloadTaskInfo();
            string diffURL = string.Format($"{m_CdnURL}/{VersionDefines.PATCH_FOLDER}/{Utility.GetPlatformName()}/{m_Backdoor.CurVersion}/{localCurVersion.ToString()}/{VersionDefines.DIFF_FILENAME}");
            info.srcUri = new Uri(diffURL);
            info.dstURL = localDiffURL;
            info.verifiedHash = hash;
            info.retryCount = 3;
            yield return m_SingleFileTask.Run(info);

            m_Diff = null;
            if (string.IsNullOrEmpty(m_SingleFileTask.error))
            {
                m_Diff = Diff.Deserialize(localDiffURL);
            }
            else
            {
                m_Error = string.Format($"Occurs fetal error when download diff.json from {info.srcUri}. {m_SingleFileTask.error}");
            }
        }        

        private IEnumerator Downloading()
        {
            string srcUriPrefix = string.Format($"{m_CdnURL}/{VersionDefines.PATCH_FOLDER}/{Utility.GetPlatformName()}/{remoteCurVersion.ToString()}/{localCurVersion.ToString()}");
            string dstURLPrefix = string.Format($"{Application.persistentDataPath}/{Utility.GetPlatformName()}");
            while(true)
            {
                foreach(var task in m_TaskWorkerList)
                {
                    // 遇到error不再执行后续操作，但已执行的操作不中断
                    if (!string.IsNullOrEmpty(m_Error))
                        break;

                    if (task.isRunning)
                        continue;

                    // dispatch the downloading task
                    Diff.DiffFileInfo fileInfo = GetDiffFileInfo();
                    if (fileInfo == null)
                        continue;

                    DownloadTaskInfo info   = new DownloadTaskInfo();
                    info.srcUri             = new System.Uri(string.Format($"{srcUriPrefix}/{fileInfo.BundleName}"));
                    info.dstURL             = string.Format($"{dstURLPrefix}/{fileInfo.BundleName}");
                    info.verifiedHash       = fileInfo.FileHash;
                    info.retryCount         = 3;
                    info.onProgress         = OnTaskProgress;
                    info.onCompleted        = OnTaskCompleted;
                    info.onRequestError     = OnRequestError;
                    info.onDownloadError    = OnDownloadError;
                    StartCoroutine(task.Run(info));
                }

                // 只要仍有任务在运行就等待，即使遇到error
                if (IsStillWorking())
                {
                    yield return null;
                }
                else
                {
                    break;
                }
            }            
        }
        
        private bool IsStillWorking()
        {
            foreach (var task in m_TaskWorkerList)
            {
                if (task.isRunning)
                    return true;
            }
            return false;
        }

        private long Prepare()
        {
            CollectPendingDownloadFileList();

            // init task workers and buffer
            int workerCount = Mathf.Min(m_DownloadFileList.Count, m_WorkerCount);
            for (int i = m_CachedBufferList.Count; i < workerCount; ++i)
            {
                m_CachedBufferList.Add(new byte[m_BufferSize]);
            }
            m_TaskWorkerList = new List<DownloadTask>(workerCount);
            for (int i = 0; i < workerCount; ++i)
            {
                m_TaskWorkerList.Add(new DownloadTask(m_CachedBufferList[i]));
            }

            long size = 0;
            foreach(var file in m_DownloadFileList)
            {
                size += file.Size;
            }
            return size;
        }

        private void DownloadingFinished()
        {
            if(string.IsNullOrEmpty(m_Error))
            {
                MarkLatestVersion();
            }
            else
            {
                Debug.LogError($"patch failed...{m_Error}");
            }

            m_Listener?.OnPatchCompleted(m_Error);
        }

        private void MarkLatestVersion()
        {
            PlayerPrefs.SetString(VersionDefines.CUR_APPVERSION, m_Backdoor.CurVersion);
            PlayerPrefs.Save();

            Debug.Log($"patch completed...{localCurVersion.ToString()}  ->  {m_Backdoor.CurVersion}");
            localCurVersion.Set(m_Backdoor.CurVersion);
        }

        private void OnTaskProgress(DownloadTaskInfo taskInfo, ulong downedLength, ulong totalLength, float downloadSpeed)
        {
            //Debug.Log($"OnProgress: {Path.GetFileName(taskInfo.dstURL)}     {downedLength}/{totalLength}    downloadSpeed({downloadSpeed})");

            m_Listener?.OnFileDownloadProgress(taskInfo.dstURL, downedLength, totalLength, downloadSpeed);
        }

        private void OnTaskCompleted(DownloadTaskInfo taskInfo, bool success, int tryCount)
        {
            if (!success)
            {
                m_Error = string.Format($"OnCompleted: failed to download {taskInfo.srcUri}");
            }
            //Debug.Log($"下载：{taskInfo.dstURL} {(success ? "成功" : "失败")}");
            m_Listener?.OnFileDownloadCompleted(taskInfo.dstURL, success);
        }

        private void OnRequestError(DownloadTaskInfo taskInfo, string error)
        {
            m_Error = string.Format($"OnRequestError: {error} : {taskInfo.srcUri}");
        }

        private void OnDownloadError(DownloadTaskInfo taskInfo, string error)
        {
            m_Error = string.Format($"OnDownloadError: {error} : {taskInfo.srcUri}");
        }        

        private void CollectPendingDownloadFileList()
        {
            m_PendingDownloadFileIndex = 0;

            // 考虑到断点续传，总是所有补丁数据检查一遍
            m_DownloadFileList.Clear();
            foreach (var dfi in m_Diff.AddedFileList)
            {
                string path = string.Format($"{Application.persistentDataPath}/{Utility.GetPlatformName()}/{dfi.BundleName}");
                if(!File.Exists(path))
                {
                    m_DownloadFileList.Add(dfi);
                    continue;
                }

                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    string hash = EasyMD5.Hash(fs);
                    if(string.Compare(hash, dfi.FileHash) != 0)
                    {
                        m_DownloadFileList.Add(dfi);
                        continue;
                    }
                }
            }

            foreach (var dfi in m_Diff.UpdatedFileList)
            {
                string path = string.Format($"{Application.persistentDataPath}/{Utility.GetPlatformName()}/{dfi.BundleName}");
                if (!File.Exists(path))
                {
                    m_DownloadFileList.Add(dfi);
                    continue;
                }

                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    string hash = EasyMD5.Hash(fs);
                    if (string.Compare(hash, dfi.FileHash) != 0)
                    {
                        m_DownloadFileList.Add(dfi);
                        continue;
                    }
                }
            }
        }

        private Diff.DiffFileInfo GetDiffFileInfo()
        {
            if (m_PendingDownloadFileIndex < 0 || m_PendingDownloadFileIndex >= m_DownloadFileList.Count)
                return null;
            return m_DownloadFileList[m_PendingDownloadFileIndex++];
        }
    }

    public interface IPatcherListener
    {
        bool OnError_DownloadBackdoor(string error, Backdoor backdoor);
        void OnCheck_IsLatestVersion(bool isLatestVersion);
        void OnError_DownloadDiffCollection(string error);
        void OnError_DownloadDiff(string error);
        void Prepare(int count, long size);
        void OnPatchCompleted(string error);
        void OnFileDownloadProgress(string filename, ulong downedLength, ulong totalLength, float downloadSpeed);
        void OnFileDownloadCompleted(string filename, bool success);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Patcher))]
    public class Patcher_Inspector : Editor
    {
        public override void OnInspectorGUI()
        {
            string curVersion = PlayerPrefs.GetString(VersionDefines.CUR_APPVERSION);
            EditorGUILayout.LabelField("Cur Version", string.IsNullOrEmpty(curVersion) ? "None" : curVersion);

            if(GUILayout.Button("Clear Version"))
            {
                if (PlayerPrefs.HasKey(VersionDefines.CUR_APPVERSION))
                    PlayerPrefs.DeleteKey(VersionDefines.CUR_APPVERSION);
            }
        }
    }
#endif
}