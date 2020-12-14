using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.Core
{
    [RequireComponent(typeof(BundleExtracter), typeof(Patcher))]
    public class VersionManager : MonoBehaviour, IExtractListener, IPatcherListener
    {
        public event Action             OnVersionControlFinished;

        static public readonly string   SKIP_VERSIONCONTROL             = "SKIP_VERSIONCONTROL_123456789";

        private BundleExtracter         m_BundleExtracter;
        private Patcher                 m_Patcher;

        public int                      WorkerCountOfBundleExtracter    = 5;
        public int                      WorkerCountOfPatcher            = 5;
        private string                  m_CdnURL;

        private void Awake()
        {
            m_CdnURL = "http://10.21.22.59";

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
            if (SkipVersionControl())
            { // 跳过版本控制直接结束
                OnVersionControlFinished?.Invoke();
            }
            else
            { // 本地模拟
                m_CdnURL = string.Format($"{Application.dataPath}/../Deployment/CDN");
                m_BundleExtracter.StartWork(WorkerCountOfBundleExtracter, this);
            }
#else
            // 真机环境必定执行
            m_BundleExtracter.StartWork(WorkerCountOfBundleExtracter, this);
#endif
        }

        private bool SkipVersionControl()
        {
            return !string.IsNullOrEmpty(PlayerPrefs.GetString(SKIP_VERSIONCONTROL));
        }

        private void StartPatch()
        {
            m_Patcher.StartWork(m_CdnURL, WorkerCountOfPatcher, this);
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
            if(string.IsNullOrEmpty(error))
            {
                OnVersionControlFinished?.Invoke();
            }
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

#if UNITY_EDITOR
    [CustomEditor(typeof(VersionManager))]
    public class VersionManager_Inspector : Editor
    {
        public override void OnInspectorGUI()
        {
            string value = PlayerPrefs.GetString(VersionManager.SKIP_VERSIONCONTROL);
            EditorGUILayout.LabelField("Simulate Mobile", string.IsNullOrEmpty(value) ? "OFF" : "ON");

            if(string.IsNullOrEmpty(value))
            {
                if(GUILayout.Button("Enable"))
                {
                    PlayerPrefs.SetString(VersionManager.SKIP_VERSIONCONTROL, "0000000000000000000");
                }
            }
            else
            {
                if(GUILayout.Button("Disable"))
                {
                    PlayerPrefs.SetString(VersionManager.SKIP_VERSIONCONTROL, "");
                }
            }
        }
    }
#endif
}