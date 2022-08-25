using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Framework.Core
{
    public class ExtraDownloader : MonoBehaviour
    {
        private int                         m_WorkerCount               = 5;
        private List<DownloadTask>          m_TaskWorkerList;
        private List<byte[]>                m_CachedBufferList          = new List<byte[]>();
        private const int                   m_BufferSize                = 1024 * 1024;

        private AppVersion                  m_BaseVersion;
        private TextAsset                   m_BundleFileListRawData;
        private BundleFileList              m_BundleFileList;
        private int                         m_PendingExtracedFileIndex;
        private List<BundleFileInfo>        m_PendingExtractedFileList  = new List<BundleFileInfo>();
        private string                      m_Error;

        private float                       m_BeginTime;
        private IExtraListener              m_Listener;

        public void StartWork(int workerCount, IExtraListener listener = null)
        {
            m_Listener = listener;
            m_WorkerCount = workerCount;

            // init
            //Uninit();
            //if (!Init())
            //{
            //    Uninit();
            //    return;
            //}

            if (!ShouldDownload())
                return;
            
            StartCoroutine(Run());
        }

        /// <summary>
        /// �ж��Ƿ���Ҫ�������ػ�������˶�������
        /// <summary>
        private bool ShouldDownload()
        {
            string flag = PlayerPrefs.GetString(VersionDefines.EXTRA_APPVERSION);
            bool bShould = !(flag == "true");
            
            m_Listener?.OnShouldExtract(ref bShould);
            return bShould;
        }

        /// <summary>
        /// load the appVersion and FileList for bundle extracting
        /// <summary>
        private bool Init()
        {
            bool bInit = true;

            // load appVersion
            m_BaseVersion = AppVersion.Load();
            if (m_BaseVersion == null)
            {
                Debug.LogError("Extracter: AppVersion == null");
                bInit = false;
            }

            m_Listener?.OnInit(bInit);
            return bInit;
        }

        private void Uninit()
        {
            //if (m_BaseVersion != null)
            //{
            //    AppVersion.Unload(m_BaseVersion);
            //    m_BaseVersion = null;
            //}

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

        private void Prepare()
        {
            // generate pending extracted file list
            CollectPendingExtractedFileList();

            // init task workers and buffer
            int workerCount = Mathf.Min(m_PendingExtractedFileList.Count, m_WorkerCount);
            for (int i = m_CachedBufferList.Count; i < workerCount; ++i)
            {
                m_CachedBufferList.Add(new byte[m_BufferSize]);
            }
            m_TaskWorkerList = new List<DownloadTask>(workerCount);
            for (int i = 0; i < workerCount; ++i)
            {
                m_TaskWorkerList.Add(new DownloadTask(m_CachedBufferList[i]));
            }
        }

        private bool LoadExtraFileList()
        {
            // load FileList
            m_BundleFileListRawData = Resources.Load<TextAsset>(string.Format($"{Utility.GetPlatformName()}/{Path.GetFileNameWithoutExtension(VersionDefines.EXTRA_FILELIST_NAME)}"));
            if (m_BundleFileListRawData == null || m_BundleFileListRawData.text == null)
            {
                Debug.LogError($"ExtraFileList not found.    {VersionDefines.EXTRA_FILELIST_PATH}/{VersionDefines.EXTRA_FILELIST_NAME}");
                return false;
            }
            else
            {
                m_BundleFileList = BundleFileList.DeserializeFromJson(m_BundleFileListRawData.text);
            }
            return true;
        }

        /// <summary>
        /// ������Ҫ��ȡ���ļ��б�
        /// </summary>
        private void CollectPendingExtractedFileList()
        {
            if (!LoadExtraFileList())
                return;

            m_PendingExtracedFileIndex = 0;

            // �в����ڻ�hash��ƥ����ļ�
            m_PendingExtractedFileList.Clear();
            foreach (var bfi in m_BundleFileList.FileList)
            {
                string filePath = string.Format($"{Application.persistentDataPath}/{Utility.GetPlatformName()}/{bfi.BundleName}");
                if (!File.Exists(filePath))
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
            string srcUriPrefix = string.Format($"{Application.streamingAssetsPath}/{Utility.GetPlatformName()}");
            string dstURLPrefix = string.Format($"{Application.persistentDataPath}/{Utility.GetPlatformName()}");
            while (true)
            {
                foreach (var task in m_TaskWorkerList)
                {
                    // ����error����ִ�к�������������ִ�еĲ������ж�
                    if (!string.IsNullOrEmpty(m_Error))
                        break;

                    if (task.isRunning)
                    {
                        continue;
                    }

                    // ��ȡ��δ��ȡ���ļ�
                    BundleFileInfo fileInfo = GetPendingExtractedFile();
                    if (fileInfo == null)
                    {
                        continue;
                    }

                    //Debug.Log($"==========NEW FILE TO BE EXTRACTING: {fileInfo.BundleName}    frame: {Time.frameCount}");

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

                // ֻҪ�������������о͵ȴ�����ʹ����error
                if (IsStillWorking())
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
            foreach (var task in m_TaskWorkerList)
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
            LoadExtraFileList();

            //Debug.Log("Begin to extract bundle file list");
            Prepare();
            m_BeginTime = Time.time;
            m_Error = null;

            m_Listener?.OnBegin(m_PendingExtractedFileList.Count);
        }

        private void OnExtractEnd(string error)
        {
            // ��ȡ��ɺ���ϱ�ǩ
            if (string.IsNullOrEmpty(error))
            {
                PlayerPrefs.SetString(VersionDefines.BASE_APPVERSION, m_BaseVersion.ToString());       // ��¼ĸ���汾�ţ���λ��
                PlayerPrefs.Save();
                Debug.Log($"BundleExtracter: asset extracted finished, mark the version {m_BaseVersion.ToString()}");
            }

            Uninit();
            m_Listener?.OnEnd(Time.time - m_BeginTime, error);

            //Debug.Log($"End to extract bundle file list: {(string.IsNullOrEmpty(error) ? "success" : "failed")}   {Time.time - m_BeginTime}");
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

            //Debug.Log($"���أ�{taskInfo.dstURL} {(success ? "�ɹ�" : "ʧ��")}");
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
        void OnInit(bool success);                                                                          // ��ȡ���ݵ�׼�������Ƿ���ɣ�ʧ�ܱ�ʾ���ڲ���������
        void OnShouldExtract(ref bool shouldExtract);                                                       // ���ݱ�ǩ�ж��Ƿ���Ҫ��ȡ����
        void OnBegin(int countOfFiles);                                                                     // ��ʼ��ȡ����
        void OnEnd(float elapsedTime, string error);                                                        // ��ȡ����
        void OnFileCompleted(string filename, bool success);                                                // һ��������ȡ���֪ͨ�����ܳɹ���Ҳ����ʧ�ܣ�
        void OnFileProgress(string filename, ulong downedLength, ulong totalLength, float downloadSpeed);   // ÿ��������ȡ����֪ͨ
    }
}