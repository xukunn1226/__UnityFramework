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
        private const int                   m_BufferSize                = 1024 * 1024;

        private AppVersion                  m_Version;
        private BundleFileList              m_BundleFileList;
        private int                         m_PendingExtracedFileIndex;
        private List<BundleFileInfo>        m_PendingExtractedFileList  = new List<BundleFileInfo>();
        private bool                        m_HasError;

        //public enum FileExtractedState
        //{
        //    NoExtracted,            // 未提取
        //    Extracting,             // 提取中
        //    ExtractingDone,         // 提取完成
        //}
        //public class ExtractedFileInfo
        //{
        //    public BundleFileInfo       target;
        //    public FileExtractedState   state;
        //}

        private void Awake()
        {
            Init();
        }

        private void Update()
        {
            Run();
        }

        private void Init()
        {
            // load appVersion
            m_Version = AppVersion.Load();
            if (m_Version == null)
            {
                Debug.LogError("Extracter: AppVersion == null");
                throw new System.ArgumentNullException("Extracter: AppVersion == null");
            }

            // load FileList
            TextAsset asset = Resources.Load<TextAsset>(string.Format($"{Utility.GetPlatformName()}/{Path.GetFileNameWithoutExtension(FILELIST_NAME)}"));
            if (asset == null || asset.text == null)
            {
                Debug.LogError($"FileList not found.    {FILELIST_PATH}/{FILELIST_NAME}");
                throw new System.ArgumentNullException($"FileList not found.    {FILELIST_PATH}/{FILELIST_NAME}");
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

            // generate pending extracted file list
            m_PendingExtracedFileIndex = 0;
            GeneratePendingExtractedFileList();
        }

        /// <summary>
        /// 生成需要提取的文件列表
        /// </summary>
        private void GeneratePendingExtractedFileList()
        {
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
            }
        }

        private void Run()
        {
            if(m_HasError)
            {
                OnError();
                return;
            }

            foreach(var task in m_TaskWorkerList)
            {
                if(task.isRunning) continue;

                // 获取尚未提取的文件
                BundleFileInfo fileInfo = GetPendingExtractedFile();
                if (fileInfo == null) continue;

                Debug.Log($"==========Run: {fileInfo.BundleName}    frame: {Time.frameCount}");

                // begin to extract file
                ExtractTaskInfo info = new ExtractTaskInfo();
                info.srcURL         = string.Format($"{Application.streamingAssetsPath}/{Utility.GetPlatformName()}/{fileInfo.BundleName}");
                info.dstURL         = string.Format($"{Application.persistentDataPath}/{Utility.GetPlatformName()}/{fileInfo.BundleName}");
                info.verifiedHash   = fileInfo.FileHash;
                info.retryCount     = 3;
                info.onCompleted    = OnExtractCompleted;
                StartCoroutine(task.Run(info));
            }

            // step4. is done?
            if(IsExtractDone())
            {

            }
        }

        private void OnError()
        {
            Debug.LogError($"Extracter:OnError");

            for(int i = 0; i < m_TaskWorkerList.Count; ++i)
            {
                m_TaskWorkerList[i].Dispose();
            }
            m_TaskWorkerList.Clear();
        }

        private void OnExtractCompleted(ExtractTaskInfo data, bool success)
        {
            if(data == null)
                throw new System.ArgumentNullException("ExtractTaskInfo data == null");

            m_HasError = !success;

            Debug.Log($"下载：{data.dstURL} {(success ? "成功" : "失败")}");
        }

        private bool IsExtractDone()
        {
            foreach(var task in m_TaskWorkerList)
            {
                if(task.isRunning)
                    return false;                
            }
            return true;
        }

        private BundleFileInfo GetPendingExtractedFile()
        {
            if (m_PendingExtracedFileIndex < 0 || m_PendingExtracedFileIndex >= m_PendingExtractedFileList.Count)
                return null;
            return m_PendingExtractedFileList[m_PendingExtracedFileIndex++];
        }

        private void OnDestroy()
        {
            if(m_Version != null)
                AppVersion.Unload(m_Version);

            m_BundleFileList = null;

            for(int i = 0; i < m_TaskWorkerList.Count; ++i)
            {
                m_TaskWorkerList[i].Dispose();
            }
        }
    }
}