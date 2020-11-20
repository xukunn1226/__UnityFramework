using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Framework.Core
{
    public class Extracter : MonoBehaviour
    {
        static public string        FILELIST_PATH   = "Assets/Resources";
        static public string        FILELIST_NAME   = "FileList.bytes";

        [Range(1, 10)]
        public int                  workerCount     = 3;
        private List<ExtractTask>   m_TaskList;
        private List<byte[]>        m_CachedBufferList;
        private const int           m_BufferSize = 1024 * 1024;

        private AppVersion          m_Version;
        private BundleFileList      m_BundleFileList;
        private int                 m_PendingExtracedFileIndex;        // 

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

            m_TaskList = new List<ExtractTask>(workerCount);
            m_CachedBufferList = new List<byte[]>(workerCount);
            for(int i = 0; i < workerCount; ++i)
            {
                m_CachedBufferList[i] = new byte[m_BufferSize];
            }

            m_PendingExtracedFileIndex = 0;

            Run();
        }

        private void Update()
        {
            
        }

        private bool Run()
        {
            foreach(var fileInfo in m_BundleFileList.FileList)
            {
                // create directory
                // string filePath = string.Format($"{Application.persistentDataPath}/{GetDirectory(fileInfo.Value.AssetPath)}");
                // if(!Directory.Exists(filePath))
                //     Directory.CreateDirectory(filePath);
            }

            return true;
        }

        // 获取下一个待提取的文件
        // false：获取失败（均已提取或提取中）；true：获取成功
        private int GetPendingExtractedFile(int pendingIndex, out BundleFileInfo fileInfo)
        {
            fileInfo = null;
            
            if(pendingIndex < 0 || pendingIndex >= m_BundleFileList.FileList.Count)
                return -1;
                // throw new System.ArgumentOutOfRangeException($"pendingIndex({pendingIndex}) < 0 || >= {m_BundleFileList.FileList.Count}");

            // fileInfo = m_BundleFileList.FileList[pendingIndex];
            // // 文件是否存在，且hash匹配
            // string filePath = string.Format($"{Application.persistentDataPath}/{fileInfo.AssetPath}");
            // if(!File.Exists(filePath))
            // {

            // }
            return 0;
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

            for(int i = 0; i < m_TaskList.Count; ++i)
            {
                m_TaskList[i].Dispose();
            }
        }
    }
}