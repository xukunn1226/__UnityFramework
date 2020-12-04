using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Framework.Core
{
    public class BundleExtracter : MonoBehaviour
    {
        static public string                FILELIST_PATH               = "Assets/Resources";
        static public string                FILELIST_NAME               = "FileList.bytes";
        static private string               BASE_APPVERSION             = "BaseAppVersion_9695e71e3a224b408c39c7a75c0fa376";

        [Range(1, 10)]
        public int                          workerCount                 = 3;
        private List<DownloadTask>           m_TaskWorkerList;
        private List<byte[]>                m_CachedBufferList;
        private const int                   m_BufferSize                = 1024 * 1024;

        private AppVersion                  m_Version;
        private TextAsset                   m_BundleFileListRawData;
        private BundleFileList              m_BundleFileList;
        private int                         m_PendingExtracedFileIndex;
        private List<BundleFileInfo>        m_PendingExtractedFileList  = new List<BundleFileInfo>();
        private string                      m_Error;

        private Coroutine                   m_Coroutine;
        private float                       m_BeginTime;
        private IExtractListener            m_Listener;
        public bool                         AutoStartInEditorMode;      // 仅编辑模式下可自动运行，真机模式需调用StartWork

        private void OnEnable()
        {
//#if UNITY_EDITOR
            if (AutoStartInEditorMode)
                StartWork();
//#endif
        }

        private void OnDisable()
        {
            Uninit();
        }

        public void SetListener(IExtractListener listener)
        {
            m_Listener = listener;
        }

        public void StartWork()
        {
            if (Init() && ShouldExtract())
            {
                InternalStartWork();
            }
            else
            {
                Uninit();
            }
        }

        public void Restart()
        {
            Uninit();
            StartWork();
        }

        private bool ShouldExtract()
        {
            bool bShould = m_Version.CompareTo(PlayerPrefs.GetString(BASE_APPVERSION)) != 0;
            m_Listener?.OnShouldExtract(ref bShould);
            return bShould;
        }

        private bool Init()
        {
            bool bInit = true;

            // load appVersion
            m_Version = AppVersion.Load();
            if (m_Version == null)
            {
                Debug.LogError("Extracter: AppVersion == null");
                bInit = false;
            }

            // load FileList
            m_BundleFileListRawData = Resources.Load<TextAsset>(string.Format($"{Utility.GetPlatformName()}/{Path.GetFileNameWithoutExtension(FILELIST_NAME)}"));
            if (m_BundleFileListRawData == null || m_BundleFileListRawData.text == null)
            {
                Debug.LogError($"FileList not found.    {FILELIST_PATH}/{FILELIST_NAME}");
                bInit = false;
            }
            else
            {
                m_BundleFileList = BundleFileList.DeserializeFromJson(m_BundleFileListRawData.text);
            }
            m_Listener?.OnInit(bInit);
            return bInit;
        }

        private void Uninit()
        {
            if (m_Coroutine != null)
                StopCoroutine(m_Coroutine);

            if (m_Version != null)
                AppVersion.Unload(m_Version);

            if (m_BundleFileListRawData != null)
                Resources.UnloadAsset(m_BundleFileListRawData);
            m_BundleFileList = null;

            if (m_TaskWorkerList != null)
            {
                for (int i = 0; i < m_TaskWorkerList.Count; ++i)
                {
                    m_TaskWorkerList[i].Dispose();
                }
                m_TaskWorkerList.Clear();
            }
        }

        private void InternalStartWork()
        {
            // init task workers and buffer
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

            // generate pending extracted file list
            GeneratePendingExtractedFileList();

            m_Coroutine = StartCoroutine(Run());
        }

        /// <summary>
        /// 生成需要提取的文件列表
        /// </summary>
        private void GeneratePendingExtractedFileList()
        {
            m_PendingExtracedFileIndex = 0;

            // 尚不存在或hash不匹配的文件
            m_PendingExtractedFileList.Clear();
            foreach(var bfi in m_BundleFileList.FileList)
            {
                string filePath = string.Format($"{Application.persistentDataPath}/{bfi.AssetPath}");
                if(!File.Exists(filePath))
                {
                    m_PendingExtractedFileList.Add(bfi);
                    continue;
                }

                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    if (!EasyMD5.Verify(fs, bfi.FileHash))
                    {
                        m_PendingExtractedFileList.Add(bfi);
                    }
                }
            }
        }

        private IEnumerator Run()
        {
            OnExtractBegin();
            while (true)
            {
                foreach (var task in m_TaskWorkerList)
                {
                    if (task.isRunning)
                    {
                        continue;
                    }

                    // 遇到error不再执行后续提取
                    if (!string.IsNullOrEmpty(m_Error))
                        continue;

                    // 获取尚未提取的文件
                    BundleFileInfo fileInfo = GetPendingExtractedFile();
                    if (fileInfo == null)
                    {
                        continue;
                    }

                    //Debug.Log($"==========NEW FILE TO BE EXTRACTING: {fileInfo.BundleName}    frame: {Time.frameCount}");

                    // begin to extract file
                    DownloadTaskInfo info    = new DownloadTaskInfo();
                    info.srcUri             = new System.Uri(Path.Combine(Application.streamingAssetsPath, Utility.GetPlatformName(), fileInfo.BundleName));
                    info.dstURL             = string.Format($"{Application.persistentDataPath}/{Utility.GetPlatformName()}/{fileInfo.BundleName}");
                    info.verifiedHash       = fileInfo.FileHash;
                    info.retryCount         = 3;
                    info.onProgress         = OnProgress;
                    info.onCompleted        = OnCompleted;
                    info.onRequestError     = OnRequestError;
                    info.onDownloadError    = OnDownloadError;
                    StartCoroutine(task.Run(info));
                }

                // 只要仍有任务在运行就等待，即使遇到error
                if (IsStillWorking() /*&& string.IsNullOrEmpty(m_Error)*/)
                {
                    yield return null;
                }
                else
                {
                    break;
                }
            }
            OnExtractEnd(m_Error);
        }

        private bool IsStillWorking()
        {
            foreach(var task in m_TaskWorkerList)
            {
                if (task.isRunning)
                    return true;
            }
            return false;
        }

        private BundleFileInfo GetPendingExtractedFile()
        {
            if (m_PendingExtracedFileIndex < 0 || m_PendingExtracedFileIndex >= m_PendingExtractedFileList.Count)
                return null;
            return m_PendingExtractedFileList[m_PendingExtracedFileIndex++];
        }

        private void OnExtractBegin()
        {
            Debug.Log("Begin to extract bundle file list");
            m_BeginTime = Time.time;
            m_Listener?.OnBegin(m_PendingExtractedFileList.Count);
        }

        private void OnExtractEnd(string error)
        {
            // 完全无误的提取完成后打上标签
            if (string.IsNullOrEmpty(error))
                PlayerPrefs.SetString(BASE_APPVERSION, m_Version.ToString());

            Uninit();
            m_Listener?.OnEnd(error);
            
            Debug.Log($"End to extract bundle file list: {(string.IsNullOrEmpty(error) ? "success" : "failed")}   {Time.time - m_BeginTime}");
        }

        private void OnProgress(DownloadTaskInfo data, ulong downedLength, ulong totalLength, float downloadSpeed)
        {
            m_Listener?.OnFileProgress(Path.GetFileName(data.dstURL), downedLength, totalLength, downloadSpeed);
            Debug.Log($"OnProgress: {Path.GetFileName(data.dstURL)}     {downedLength}/{totalLength}    downloadSpeed({downloadSpeed})");
        }

        private void OnCompleted(DownloadTaskInfo data, bool success, int tryCount)
        {
            if (!success)
            {
                m_Error = string.Format($"OnCompleted: failed to download {data.srcUri.ToString()}");
            }
            m_Listener?.OnFileCompleted(Path.GetFileName(data.dstURL), success);

            Debug.Log($"下载：{data.dstURL} {(success ? "成功" : "失败")}");
        }

        private void OnRequestError(DownloadTaskInfo data, string error)
        {
            m_Error = string.Format($"OnRequestError: {error} : {data.srcUri.ToString()}");
        }

        private void OnDownloadError(DownloadTaskInfo data, string error)
        {
            m_Error = string.Format($"OnDownloadError: {error} : {data.srcUri.ToString()}");
        }
    }

    public interface IExtractListener
    {
        void OnInit(bool success);                                                                          // 提取数据的准备工作是否完成
        void OnShouldExtract(ref bool shouldExtract);                                                       // 根据标签判断是否需要提取数据
        void OnBegin(int countOfFiles);                                                                     // 开始提取数据
        void OnEnd(string error);                                                                           // 提取结束
        void OnFileCompleted(string filename, bool success);                                                // 一个数据提取完成通知（可能成功，也可能失败）
        void OnFileProgress(string filename, ulong downedLength, ulong totalLength, float downloadSpeed);   // 每个数据提取进度通知
    }
}