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
        public int                  workerCount;
        private List<ExtractTask>   m_TaskList;
        private List<byte[]>        m_CachedBufferList;
        private const int           m_BufferSize = 1024 * 1024;

        private AppVersion          m_Version;
        private BundleFileList      m_BundleFileList;

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
        }

        private void Update()
        {
            
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