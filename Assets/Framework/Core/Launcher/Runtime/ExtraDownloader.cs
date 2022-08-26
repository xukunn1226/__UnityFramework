using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.Core
{
    public class ExtraDownloader : MonoBehaviour
    {
        private int                         m_WorkerCount               = 5;
        private List<DownloadTask>          m_TaskWorkerList;
        private List<byte[]>                m_CachedBufferList          = new List<byte[]>();
        private const int                   m_BufferSize                = 1024 * 1024;

        private TextAsset                   m_BundleFileListRawData;
        private BundleFileList              m_BundleFileList;
        private int                         m_PendingDownloadedFileIndex;
        private List<BundleFileInfo>        m_PendingDownloadedFileList = new List<BundleFileInfo>();
        private string                      m_Error;
        private string                      m_CdnURL;
        private IExtraListener              m_Listener;
        private AppVersion                  m_AppVersion;               // 当前引擎版本号，记录在Resources/

        public AppVersion localBaseVersion
        {
            get
            {
                if (m_AppVersion == null)
                {
                    m_AppVersion = AppVersion.CreateInstance<AppVersion>();
                    AppVersion version = AppVersion.Load();
                    m_AppVersion.Set(version.ToString());
                    AppVersion.Unload(version);
                }
                return m_AppVersion;
            }
        }

        public void StartWork(string cdnURL, int workerCount, IExtraListener listener = null)
        {
            m_CdnURL = cdnURL;
            m_Listener = listener;
            m_WorkerCount = workerCount;

            if (!ShouldDownload())
                return;
            
            StartCoroutine(Run());
        }

        /// <summary>
        /// 判断是否需要二次下载或已完成了二次下载
        /// <summary>
        private bool ShouldDownload()
        {
            string flag = PlayerPrefs.GetString(VersionDefines.EXTRA_APPVERSION);
            bool bShould = flag != "true";
            
            m_Listener?.OnShouldDownloadExtra(ref bShould);
            return bShould;
        }

        private IEnumerator Run()
        {
            OnDownloadExtraBegin();
            string srcUriPrefix = string.Format($"{m_CdnURL}/{VersionDefines.DEPLOYMENT_EXTRA_DATA_FOLDER}/{Utility.GetPlatformName()}/{localBaseVersion.ToString()}");
            string dstURLPrefix = string.Format($"{Application.persistentDataPath}/{Utility.GetPlatformName()}");
            while (true)
            {
                foreach (var task in m_TaskWorkerList)
                {
                    // 遇到error不再执行后续操作，但已执行的操作不中断
                    if (!string.IsNullOrEmpty(m_Error))
                        break;

                    if (task.isRunning)
                    {
                        continue;
                    }

                    // 获取尚未提取的文件
                    BundleFileInfo fileInfo = GetPendingExtractedFile();
                    if (fileInfo == null)
                    {
                        continue;
                    }

                    // begin to extract file
                    DownloadTaskInfo info = new DownloadTaskInfo();
                    info.srcUri = new System.Uri(string.Format($"{srcUriPrefix}/{fileInfo.BundleName}"));
                    info.dstURL = string.Format($"{dstURLPrefix}/{fileInfo.BundleName}");
                    info.verifiedHash = fileInfo.FileHash;
                    info.retryCount = 3;
                    info.onProgress = OnProgress;
                    info.onCompleted = OnCompleted;
                    info.onRequestError = OnRequestError;
                    info.onDownloadError = OnDownloadError;
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
            OnDownloadExtraEnd(m_Error);
        }

        private void OnDownloadExtraBegin()
        {
            LoadExtraFileList();

            long size = Prepare();
            m_Error = null;

            m_Listener?.OnDownloadExtraBegin(m_PendingDownloadedFileList.Count, size);
        }

        private void OnDownloadExtraEnd(string error)
        {
            // 提取完成后打上标签
            if (string.IsNullOrEmpty(error))
            {
                PlayerPrefs.SetString(VersionDefines.EXTRA_APPVERSION, "true");
                PlayerPrefs.Save();
                Debug.Log($"Download Extra Data: download completed, mark the flag is true");
            }

            Uninit();
            m_Listener?.OnDownloadExtraEnd(error);
        }

        private bool LoadExtraFileList()
        {
            // load FileList
            m_BundleFileListRawData = Resources.Load<TextAsset>(string.Format($"{Utility.GetPlatformName()}/{Path.GetFileNameWithoutExtension(VersionDefines.EXTRA_FILELIST_NAME)}"));
            if (m_BundleFileListRawData == null || m_BundleFileListRawData.text == null)
            { // 没有配置二次下载的数据时，可能加载不到ExtraFileList
                //Debug.LogError($"ExtraFileList not found.    {VersionDefines.EXTRA_FILELIST_PATH}/{VersionDefines.EXTRA_FILELIST_NAME}");
                return false;
            }
            else
            {
                m_BundleFileList = BundleFileList.DeserializeFromJson(m_BundleFileListRawData.text);
            }
            return true;
        }

        private void Uninit()
        {
            if (m_BundleFileListRawData != null)
            {
                Resources.UnloadAsset(m_BundleFileListRawData);
                m_BundleFileListRawData = null;
            }
            m_BundleFileList = null;

            if (m_TaskWorkerList != null)
            {
                for (int i = 0; i < m_TaskWorkerList.Count; ++i)
                {
                    m_TaskWorkerList[i].Dispose();
                }
                m_TaskWorkerList.Clear();
            }
            m_CachedBufferList.Clear();
        }        

        private long Prepare()
        {
            // generate pending downloaded file list
            CollectPendingDownloadedFileList();

            // init task workers and buffer
            int workerCount = Mathf.Min(m_PendingDownloadedFileList.Count, m_WorkerCount);
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
            foreach(var file in m_PendingDownloadedFileList)
            {
                size += file.Size;
            }
            return size;
        }

        /// <summary>
        /// 生成需要提取的文件列表
        /// </summary>
        private void CollectPendingDownloadedFileList()
        {
            if (m_BundleFileList == null)
                return;

            m_PendingDownloadedFileIndex = 0;

            // 尚不存在或hash不匹配的文件
            m_PendingDownloadedFileList.Clear();
            foreach (var bfi in m_BundleFileList.FileList)
            {
                string filePath = string.Format($"{Application.persistentDataPath}/{Utility.GetPlatformName()}/{bfi.BundleName}");
                if (!File.Exists(filePath))
                {
                    m_PendingDownloadedFileList.Add(bfi);
                    continue;
                }

                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    if (!EasyMD5.Verify(fs, bfi.FileHash))
                    {
                        m_PendingDownloadedFileList.Add(bfi);
                    }
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

        private BundleFileInfo GetPendingExtractedFile()
        {
            if (m_PendingDownloadedFileIndex < 0 || m_PendingDownloadedFileIndex >= m_PendingDownloadedFileList.Count)
                return null;
            return m_PendingDownloadedFileList[m_PendingDownloadedFileIndex++];
        }

        private void OnProgress(DownloadTaskInfo taskInfo, ulong downedLength, ulong totalLength, float downloadSpeed)
        {
            m_Listener?.OnFileProgress(Path.GetFileName(taskInfo.dstURL), downedLength, totalLength, downloadSpeed);
            //Debug.Log($"OnProgress: {Path.GetFileName(taskInfo.dstURL)}     {downedLength}/{totalLength}    downloadSpeed({downloadSpeed})");
        }

        private void OnCompleted(DownloadTaskInfo taskInfo, bool success, int tryCount)
        {
            if (!success)
            {
                m_Error = string.Format($"OnCompleted: failed to download {taskInfo.srcUri}");
            }
            m_Listener?.OnFileCompleted(Path.GetFileName(taskInfo.dstURL), success);

            //Debug.Log($"下载：{taskInfo.dstURL} {(success ? "成功" : "失败")}");
        }

        private void OnRequestError(DownloadTaskInfo taskInfo, string error)
        {
            m_Error = string.Format($"OnRequestError: {error} : {taskInfo.srcUri}");
        }

        private void OnDownloadError(DownloadTaskInfo taskInfo, string error)
        {
            m_Error = string.Format($"OnDownloadError: {error} : {taskInfo.srcUri}");
        }
    }

    public interface IExtraListener
    {
        void OnShouldDownloadExtra(ref bool shouldDownload);                                                // 是否需要进行二次下载
        void OnDownloadExtraBegin(int countOfFiles, long size);                                             // 开始下载数据
        void OnDownloadExtraEnd(string error);                                                              // 下载结束
        void OnFileCompleted(string filename, bool success);                                                // 一个数据提取完成通知（可能成功，也可能失败）
        void OnFileProgress(string filename, ulong downedLength, ulong totalLength, float downloadSpeed);   // 每个数据提取进度通知
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ExtraDownloader))]
    public class ExtraDownloader_Inspector : Editor
    {
        public override void OnInspectorGUI()
        {
            string flag = PlayerPrefs.GetString(VersionDefines.EXTRA_APPVERSION);
            EditorGUILayout.LabelField("Extra Flag", string.IsNullOrEmpty(flag) ? "None" : flag);

            if (GUILayout.Button("Clear Flag"))
            {
                if (PlayerPrefs.HasKey(VersionDefines.EXTRA_APPVERSION))
                    PlayerPrefs.DeleteKey(VersionDefines.EXTRA_APPVERSION);
            }
        }
    }
#endif
}