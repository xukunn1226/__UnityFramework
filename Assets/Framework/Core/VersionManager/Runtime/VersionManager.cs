using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    [RequireComponent(typeof(BundleExtracter), typeof(Patcher))]
    public class VersionManager : MonoBehaviour, IExtractListener, IPatcherListener
    {
        private BundleExtracter m_BundleExtracter;
        private Patcher         m_Patcher;

        public bool             bSimulateMobile;                // 编辑器模式下是否模拟真机环境
        public int              WorkerCountOfBundleExtracter    = 5;
        public int              WorkerCountOfPatcher            = 5;

        private void Awake()
        {
            m_BundleExtracter = GetComponent<BundleExtracter>();
            m_Patcher = GetComponent<Patcher>();

            if (m_BundleExtracter == null || m_Patcher == null)
                throw new System.ArgumentNullException("m_BundleExtracter == null || m_Patcher == null");
        }

        void Start()
        {
            StartExtracting();
        }

        private void StartExtracting()
        {
#if UNITY_EDITOR
            // 编辑器环境通过开关控制此流程
            if (bSimulateMobile)
                m_BundleExtracter.StartWork(WorkerCountOfBundleExtracter, this);
#else
            // 真机环境必定执行
            m_BundleExtracter.StartWork(WorkerCountOfBundleExtracter, this);
#endif
        }

        private void StartPatch()
        {
            m_Patcher.StartWork("", WorkerCountOfPatcher, this);
        }

        void IExtractListener.OnInit(bool success)
        {
            Debug.Log($"IExtractListener.OnInit:    {success}");
        }

        void IExtractListener.OnShouldExtract(ref bool shouldExtract)
        {
            Debug.Log($"IExtractListener.OnShouldExtract:   {shouldExtract}");

            if(!shouldExtract)
            { // 无需提取母包数据则执行补丁操作
                StartPatch();
            }
        }

        void IExtractListener.OnBegin(int countOfFiles)
        {
            Debug.Log($"IExtractListener.OnBegin:   {countOfFiles}");
        }

        void IExtractListener.OnEnd(float elapsedTime, string error)
        {
            Debug.Log($"IExtractListener.OnEnd:     elapsedTime({elapsedTime})      error({error})");

            if(string.IsNullOrEmpty(error))
            { // 提取母包数据完成则执行补丁操作
                StartPatch();
            }
        }

        void IExtractListener.OnFileCompleted(string filename, bool success)
        {
            Debug.Log($"IExtractListener.OnFileCompleted:   filename({filename})    success({success})");
        }

        void IExtractListener.OnFileProgress(string filename, ulong downedLength, ulong totalLength, float downloadSpeed)
        {
            Debug.Log($"IExtractListener.OnFileProgress:    filename({filename})    downedLength({downedLength})    totalLength({totalLength})      downloadSpeed({downloadSpeed})");
        }

        void IPatcherListener.OnError_DownloadBackdoor(string error)
        {
            Debug.Log($"IPatcherListener.OnError_DownloadBackdoor:  error({error})");
        }

        void IPatcherListener.OnCheck_IsLatestVersion(bool isLatestVersion)
        {
            Debug.Log($"IPatcherListener.OnCheck_IsLatestVersion:   isLatestVersion({isLatestVersion})");
        }

        void IPatcherListener.OnError_DownloadDiffCollection(string error)
        {
            Debug.Log($"IPatcherListener.OnError_DownloadDiffCollection:    error({error})");
        }

        void IPatcherListener.OnError_DownloadDiff(string error)
        {
            Debug.Log($"IPatcherListener.OnError_DownloadDiff:    error({error})");
        }

        void IPatcherListener.OnBeginDownload(int count, long size)
        {
            Debug.Log($"IPatcherListener.OnBeginDownload:   count({count})  size({size})");
        }

        void IPatcherListener.OnEndDownload(string error)
        {
            Debug.Log($"IPatcherListener.OnEndDownload:     error({error})");
        }

        void IPatcherListener.OnFileDownloadProgress(string filename, ulong downedLength, ulong totalLength, float downloadSpeed)
        {
            Debug.Log($"IPatcherListener.OnFileDownloadProgress:    filename({filename})    downedLength({downedLength})    totalLength({totalLength})      downloadSpeed({downloadSpeed})");
        }

        void IPatcherListener.OnFileDownloadCompleted(string filename, bool success)
        {
            Debug.Log($"IPatcherListener.OnFileDownloadCompleted:   filename({filename})    success({success})");
        }
    }
}