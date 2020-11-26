using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Framework.Core
{
    public class Extracter : MonoBehaviour
    {
        static public string                FILELIST_PATH               = "Assets/Resources";
        static public string                FILELIST_NAME               = "FileList.bytes";

        [Range(1, 10)]
        public int                          workerCount                 = 3;
        private List<ExtractTask>           m_TaskWorkerList;
        private List<byte[]>                m_CachedBufferList;
        private const int                   m_BufferSize                = 1024 * 1;

        private AppVersion                  m_Version;
        private BundleFileList              m_BundleFileList;
        private int                         m_PendingExtracedFileIndex;
        private List<BundleFileInfo>        m_PendingExtractedFileList  = new List<BundleFileInfo>();
        private string                      m_Error;

        private Coroutine                   m_Coroutine;

        private void OnEnable()
        {
            if (Init())
                StartWork();
        }

        private void OnDisable()
        {
            Uninit();
        }

        private bool Init()
        {
            // load appVersion
            m_Version = AppVersion.Load();
            if (m_Version == null)
            {
                Debug.LogError("Extracter: AppVersion == null");
                return false;
            }

            // load FileList
            TextAsset asset = Resources.Load<TextAsset>(string.Format($"{Utility.GetPlatformName()}/{Path.GetFileNameWithoutExtension(FILELIST_NAME)}"));
            if (asset == null || asset.text == null)
            {
                Debug.LogError($"FileList not found.    {FILELIST_PATH}/{FILELIST_NAME}");
                return false;
            }
            m_BundleFileList = BundleFileList.DeserializeFromJson(asset.text);

            // init task workers and buffer
            m_CachedBufferList = new List<byte[]>(workerCount);
            for (int i = 0; i < workerCount; ++i)
            {
                m_CachedBufferList.Add(new byte[m_BufferSize]);
            }
            m_TaskWorkerList = new List<ExtractTask>(workerCount);
            for(int i = 0; i < workerCount; ++i)
            {
                m_TaskWorkerList.Add(new ExtractTask(m_CachedBufferList[i]));
            }
            return true;
        }

        private void Uninit()
        {
            if (m_Coroutine != null)
                StopCoroutine(m_Coroutine);

            if (m_Version != null)
                AppVersion.Unload(m_Version);

            m_BundleFileList = null;

            for (int i = 0; i < m_TaskWorkerList.Count; ++i)
            {
                m_TaskWorkerList[i].Dispose();
            }
            m_TaskWorkerList.Clear();
        }

        private void StartWork()
        {
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

                FileStream stream = new FileStream(filePath, FileMode.Open);
                if(!EasyMD5.Verify(stream, bfi.FileHash))
                {
                    m_PendingExtractedFileList.Add(bfi);
                }
                stream.Close();
                stream.Dispose();
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

                    // 获取尚未提取的文件
                    BundleFileInfo fileInfo = GetPendingExtractedFile();
                    if (fileInfo == null)
                    {
                        continue;
                    }

                    Debug.Log($"==========NEW FILE TO BE EXTRACTING: {fileInfo.BundleName}    frame: {Time.frameCount}");

                    // begin to extract file
                    ExtractTaskInfo info    = new ExtractTaskInfo();
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

                if (IsStillWorking() && string.IsNullOrEmpty(m_Error))
                {
                    yield return null;
                }
                else
                {
                    break;
                }
            }
            OnExtractEnd();
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
            Debug.Log("OnExtractBegin");
        }

        private void OnExtractEnd()
        {
            Debug.Log("OnExtractEnd");
        }

        private void OnProgress(ExtractTaskInfo data, ulong downedLength, ulong totalLength, float downloadSpeed)
        {
            Debug.Log($"{Path.GetFileName(data.dstURL)}     {downedLength}/{totalLength}    downloadSpeed({downloadSpeed})");
        }

        private void OnCompleted(ExtractTaskInfo data, bool success, int tryCount)
        {
            if (!success)
            {
                m_Error = string.Format($"OnCompleted--- failed to download {data.srcUri.ToString()}");
            }

            //Debug.Log($"下载：{data.dstURL} {(success ? "成功" : "失败")}");
        }

        private void OnRequestError(ExtractTaskInfo data, string error)
        {
            m_Error = string.Format($"OnRequestError--- {error} : {data.srcUri.ToString()}");
        }

        private void OnDownloadError(ExtractTaskInfo data, string error)
        {
            m_Error = string.Format($"OnDownloadError--- {error} : {data.srcUri.ToString()}");
        }
    }
}