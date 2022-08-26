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
        private AppVersion                  m_AppVersion;               // ��ǰ����汾�ţ���¼��Resources/

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
        /// �ж��Ƿ���Ҫ�������ػ�������˶�������
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
            // ��ȡ��ɺ���ϱ�ǩ
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
            { // û�����ö������ص�����ʱ�����ܼ��ز���ExtraFileList
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
        /// ������Ҫ��ȡ���ļ��б�
        /// </summary>
        private void CollectPendingDownloadedFileList()
        {
            if (m_BundleFileList == null)
                return;

            m_PendingDownloadedFileIndex = 0;

            // �в����ڻ�hash��ƥ����ļ�
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
        void OnShouldDownloadExtra(ref bool shouldDownload);                                                // �Ƿ���Ҫ���ж�������
        void OnDownloadExtraBegin(int countOfFiles, long size);                                             // ��ʼ��������
        void OnDownloadExtraEnd(string error);                                                              // ���ؽ���
        void OnFileCompleted(string filename, bool success);                                                // һ��������ȡ���֪ͨ�����ܳɹ���Ҳ����ʧ�ܣ�
        void OnFileProgress(string filename, ulong downedLength, ulong totalLength, float downloadSpeed);   // ÿ��������ȡ����֪ͨ
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