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
        private int                         m_PendingExtracedFileIndex;        // 
        private List<ExtractedFileInfo>     m_PendingExtractedFileList  = new List<ExtractedFileInfo>();
        private bool                        m_HasError;

        public enum FileExtractedState
        {
            NoExtracted,            // 未提取
            Extracting,             // 提取中
            ExtractingDone,         // 提取完成
        }
        public class ExtractedFileInfo
        {
            public BundleFileInfo       target;
            public FileExtractedState   state;
        }

        private void Awake()
        {
            m_Version = AppVersion.Load();
            if(m_Version == null)
            {
                Debug.LogError("Extracter: AppVersion == null");
                throw new System.ArgumentNullException("Extracter: AppVersion == null");
            }

            TextAsset asset = Resources.Load<TextAsset>(string.Format($"{Utility.GetPlatformName()}/{Path.GetFileNameWithoutExtension(FILELIST_NAME)}"));
            if(asset == null || asset.text == null)
            {
                Debug.LogError($"FileList not found.    {FILELIST_PATH}/{FILELIST_NAME}");
                throw new System.ArgumentNullException($"FileList not found.    {FILELIST_PATH}/{FILELIST_NAME}");
            }
            m_BundleFileList = BundleFileList.DeserializeFromJson(asset.text);

            m_TaskWorkerList = new List<ExtractTask>(workerCount);
            m_CachedBufferList = new List<byte[]>(workerCount);
            for(int i = 0; i < workerCount; ++i)
            {
                m_CachedBufferList[i] = new byte[m_BufferSize];
            }

            m_PendingExtracedFileIndex = 0;
            GeneratePendingExtractedFileList();

            Run();
        }

        private void Update()
        {
            
        }

        private void GeneratePendingExtractedFileList()
        {
            m_PendingExtractedFileList.Clear();
            foreach(var bfi in m_BundleFileList.FileList)
            {
                string filePath = string.Format($"{Application.persistentDataPath}/{bfi.AssetPath}");
                if(!File.Exists(filePath))
                {
                    AddPendingExtractedFile(bfi);
                }

                FileStream stream = new FileStream(filePath, FileMode.Open);
                if(!EasyMD5.Verify(stream, bfi.FileHash))
                {
                    AddPendingExtractedFile(bfi);
                }
            }
        }

        private void AddPendingExtractedFile(BundleFileInfo bfi)
        {
            ExtractedFileInfo efi = new ExtractedFileInfo();
            efi.target = bfi;
            efi.state = FileExtractedState.NoExtracted;
            m_PendingExtractedFileList.Add(efi);
        }

        private void Run()
        {
            // foreach(var fileInfo in m_BundleFileList.FileList)
            // {
            //     // create directory
            //     string filePath = string.Format($"{Application.persistentDataPath}/{GetDirectory(fileInfo.AssetPath)}");
            //     if(!Directory.Exists(filePath))
            //         Directory.CreateDirectory(filePath);
            // }

            if(m_HasError)
            {
                OnError();
                return;
            }

            foreach(var task in m_TaskWorkerList)
            {
                if(task.isRunning) continue;

                // 获取尚未提取的文件
                m_PendingExtracedFileIndex = GetPendingExtractedFile(m_PendingExtracedFileIndex);
                if(m_PendingExtracedFileIndex == -1) break;
                BundleFileInfo fileInfo = m_BundleFileList.FileList[m_PendingExtracedFileIndex];

                // begin to extract file
                ExtractTaskInfo info = new ExtractTaskInfo();
                info.srcURL         = string.Format($"{Application.streamingAssetsPath}/{Utility.GetPlatformName()}/{fileInfo.BundleName}");
                info.dstURL         = string.Format($"{Application.persistentDataPath}/{Utility.GetPlatformName()}/{fileInfo.BundleName}");
                info.userData       = fileInfo;
                info.onCompleted    = OnExtractCompleted;
                StartCoroutine(task.Run(info));
            }

            // step4. is over?
            if(IsAllTaskStop() && m_PendingExtracedFileIndex == -1)
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
        }

        private void OnExtractCompleted(ExtractTaskInfo data, bool success)
        {
            if(data == null)
                throw new System.ArgumentNullException("ExtractTaskInfo data == null");

            ExtractedFileInfo efi = (ExtractedFileInfo)data.userData;
            if(efi == null)
                throw new System.ArgumentNullException("data.userData can't convert to ExtractedFileInfo");

            if(efi.target == null)
                throw new System.ArgumentNullException("efi.target");

            // if(string.IsNullOrEmpty(fileHash) || string.Compare(fileHash, efi.target.FileHash) != 0)
            // {

            // }
        }

        private bool IsAllTaskStop()
        {
            foreach(var task in m_TaskWorkerList)
            {
                if(task.isRunning)
                    return false;                
            }
            return true;
        }

        // get the next file to be extracted
        private int GetPendingExtractedFile(int pendingIndex)
        {
            if(pendingIndex < 0 || pendingIndex >= m_BundleFileList.FileList.Count)
                return -1;

            BundleFileInfo fileInfo = m_BundleFileList.FileList[pendingIndex];
            string filePath = string.Format($"{Application.persistentDataPath}/{fileInfo.AssetPath}");
            if(!File.Exists(filePath))
            { // 文件不存在，需要提取
                // fileInfo.FileStream = new FileStream(filePath, FileMode.Create);
                // fileInfo.State = BundleFileState.NoExtracted;
                return pendingIndex;
            }

            FileStream stream = new FileStream(filePath, FileMode.Open);
            if(!EasyMD5.Verify(stream, fileInfo.FileHash))
            { // 文件hash不匹配，需要重新提取
                // fileInfo.FileStream = stream;
                // fileInfo.State = BundleFileState.NoExtracted;
                stream.Close();
                stream.Dispose();
                return pendingIndex;
            }
            stream.Close();
            stream.Dispose();

            // fileInfo.State = BundleFileState.ExtractingDone;
            return GetPendingExtractedFile(pendingIndex + 1);
        }

        private string GetDirectory(string filePath)
        {
            filePath = filePath.Replace("\\", "/");
            return filePath.Substring(0, filePath.LastIndexOf("/"));
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