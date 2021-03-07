using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Framework.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Application.Runtime
{
    /// <summary>
    /// 版本管理————负责obb下载、资源提取、补丁下载等
    /// 负责游戏启动（obb下载、补丁、资源提取、修复、启动画面显示/隐藏，公告、多语言，网络状态提示、check devices等等）
    /// launcher -> core -> theFirstGameScene
    /// launcher: resource extracted & check obb & patch & board
    /// core: init core components
    /// theFirstGameScene: game play scene
    /// 提供四种启动方式，将影响版控流程和资源加载方式（见LauncherMode）：
    /// 0、None（Dev）：1、略过版控流程（obb下载、资源提取、补丁下载）；2、资源加载方式由ResourceManager自主控制
    /// 1、FromEditor(Dev)：1、略过版控流程（obb下载、资源提取、补丁下载）；2、资源加载方式从AssetDatabase加载，无需打bundle
    /// 2、FromStreamingAssets(Dev)：1、略过版控流程（obb下载、资源提取、补丁下载）；2、资源加载方式从StreamingAssets加载，需打bundle
    /// 3、FromPersistent(Dev & Runtime)：1、执行版控流程（完整包和分包会有小区别，如下）；2、资源加载方式从persistentDataPath加载，需打bundle
    ///     3.1、完整包（安卓和ios）—— 资源提取、补丁下载
    ///     3.2、分包（仅安卓）—— obb下载、资源提取、补丁下载
    /// </summary>
    [RequireComponent(typeof(BundleExtracter), typeof(Patcher))]
    public class Launcher : MonoBehaviour, IExtractListener, IPatcherListener
    {
        // static public readonly string   SKIP_VERSIONCONTROL     = "SKIP_VERSIONCONTROL_123456789";
        

        private BundleExtracter         m_BundleExtracter;
        private Patcher                 m_Patcher;

        public int                      WorkerCountOfBundleExtracter = 5;
        public int                      WorkerCountOfPatcher = 5;
#pragma warning disable CS0414
        [SerializeField]
        private string                  m_CdnURL = "http://10.21.22.59";
#pragma warning restore CS0414
        private string                  m_Error;

#pragma warning disable CS0649
        [SerializeField]
        private Camera                  Camera;
        [SerializeField]
        private Canvas                  Canvas;
#pragma warning restore CS0649

        public string                   SceneName;
        public string                   ScenePath;
        public string                   BundlePath;

        private bool                    m_theFirstStart;

        static public Launcher          Instance    { get; private set; }

        private void Awake()
        {
            m_theFirstStart = true;

            if (FindObjectsOfType<Launcher>().Length > 1)
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

            if (Camera == null)
                throw new ArgumentNullException("camera");

            if (Canvas == null)
                throw new ArgumentNullException("canvas");
        }

        void Start()
        {
            // await TestPing("202.108.22.5", 3);

            StartWork();
        }

        private void StartWork()
        {
            ShowUI(true);

#if UNITY_EDITOR
            if (EditorLauncherMode.Mode() != LauncherMode.FromPersistent)
            {
                VersionControlFinished();
            }
            else
            {
                StartBundleExtracted();
            }
#else
            StartBundleExtracted();
#endif
        }
        
        // 再次执行完整流程（流程结束或异常时才可restart，过程中不可使用）
        public void Restart()
        {
            m_theFirstStart = false;
            StartWork();
        }

#if USE_APK_EXPANSIONFILES
        private void StartObbDownload()
        {

        }
#endif

        private void StartBundleExtracted()
        {            
            if(m_theFirstStart)
                m_BundleExtracter.StartWork(WorkerCountOfBundleExtracter, this);
            else
                m_BundleExtracter.Restart();
        }

        private void StartPatch()
        {
            m_Patcher.StartWork(GetCDNURL(), WorkerCountOfPatcher, this);
        }

        private string GetCDNURL()
        {
#if UNITY_EDITOR
            return string.Format($"{UnityEngine.Application.dataPath}/../Deployment/CDN");          // 编辑模式下默认的CDN地址
#else
            return m_CdnURL;
#endif
        }

        void IExtractListener.OnInit(bool success)
        {
            Debug.Log($"IExtractListener.OnInit:    {success}");
        }

        void IExtractListener.OnShouldExtract(ref bool shouldExtract)
        {
            Debug.Log($"IExtractListener.OnShouldExtract:   {shouldExtract}");

            if (!shouldExtract)
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
            if (string.IsNullOrEmpty(error))
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

            if (isLatestVersion)
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

        private void VersionControlFinished()
        {
            if (string.IsNullOrEmpty(m_Error))
            {
                Debug.Log($"VersionControl finished successfully");
            }
            else
            {
                Debug.LogError($"VersionControl finished, there is an error: {m_Error}");
            }

            LoadCoreScene();
        }

        // 以场景的形式(core.unity)加载核心组件，相比以prefab(core.prefab)的方式加载核心组件优势有：
        // 1、加载场景更方便
        // 2、核心组件数据便于修改，可以热更
        private void LoadCoreScene()
        {
            LevelManager.LevelContext ctx = new LevelManager.LevelContext();
            ctx.sceneName = SceneName;
            ctx.scenePath = ScenePath;
            ctx.additive = false;
            ctx.bundlePath = BundlePath;
            LevelManager.Instance.LoadAsync(ctx);
        }        

        // disable Launcher
        public void Disable()
        {
            ShowUI(false);
        }

        private void ShowUI(bool show)
        {
            Canvas.enabled = show;
            Camera.enabled = show;
        }

        public async Task<bool> TestPing(string ipString, int tryCount = 3)
        {
            System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
            System.Net.NetworkInformation.PingOptions opt = new System.Net.NetworkInformation.PingOptions();
            opt.DontFragment = true;
            byte[] buf = System.Text.Encoding.ASCII.GetBytes("123");

            tryCount = tryCount <= 0 ? Int16.MaxValue : tryCount;
            while (tryCount > 0)
            {
                --tryCount;

                try
                {
                    System.Net.NetworkInformation.PingReply result = await ping.SendPingAsync(IPAddress.Parse(ipString), 1000, buf, opt);
                    Debug.Log($"Ping: {ipString}    {result.Status}   {result.RoundtripTime}");
                    if (result.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            return false;
        }

        private bool IsWifi()
        {
            return UnityEngine.Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        }

        // 网络链接是否断开
        private bool IsNetworkReachability()
        {
            return UnityEngine.Application.internetReachability != NetworkReachability.NotReachable;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Launcher))]
    public class Launcher_Inspector : Editor
    {
        private SerializedProperty  m_WorkerCountOfBundleExtracterProp;
        private SerializedProperty  m_WorkerCountOfPatcherProp;
        private SerializedProperty  m_CdnURLProp;
        private SerializedProperty  m_CameraProp;
        private SerializedProperty  m_CanvasProp;
        private SerializedProperty  m_SceneNameProp;
        private SerializedProperty  m_ScenePathProp;
        private SerializedProperty  m_BundlePathProp;
        private string              m_ipString      = "202.108.22.5";
        private string              m_pingResult    = "No Information";

        void OnEnable()
        {
            m_WorkerCountOfBundleExtracterProp = serializedObject.FindProperty("WorkerCountOfPatcher");
            m_WorkerCountOfPatcherProp = serializedObject.FindProperty("WorkerCountOfPatcher");
            m_CdnURLProp = serializedObject.FindProperty("m_CdnURL");
            m_CameraProp = serializedObject.FindProperty("Camera");
            m_CanvasProp = serializedObject.FindProperty("Canvas");
            m_SceneNameProp = serializedObject.FindProperty("SceneName");
            m_ScenePathProp = serializedObject.FindProperty("ScenePath");
            m_BundlePathProp = serializedObject.FindProperty("BundlePath");
        }

        async public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.ObjectField(m_CameraProp, typeof(Camera));
            EditorGUILayout.ObjectField(m_CanvasProp, typeof(Canvas));
            EditorGUILayout.PropertyField(m_SceneNameProp);
            EditorGUILayout.PropertyField(m_ScenePathProp);
            EditorGUILayout.PropertyField(m_BundlePathProp);
            EditorGUILayout.IntSlider(m_WorkerCountOfBundleExtracterProp, 1, 10, "Extracter Worker");
            EditorGUILayout.IntSlider(m_WorkerCountOfPatcherProp, 1, 10, "Patcher Worker");
            EditorGUILayout.LabelField("CDN", m_CdnURLProp.stringValue, EditorStyles.boldLabel);

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Ping"))
            {
                bool ret = await ((Launcher)target).TestPing(m_ipString);
                m_pingResult = ret ? "Network connectivity" : "Network not reachable";
            }
            m_ipString = EditorGUILayout.TextField(m_ipString);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Ping reply", m_pingResult, EditorStyles.boldLabel);
            
            // EditorGUILayout.Separator();
            // EditorGUILayout.Separator();

            // string value = PlayerPrefs.GetString(Launcher.SKIP_VERSIONCONTROL);
            // EditorGUILayout.LabelField("Skip Version Control: ", string.IsNullOrEmpty(value) ? "OFF" : "ON", EditorStyles.boldLabel);

            // if (string.IsNullOrEmpty(value))
            // {
            //     if (GUILayout.Button("Enable"))
            //     {
            //         PlayerPrefs.SetString(Launcher.SKIP_VERSIONCONTROL, "0000000000000000000");
            //     }
            // }
            // else
            // {
            //     if (GUILayout.Button("Disable"))
            //     {
            //         PlayerPrefs.SetString(Launcher.SKIP_VERSIONCONTROL, "");
            //     }
            // }

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            GUILayout.BeginVertical(new GUIStyle("HelpBox"));
            EditorGUILayout.LabelField("启动项目的四种方式：见Tools/Launcher Mode", new GUIStyle("LargeLabel"));
            EditorGUILayout.LabelField(@"None：                        1、略过版控流程（obb下载、资源提取、补丁下载）；2、资源加载方式由ResourceManager配置决定", new GUIStyle("LargeLabel"));
            EditorGUILayout.LabelField(@"FromEditor：                1、略过版控流程（obb下载、资源提取、补丁下载）；2、资源加载方式从AssetDatabase加载，无需打bundle", new GUIStyle("LargeLabel"));
            EditorGUILayout.LabelField(@"FromStreamingAssets：  1、略过版控流程（obb下载、资源提取、补丁下载）；2、资源加载方式从StreamingAssets加载，需打bundle", new GUIStyle("LargeLabel"));
            EditorGUILayout.LabelField(@"FromPersistent：           1、执行版控流程（完整包和分包会有一些区别）；2、资源加载方式从persistentDataPath加载，需打bundle", new GUIStyle("LargeLabel"));
            GUILayout.EndVertical();

            if (GUILayout.Button("Restart Launcher"))
            {
                ((Launcher)target).Restart();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
    
    public enum LauncherMode
    {
        None,                   // 1、略过版控流程（obb下载、资源提取、补丁下载）；2、资源加载方式由ResourceManager自主控制
        FromEditor,             // 1、略过版控流程（obb下载、资源提取、补丁下载）；2、资源加载方式从AssetDatabase加载，无需打bundle
        FromStreamingAssets,    // 1、略过版控流程（obb下载、资源提取、补丁下载）；2、资源加载方式从StreamingAssets加载，需打bundle
        FromPersistent,         // 1、执行版控流程（完整包和分包会有小区别，如下）；2、资源加载方式从persistentDataPath加载，需打bundle
                                //      * 完整包（安卓和ios）—— 资源提取、补丁下载
                                //      * 分包（仅安卓）—— obb下载、资源提取、补丁下载
    }

    [InitializeOnLoad]
    public static class EditorLauncherMode
    {        
        static private readonly string   LAUNCHER_MODE_FLAG             = "LAUNCHER_MODE_FLAG_8a0848d861a7eee48aad2b8083a7db8d";
        private const string            MenuName_None                   = "Tools/Launcher Mode/None";
        private const string            MenuName_FromEditor             = "Tools/Launcher Mode/From Editor";
        private const string            MenuName_FromStreamingAssets    = "Tools/Launcher Mode/From StreamingAssets";
        private const string            MenuName_FromPersistent         = "Tools/Launcher Mode/From Persistent";

        static EditorLauncherMode()
        {
            UpdateMenu(Mode());
        }

        static public LauncherMode Mode()
        {
            string mode = PlayerPrefs.GetString(LAUNCHER_MODE_FLAG, LauncherMode.FromEditor.ToString());
            if(mode == LauncherMode.None.ToString())
            {
                return LauncherMode.None;
            }
            else if(mode == LauncherMode.FromEditor.ToString())
            {
                return LauncherMode.FromEditor;
            }
            else if(mode == LauncherMode.FromStreamingAssets.ToString())
            {
                return LauncherMode.FromStreamingAssets;
            }
            else if(mode == LauncherMode.FromPersistent.ToString())
            {
                return LauncherMode.FromPersistent;
            }            
            return LauncherMode.FromEditor;
        }

        static private void UpdateMenu(LauncherMode mode)
        {
            switch(mode)
            {
                case LauncherMode.None:
                    Menu.SetChecked(MenuName_None,                  true);
                    Menu.SetChecked(MenuName_FromEditor,            false);
                    Menu.SetChecked(MenuName_FromStreamingAssets,   false);
                    Menu.SetChecked(MenuName_FromPersistent,        false);
                    break;
                case LauncherMode.FromEditor:
                    Menu.SetChecked(MenuName_None,                  false);
                    Menu.SetChecked(MenuName_FromEditor,            true);
                    Menu.SetChecked(MenuName_FromStreamingAssets,   false);
                    Menu.SetChecked(MenuName_FromPersistent,        false);
                    break;
                case LauncherMode.FromStreamingAssets:
                    Menu.SetChecked(MenuName_None,                  false);
                    Menu.SetChecked(MenuName_FromEditor,            false);
                    Menu.SetChecked(MenuName_FromStreamingAssets,   true);
                    Menu.SetChecked(MenuName_FromPersistent,        false);
                    break;
                case LauncherMode.FromPersistent:
                    Menu.SetChecked(MenuName_None,                  false);
                    Menu.SetChecked(MenuName_FromEditor,            false);
                    Menu.SetChecked(MenuName_FromStreamingAssets,   false);
                    Menu.SetChecked(MenuName_FromPersistent,        true);
                    break;
            }
        }

        [MenuItem(MenuName_None, priority = 0)]
        static public void Launcher_None()
        {
            PlayerPrefs.SetString(LAUNCHER_MODE_FLAG, LauncherMode.None.ToString());
            PlayerPrefs.Save();
            UpdateMenu(LauncherMode.None);
        }

        [MenuItem(MenuName_FromEditor, priority = 1)]
        static public void Launcher_FromEditor()
        {
            PlayerPrefs.SetString(LAUNCHER_MODE_FLAG, LauncherMode.FromEditor.ToString());
            PlayerPrefs.Save();
            UpdateMenu(LauncherMode.FromEditor);
        }

        [MenuItem(MenuName_FromStreamingAssets, priority = 2)]
        static public void Launcher_FromStreamingAssets()
        {
            PlayerPrefs.SetString(LAUNCHER_MODE_FLAG, LauncherMode.FromStreamingAssets.ToString());
            PlayerPrefs.Save();
            UpdateMenu(LauncherMode.FromStreamingAssets);
        }

        [MenuItem(MenuName_FromPersistent, priority = 3)]
        static public void Launcher_FromPersistent()
        {
            PlayerPrefs.SetString(LAUNCHER_MODE_FLAG, LauncherMode.FromPersistent.ToString());
            PlayerPrefs.Save();
            UpdateMenu(LauncherMode.FromPersistent);
        }
    }
#endif
}