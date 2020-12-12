using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;

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
        static private string               CUR_APPVERSION              = "CurAppVersion_fe2679cf89a145ccb45b715568e6bc07";

        private int                         m_WorkerCount               = 5;
        private List<DownloadTask>          m_TaskWorkerList;
        private List<byte[]>                m_CachedBufferList;
        private const int                   m_BufferSize                = 1024 * 1024;
        private List<Diff.DiffFileInfo>     m_DownloadFileList          = new List<Diff.DiffFileInfo>();

        private DownloadTask                m_SingleFileTask;

        private Backdoor                    m_Backdoor;                 // 服务器下载的backdoor
        private DiffCollection              m_DiffCollection;           // 对应最新版本的diffcollection.json
        private Diff                        m_Diff;                     // 补丁包差异配置数据
        private string                      m_CurVersion;               // 当前版本号（记录在PlayerPrefs）

        private string                      m_CdnURL;
        private string                      m_Error;

        private void Awake()
        {
            m_CdnURL = string.Format($"{Application.dataPath}/../Deployment/CDN");
        }

        private void OnEnable()
        {
            StartWork(m_CdnURL, 5);
        }

        public void StartWork(string cdnURL, int workerCount)
        {
            m_CdnURL = cdnURL;
            m_WorkerCount = workerCount;

            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            m_SingleFileTask = new DownloadTask(new byte[m_BufferSize]);

            // step1. download backdoor config
            yield return StartCoroutine(DownloadBackdoor());
            if(!string.IsNullOrEmpty(m_Error))
            {
                Debug.LogError(m_Error);
                yield break;
            }

            // step2. check that the current version is up to date
            if (IsLatestVersion())
                yield break;

            // step3. download the diff collection of the latest version
            yield return StartCoroutine(DownloadDiffCollection());
            if (!string.IsNullOrEmpty(m_Error))
            {
                Debug.LogError(m_Error);
                yield break;
            }

            // step4. download the diff.json between local version and latest version
            yield return StartCoroutine(DownloadDiff());
            if (!string.IsNullOrEmpty(m_Error))
            {
                Debug.LogError(m_Error);
                yield break;
            }

            // step5. downloading...
            BeginDownload();

            yield return StartCoroutine(Downloading());

            EndDownload();
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
            m_Error = null;
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
                }
            }
            return m_CurVersion;
        }

        private IEnumerator DownloadDiffCollection()
        {
            string hash = m_Backdoor.GetDiffCollectionFileHash(m_Backdoor.CurVersion);
            if (string.IsNullOrEmpty(hash))
            {
                m_Error = string.Format($"can't find the hash of diffcollection.json, plz check backdoor's version history");
                yield break;
            }

            string localDiffCollectionURL = string.Format($"{Application.persistentDataPath}/diffcollection.json");
            if (File.Exists(localDiffCollectionURL))
                File.Delete(localDiffCollectionURL);

            DownloadTaskInfo info = new DownloadTaskInfo();
            info.srcUri = new Uri(string.Format($"{m_CdnURL}/patch/{Utility.GetPlatformName()}/{m_Backdoor.CurVersion}/diffcollection.json"));
            info.dstURL = localDiffCollectionURL;
            info.verifiedHash = hash;
            info.retryCount = 3;
            yield return m_SingleFileTask.Run(info);

            m_DiffCollection = null;
            m_Error = null;
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

            string localDiffURL = string.Format($"{Application.persistentDataPath}/diff.json");
            if (File.Exists(localDiffURL))
                File.Delete(localDiffURL);

            DownloadTaskInfo info = new DownloadTaskInfo();
            string diffURL = Path.Combine(m_CdnURL, "patch", Utility.GetPlatformName(), m_Backdoor.CurVersion, GetLocalCurVersion(), "diff.json");
            info.srcUri = new Uri(diffURL);
            info.dstURL = localDiffURL;
            info.verifiedHash = hash;
            info.retryCount = 3;
            yield return m_SingleFileTask.Run(info);

            m_Diff = null;
            m_Error = null;
            if (string.IsNullOrEmpty(m_SingleFileTask.error))
            {
                m_Diff = Diff.Deserialize(localDiffURL);
            }
            else
            {
                m_Error = string.Format($"Occurs fetal error when download diff.json from {info.srcUri}. {m_SingleFileTask.error}");
            }
        }

        private void BeginDownload()
        {
            CollectPendingDownloadFileList();

            // init task workers and buffer
            int workerCount = Mathf.Min(m_DownloadFileList.Count, m_WorkerCount);
            m_CachedBufferList = new List<byte[]>(workerCount);
            for (int i = 0; i < workerCount; ++i)
            {
                m_CachedBufferList.Add(new byte[m_BufferSize]);
            }
            m_TaskWorkerList = new List<DownloadTask>(workerCount);
            for (int i = 0; i < workerCount; ++i)
            {
                m_TaskWorkerList.Add(new DownloadTask(m_CachedBufferList[i]));
            }
        }

        private IEnumerator Downloading()
        {
            yield break;
        }

        private void EndDownload()
        {

        }

        private void CollectPendingDownloadFileList()
        {
            // 考虑到断点续传，总是所有补丁数据检查一遍
            foreach(var dfi in m_Diff.AddedFileList)
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
    }
}