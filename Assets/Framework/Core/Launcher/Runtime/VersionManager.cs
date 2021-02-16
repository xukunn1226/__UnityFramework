using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.Core
{
    /// <summary>
    /// 版本管理，负责obb下载、资源提取、补丁下载等
    /// 负责游戏启动（obb下载、补丁、资源提取、修复、启动画面显示、隐藏，多语言，网络状态提示等等）
    /// </summary>
    [RequireComponent(typeof(BundleExtracter), typeof(Patcher))]
    public class VersionManager : MonoBehaviour, IExtractListener, IPatcherListener
    {
        public event Action<string>     OnVersionControlFinished;

        static public readonly string   SKIP_VERSIONCONTROL             = "SKIP_VERSIONCONTROL_123456789";

        private BundleExtracter         m_BundleExtracter;
        private Patcher                 m_Patcher;

        public int                      WorkerCountOfBundleExtracter    = 5;
        public int                      WorkerCountOfPatcher            = 5;
#pragma warning disable CS0414        
        [SerializeField]
        private string                  m_CdnURL                        = "http://10.21.22.59";
#pragma warning restore CS0414
        private string                  m_Error;

        [SerializeField]
        new private Camera              camera;
        [SerializeField]
        private Canvas                  canvas;

        static public VersionManager          Instance
        {
            get; private set;
        }

        private void Awake()
        {
            if (FindObjectsOfType<VersionManager>().Length > 1)
            {
                DestroyImmediate(this);
                throw new Exception("Launcher has already exist...");
            }

            gameObject.name = "[Launcher]";

            Instance = this;

            DontDestroyOnLoad(gameObject);

            m_BundleExtracter = GetComponent<BundleExtracter>();
            m_Patcher = GetComponent<Patcher>();

            if (m_BundleExtracter == null || m_Patcher == null)
                throw new System.ArgumentNullException("m_BundleExtracter == null || m_Patcher == null");

            if(camera == null)
                throw new ArgumentNullException("camera");

            if(canvas == null)
                throw new ArgumentNullException("canvas");
        }

        void Start()
        {
            StartWork();
        }

        static public bool IsWifi()
        {
            return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        }

        // 网络链接是否断开
        static public bool IsNetworkReachability()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        // 异常失败或网络中断时才可执行
        public void Restart()
        {
#if UNITY_EDITOR
            if(SkipVersionControl())
            {
                VersionControlFinished();
            }
            else
            {
                m_BundleExtracter?.Restart();
            }
#else
            m_BundleExtracter?.Restart();
#endif            
        }

        public void StartWork()
        {
            canvas.enabled = true;
#if UNITY_EDITOR
            if (SkipVersionControl())
            {
                VersionControlFinished();
            }
            else
            {
                m_BundleExtracter.StartWork(WorkerCountOfBundleExtracter, this);
            }
#else
            m_BundleExtracter.StartWork(WorkerCountOfBundleExtracter, this);
#endif
        }

        private bool SkipVersionControl()
        {
            return !string.IsNullOrEmpty(PlayerPrefs.GetString(SKIP_VERSIONCONTROL));
        }

        private string GetCDNURL()
        {
#if UNITY_EDITOR
            return string.Format($"{Application.dataPath}/../Deployment/CDN");          // 编辑模式下默认的CDN地址
#else
            return m_CdnURL;
#endif
        }

        private void StartPatch()
        {
            m_Patcher.StartWork(GetCDNURL(), WorkerCountOfPatcher, this);
        }

        void IExtractListener.OnInit(bool success)
        {
            Debug.Log($"IExtractListener.OnInit:    {success}");
        }

        void IExtractListener.OnShouldExtract(ref bool shouldExtract)
        {
            Debug.Log($"IExtractListener.OnShouldExtract:   {shouldExtract}");

            if(!shouldExtract)
            { // 不需要提取直接执行补丁流程
                StartPatch();
            }
        }

        void IExtractListener.OnBegin(int countOfFiles)
        {
            Debug.Log($"IExtractListener.OnBegin:   {countOfFiles}");
        }

        void IExtractListener.OnEnd(float elapsedTime, string error)
        {
            if(string.IsNullOrEmpty(error))
            {
                Debug.Log($"IExtractListener.OnEnd:     elapsedTime({elapsedTime})");

                StartPatch();       // 提取结束执行补丁流程
            }
            else
            {
                Debug.LogError($"IExtractListener.OnEnd:     elapsedTime({elapsedTime})      error({error})");

                m_Error = error;
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
            m_Error = error;
        }

        void IPatcherListener.OnCheck_IsLatestVersion(bool isLatestVersion)
        {
            Debug.Log($"IPatcherListener.OnCheck_IsLatestVersion:   isLatestVersion({isLatestVersion})");

            if(isLatestVersion)
            {
                VersionControlFinished();
            }
        }

        void IPatcherListener.OnError_DownloadDiffCollection(string error)
        {
            Debug.Log($"IPatcherListener.OnError_DownloadDiffCollection:    error({error})");
            m_Error = error;
        }

        void IPatcherListener.OnError_DownloadDiff(string error)
        {
            Debug.Log($"IPatcherListener.OnError_DownloadDiff:    error({error})");
            m_Error = error;
        }

        bool IPatcherListener.Prepare(int count, long size)
        {
            Debug.Log($"IPatcherListener.OnBeginDownload:   count({count})  size({size})");
            return true;
        }

        void IPatcherListener.OnPatchCompleted(string error)
        {
            Debug.Log($"IPatcherListener.OnEndDownload:     error({error})");

            m_Error = error;
            VersionControlFinished();
        }

        void IPatcherListener.OnFileDownloadProgress(string filename, ulong downedLength, ulong totalLength, float downloadSpeed)
        {
            Debug.Log($"IPatcherListener.OnFileDownloadProgress:    filename({filename})    downedLength({downedLength})    totalLength({totalLength})      downloadSpeed({downloadSpeed})");
        }

        void IPatcherListener.OnFileDownloadCompleted(string filename, bool success)
        {
            Debug.Log($"IPatcherListener.OnFileDownloadCompleted:   filename({filename})    success({success})");
        }

        void VersionControlFinished()
        {
            if(string.IsNullOrEmpty(m_Error))
            {
                Debug.Log($"VersionControl finished successfully");
            }
            else
            {
                Debug.LogError($"VersionControl finished, there is an error: {m_Error}");
            }
            OnVersionControlFinished?.Invoke(m_Error);
            canvas.enabled = false;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(VersionManager))]
    public class VersionManager_Inspector : Editor
    {
        private SerializedProperty m_WorkerCountOfBundleExtracterProp;
        private SerializedProperty m_WorkerCountOfPatcherProp;
        private SerializedProperty m_CdnURLProp;
        private SerializedProperty m_CameraProp;
        private SerializedProperty m_CanvasProp;

        void OnEnable()
        {
            m_WorkerCountOfBundleExtracterProp = serializedObject.FindProperty("WorkerCountOfPatcher");
            m_WorkerCountOfPatcherProp = serializedObject.FindProperty("WorkerCountOfPatcher");
            m_CdnURLProp = serializedObject.FindProperty("m_CdnURL");
            m_CameraProp = serializedObject.FindProperty("camera");
            m_CanvasProp = serializedObject.FindProperty("canvas");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.ObjectField(m_CameraProp, typeof(Camera));
            EditorGUILayout.ObjectField(m_CanvasProp, typeof(Canvas));
            
            string value = PlayerPrefs.GetString(VersionManager.SKIP_VERSIONCONTROL);
            EditorGUILayout.LabelField("Skip Version Control: ", string.IsNullOrEmpty(value) ? "OFF" : "ON", EditorStyles.boldLabel);

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

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            EditorGUILayout.IntSlider(m_WorkerCountOfBundleExtracterProp, 1, 10, "Extracter Worker");
            EditorGUILayout.IntSlider(m_WorkerCountOfPatcherProp, 1, 10, "Patcher Worker");

            EditorGUILayout.LabelField("CDN", m_CdnURLProp.stringValue, EditorStyles.boldLabel);
            
            if(GUILayout.Button("Restart"))
            {
                ((VersionManager)target).Restart();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}