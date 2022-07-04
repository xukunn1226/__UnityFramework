using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using System;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Application.Runtime
{
    /// <summary>
    /// 
    /// <summary>
    public class MainLoop : SingletonMono<MainLoop>, INetManagerListener<NetMsgData>
    {
        private const string    kEmptySceneName    = "empty";
        private const string    kEmptyScenePath    = "assets/res/scenes/empty.unity";

        public string           Ip                 = "192.168.2.7";
        public int              Port               = 11000;
        public bool             AutoConnect;
        private float           m_TimeToLostFocus;
        public CodeMode         codeMode           = CodeMode.Mono;
        private ILStartup       codeStartup;

        IEnumerator Start()
        {
            // 等待其他组件初始化完成
            yield return null;
            yield return null;
            yield return null;

            codeStartup = gameObject.AddComponent<ILStartup>();
            codeStartup.codeMode = codeMode;
            // #if !UNITY_EDITOR
            // codeStartup.codeMode = CodeMode.ILRuntime;
            // #endif

            // TODO:
            // NetModuleManager.Instance.Init();

            // if(Launcher.GetLauncherMode() == LoaderType.FromStreamingAssets)
            // { // 仅FromStreamingAssets时需要提取db，FromEditor从本地读取，FromPersistent会首次启动时提取
            //     yield return StartCoroutine(ConfigManager.ExtractDatabase());
            // }

            // //GlobalConfigManager.Init(AssetManager.Instance.loaderType == LoaderType.FromEditor);
            LuaMainLoop.Init();

            // if (AutoConnect)
            // {
            //     NetManager.Instance.SetListener(this);
            //     _ = Connect();
            // }
            
            yield break;
        }

        protected override void OnDestroy()
        {
            LuaMainLoop.Uninit();
            base.OnDestroy();
        }

        void Update()
        {
            LuaMainLoop.Tick();
        }

        public void ReturnToLauncher()
        {
            StreamingLevelManager.LevelContext ctx = new StreamingLevelManager.LevelContext();
            ctx.sceneName = kEmptySceneName;
            ctx.scenePath = kEmptyScenePath;
            ctx.additive = false;
            ctx.fromBundle = true;
            StreamingLevelManager.Instance.LoadAsync(ctx);
        }
        
        async public Task Connect()
        {
            await NetManager.Instance.Connect(Ip, Port);
        }

        async public Task Reconnect()
        {
            Shutdown();
            await Task.Delay(500);          // hack: ensure that the socket is closed completely
            await NetManager.Instance.Reconnect();
        }
        
        public void Shutdown()
        {
            NetManager.Instance.Shutdown();
        }

        // 连接成功
        void INetManagerListener<NetMsgData>.OnPeerConnectSuccess()
        {
            Debug.LogWarning($"MainLoop: connected success:   {Ip}:{Port}");
        }

        // 连接失败
        void INetManagerListener<NetMsgData>.OnPeerConnectFailed(Exception e)
        {
            Debug.LogWarning($"MainLoop: connect failed: {e.ToString()}");
        }

        // 异常断开连接
        void INetManagerListener<NetMsgData>.OnPeerDisconnected(Exception e)
        {
            Debug.LogError($"MainLoop: network error: {e.ToString()}");
        }

        // 主动断开连接
        void INetManagerListener<NetMsgData>.OnPeerClose()
        {
            Debug.Log($"MainLoop: connect shutdown:   {Time.frameCount}");
        }

        void INetManagerListener<NetMsgData>.OnNetworkReceive(in List<NetMsgData> msgs)
        {
            Debug.Log($"MainLoop: receive data   {msgs.Count}");
            foreach(var msg in msgs)
            {
                // NetModuleManager.Instance.DispatchMsg(msg);      // TODO:
            }
        }

        void OnApplicationFocus(bool isFocus)
        {
            // Debug.Log($"OnApplicationFocus      isFocus: {isFocus}       {System.DateTime.Now}  {Time.frameCount}");
            //GlobalConfigManager.FlushAll();

            if(isFocus)
            {
                if(Time.time - m_TimeToLostFocus > 5)
                {
                    //Debug.LogWarning("receive focus -----------");
                    //NetManager.Instance.Shutdown();
                    //_ = Reconnect();
                }
            }
            else
            {
                m_TimeToLostFocus = Time.time;
                //Debug.LogWarning("------- lost focus");
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MainLoop))]
    public class MainLoop_Inspector : UnityEditor.Editor
    {
        SerializedProperty m_IPProp;
        SerializedProperty m_PortProp;
        SerializedProperty m_AutoConnectProp;
        SerializedProperty m_CodeModeProp;

        void OnEnable()
        {
            m_IPProp = serializedObject.FindProperty("Ip");
            m_PortProp = serializedObject.FindProperty("Port");
            m_AutoConnectProp = serializedObject.FindProperty("AutoConnect");
            m_CodeModeProp = serializedObject.FindProperty("codeMode");
        }

        async public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_IPProp);
            EditorGUILayout.PropertyField(m_PortProp);
            EditorGUILayout.PropertyField(m_AutoConnectProp);
            EditorGUILayout.PropertyField(m_CodeModeProp);

            if (GUILayout.Button("ReturnToLauncher"))
            {
                ((MainLoop)target).ReturnToLauncher();
            }

            if (GUILayout.Button("Disconnect"))
            {
                ((MainLoop)target).Shutdown();
            }

            if (GUILayout.Button("Reconnect"))
            {
                await ((MainLoop)target).Reconnect();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}