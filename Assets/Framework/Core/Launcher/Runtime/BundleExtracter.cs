using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.Core
{
    public class BundleExtracter : MonoBehaviour
    {
        static public string                FILELIST_PATH               = "Assets/Resources";
        static public string                FILELIST_NAME               = "FileList.bytes";
        static public readonly string       BASE_APPVERSION             = "BaseAppVersion_9695e71e3a224b408c39c7a75c0fa376";

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
        private IExtractListener            m_Listener;
        
        private void OnDisable()
        {
            Uninit();
        }

        public void StartWork(int workerCount, IExtractListener listener = null)
        {
            m_Listener = listener;
            m_WorkerCount = workerCount;

            StartCoroutine(Run());
        }

        /// <summary>
        /// 重新执行提取操作，不可在之前提取操作未完成时执行
        /// </summary>
        public void Restart()
        {
            Uninit();
            StartWork(m_WorkerCount, m_Listener);
        }

        private IEnumerator Run()
        {
            if(!Init())
            {
                Uninit();
                yield break;
            }

            if (!ShouldExtract())
                yield break;

            yield return StartCoroutine(Extracting());
        }

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
            if (m_BaseVersion != null)
            {
                AppVersion.Unload(m_BaseVersion);
                m_BaseVersion = null;
            }

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
        }

        private bool ShouldExtract()
        {
            string versionStr = PlayerPrefs.GetString(BASE_APPVERSION);
            bool bShould = string.IsNullOrEmpty(versionStr) ? true : m_BaseVersion.CompareTo(versionStr) != 0;
            m_Listener?.OnShouldExtract(ref bShould);
            return bShould;
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

        /// <summary>
        /// 生成需要提取的文件列表
        /// </summary>
        private void CollectPendingExtractedFileList()
        {
            m_PendingExtracedFileIndex = 0;

            // 尚不存在或hash不匹配的文件
            m_PendingExtractedFileList.Clear();
            foreach(var bfi in m_BundleFileList.FileList)
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

        private IEnumerator Extracting()
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

                    // 遇到error不再执行后续操作，但已执行的操作不中断
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
                    DownloadTaskInfo info   = new DownloadTaskInfo();
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
            //Debug.Log("Begin to extract bundle file list");
            Prepare();
            m_BeginTime = Time.time;

            m_Listener?.OnBegin(m_PendingExtractedFileList.Count);
        }

        private void OnExtractEnd(string error)
        {
            // 完全无误的提取完成后打上标签
            if (string.IsNullOrEmpty(error))
            {
                PlayerPrefs.SetString(BASE_APPVERSION, m_BaseVersion.ToString());
                PlayerPrefs.Save();
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

    public interface IExtractListener
    {
        void OnInit(bool success);                                                                          // 提取数据的准备工作是否完成，失败表示有内部致命错误
        void OnShouldExtract(ref bool shouldExtract);                                                       // 根据标签判断是否需要提取数据
        void OnBegin(int countOfFiles);                                                                     // 开始提取数据
        void OnEnd(float elapsedTime, string error);                                                        // 提取结束
        void OnFileCompleted(string filename, bool success);                                                // 一个数据提取完成通知（可能成功，也可能失败）
        void OnFileProgress(string filename, ulong downedLength, ulong totalLength, float downloadSpeed);   // 每个数据提取进度通知
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(BundleExtracter))]
    public class BundleExtracter_Inspector : Editor
    {
        public override void OnInspectorGUI()
        {
            string baseVersion = PlayerPrefs.GetString(BundleExtracter.BASE_APPVERSION);
            EditorGUILayout.LabelField("Base Version", string.IsNullOrEmpty(baseVersion) ? "None" : baseVersion);

            if (GUILayout.Button("Clear Data"))
            {
                if (PlayerPrefs.HasKey(BundleExtracter.BASE_APPVERSION))
                    PlayerPrefs.DeleteKey(BundleExtracter.BASE_APPVERSION);

                string dataPath = string.Format($"{Application.persistentDataPath}/{Utility.GetPlatformName()}");
                if (Directory.Exists(dataPath))
                    Directory.Delete(dataPath, true);
            }

            if(GUILayout.Button("Restart"))
            {
                ((BundleExtracter)target).Restart();
            }
        }
    }
#endif
}