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
        static public readonly string       CUR_APPVERSION              = "CurAppVersion_fe2679cf89a145ccb45b715568e6bc07";
        static public readonly string       DIFFCOLLECTION_FILENAME     = "diffcollection.json";
        static public readonly string       DIFF_FILENAME               = "diff.json";

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
        private string                      m_CurVersion;               // 当前版本号（记录在PlayerPrefs）

        private string                      m_CdnURL;
        private string                      m_Error;

        private IPatcherListener            m_Listener;

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
            if(!string.IsNullOrEmpty(m_Error))
            {
                // Debug.LogError(m_Error);
                m_Listener?.OnError_DownloadBackdoor(m_Error);
                yield break;
            }

            // step2. check that the current version is up to date
            if (IsLatestVersion())
            {
                m_Listener?.OnCheck_IsLatestVersion(true);
                yield break;
            }
            m_Listener?.OnCheck_IsLatestVersion(false);

            // step3. download the diff collection of the latest version
            yield return StartCoroutine(DownloadDiffCollection());
            if (!string.IsNullOrEmpty(m_Error))
            {
                // Debug.LogError(m_Error);
                m_Listener?.OnError_DownloadDiffCollection(m_Error);
                yield break;
            }

            // step4. download the diff.json for upgrade from local version to latest version
            yield return StartCoroutine(DownloadDiff());
            if (!string.IsNullOrEmpty(m_Error))
            {
                // Debug.LogError(m_Error);
                m_Listener?.OnError_DownloadDiff(m_Error);
                yield break;
            }
            
            // prepare for downloading
            if(!Prepare())
                yield break;

            // step5. downloading...
            yield return StartCoroutine(Downloading());
        }

        private IEnumerator DownloadBackdoor()
        {
            string localBackdoorURL = string.Format($"{Application.persistentDataPath}/backdoor.json");
            if (File.Exists(localBackdoorURL))
                File.Delete(localBackdoorURL);

            DownloadTaskInfo info = new DownloadTaskInfo();
            info.srcUri = new Uri(string.Format($"{m_CdnURL}/backdoor.json"));
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
            string localCurVersion = GetLocalCurVersion();

            AppVersion remoteCurVersion = AppVersion.CreateInstance<AppVersion>();
            remoteCurVersion.Set(m_Backdoor.CurVersion);

            return !string.IsNullOrEmpty(localCurVersion) && remoteCurVersion.CompareTo(localCurVersion) == 0;
        }

        /// <summary>
        /// 获取本地当前版本号
        /// </summary>
        /// <returns></returns>
        private string GetLocalCurVersion()
        {
            if (string.IsNullOrEmpty(m_CurVersion))
            {
                m_CurVersion = PlayerPrefs.GetString(CUR_APPVERSION);
                if (string.IsNullOrEmpty(m_CurVersion))
                { // 首次安装，尚未下载过会没有CUR_APPVERSION标记，则使用母包的版本号
                    AppVersion version = AppVersion.Load();
                    m_CurVersion = version.ToString();
                    AppVersion.Unload(version);
                }

                if (!string.IsNullOrEmpty(AppVersion.Check(m_CurVersion)))
                {
                    m_CurVersion = null;
                    throw new ArgumentNullException($"GetLocalCurVersion: {m_CurVersion} is not standard");
                }
            }
            return m_CurVersion;
        }

        private IEnumerator DownloadDiffCollection()
        {
            string hash = m_Backdoor.GetDiffCollectionFileHash(m_Backdoor.CurVersion);
            if (string.IsNullOrEmpty(hash))
            {
                m_Error = string.Format($"can't find the hash of diffcollection.json, plz check backdoor's version history.  CurVersion is [{m_Backdoor.CurVersion}]");
                yield break;
            }

            string localDiffCollectionURL = string.Format($"{Application.persistentDataPath}/{DIFFCOLLECTION_FILENAME}");
            if (File.Exists(localDiffCollectionURL))
                File.Delete(localDiffCollectionURL);

            DownloadTaskInfo info = new DownloadTaskInfo();
            info.srcUri = new Uri(string.Format($"{m_CdnURL}/patch/{Utility.GetPlatformName()}/{m_Backdoor.CurVersion}/{DIFFCOLLECTION_FILENAME}"));
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
            string hash = m_DiffCollection.GetDiffFileHash(GetLocalCurVersion());
            if (string.IsNullOrEmpty(hash))
            {
                m_Error = string.Format($"can't find the hash of diff.json, plz check diffcollection.json's VersionHashMap");
                yield break;
            }

            string localDiffURL = string.Format($"{Application.persistentDataPath}/{DIFF_FILENAME}");
            if (File.Exists(localDiffURL))
                File.Delete(localDiffURL);

            DownloadTaskInfo info = new DownloadTaskInfo();
            string diffURL = Path.Combine(m_CdnURL, "patch", Utility.GetPlatformName(), m_Backdoor.CurVersion, GetLocalCurVersion(), DIFF_FILENAME);
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
            while(true)
            {
                foreach(var task in m_TaskWorkerList)
                {
                    if (task.isRunning)
                        continue;

                    // 遇到error不再执行后续操作，但已执行的操作不中断
                    if (!string.IsNullOrEmpty(m_Error))
                        continue;

                    // dispatch the downloading task
                    Diff.DiffFileInfo fileInfo = GetDiffFileInfo();
                    if (fileInfo == null)
                        continue;

                    DownloadTaskInfo info   = new DownloadTaskInfo();
                    info.srcUri             = new System.Uri(Path.Combine(m_CdnURL, "patch", Utility.GetPlatformName(), m_Backdoor.CurVersion, m_CurVersion, fileInfo.BundleName));
                    info.dstURL             = string.Format($"{Application.persistentDataPath}/{Utility.GetPlatformName()}/{fileInfo.BundleName}");
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
            DownloadingFinished();
        }

        private bool Prepare()
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

            return m_Listener != null ? m_Listener.Prepare(m_DownloadFileList.Count, m_Diff.Size) : false;
        }

        private void DownloadingFinished()
        {
            if(string.IsNullOrEmpty(m_Error))
            {
                PlayerPrefs.SetString(CUR_APPVERSION, m_Backdoor.CurVersion);
                PlayerPrefs.Save();

                Debug.Log($"patch completed...{m_CurVersion}  ->  {m_Backdoor.CurVersion}");
                m_CurVersion = m_Backdoor.CurVersion;
            }
            else
            {
                //Debug.LogWarning($"patch failed...{m_Error}");
            }

            m_Listener?.OnPatchCompleted(m_Error);
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

        private bool IsStillWorking()
        {
            foreach (var task in m_TaskWorkerList)
            {
                if (task.isRunning)
                    return true;
            }
            return false;
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
        void OnError_DownloadBackdoor(string error);
        void OnCheck_IsLatestVersion(bool isLatestVersion);
        void OnError_DownloadDiffCollection(string error);
        void OnError_DownloadDiff(string error);
        bool Prepare(int count, long size);     // true: 开始下载；false：取消下载
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
            string curVersion = PlayerPrefs.GetString(Patcher.CUR_APPVERSION);
            EditorGUILayout.LabelField("Cur Version", string.IsNullOrEmpty(curVersion) ? "None" : curVersion);

            if(GUILayout.Button("Clear Version"))
            {
                if (PlayerPrefs.HasKey(Patcher.CUR_APPVERSION))
                    PlayerPrefs.DeleteKey(Patcher.CUR_APPVERSION);
            }
        }
    }
#endif
}