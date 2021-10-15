using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using Framework.NetWork;
using Framework.AssetManagement.Runtime;
using NetProtocol;
using Google.Protobuf;
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
    public class MainLoop : SingletonMono<MainLoop>, INetManagerListener<IMessage>
    {
        public string           TheFirstGame;
        public string           ScenePath;
        public string           BundlePath;

        private const string    kEmptySceneName    = "empty";
        private const string    kEmptyScenePath    = "assets/res/scenes/empty.unity";
        private const string    kBundlePath        = "assets/res/scenes.ab";

        public string           Ip                 = "192.168.2.7";
        public int              Port               = 11000;
        public bool             AutoConnect;
        private bool            m_NeedReconnect;
        private float           m_TimeToLostFocus;

        protected override void Awake()
        {
            base.Awake();

            if (Launcher.Instance == null)
                throw new System.Exception("MainLoop: Launcher.Instance == null");
            if (NetManager.Instance == null)
                throw new System.Exception("MainLoop: NetManager.Instance == null");

            NetManager.Instance.SetListener(this);
            // if(AutoConnect)
            //     await Connect();

            Launcher.Instance.Disable();        // 结束Launcher流程

            LuaMainLoop.Init();            
        }

        IEnumerator Start()
        {
            GlobalConfigManager.Init(AssetManager.Instance.loaderType == LoaderType.FromEditor);

            StreamingLevelManager.LevelContext ctx = new StreamingLevelManager.LevelContext();
            ctx.sceneName = TheFirstGame;
            ctx.scenePath = ScenePath;
            ctx.additive = false;
            ctx.bundlePath = BundlePath;
            StreamingLevelManager.Instance.LoadAsync(ctx);


            // GameInfoManager.Instance.SwitchTo(GameState.World);
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
            ctx.bundlePath = kBundlePath;
            StreamingLevelManager.Instance.LoadAsync(ctx);
        }
        
        async public Task Connect()
        {
            await NetManager.Instance.Connect(Ip, Port);
        }

        async public Task Reconnect()
        {
            await Task.Delay(500);          // hack: ensure that the socket is closed completely
            await NetManager.Instance.Reconnect();
        }
        
        public void Shutdown()
        {
            NetManager.Instance.Shutdown();
        }

        // 连接成功
        async void INetManagerListener<IMessage>.OnPeerConnectSuccess()
        {
            Debug.Log($"connected success:   {Ip}:{Port}");
            await AutoSendingForDebug();
        }

        // 连接失败
        void INetManagerListener<IMessage>.OnPeerConnectFailed(Exception e)
        {
            Debug.LogWarning($"connect failed: {e.ToString()}");
        }

        // 异常断开连接
        async void INetManagerListener<IMessage>.OnPeerDisconnected(Exception e)
        {
            Debug.LogError($"network error: {e.ToString()}");

            if(m_NeedReconnect)
            {
                await Reconnect();
                m_NeedReconnect = false;
            }
        }

        // 主动断开连接
        async void INetManagerListener<IMessage>.OnPeerClose()
        {
            Debug.Log($"connect shutdown:   {Time.frameCount}");

            if(m_NeedReconnect)
            {
                await Reconnect();
                m_NeedReconnect = false;
            }
        }

        void INetManagerListener<IMessage>.OnNetworkReceive(in List<IMessage> msgs)
        {
            // foreach (var msg in msgs)
            // {
            //     Debug.Log($"====Receive: {msg}");
            // }
        }

        void OnApplicationFocus(bool isFocus)
        {
            // Debug.Log($"OnApplicationFocus      isFocus: {isFocus}       {System.DateTime.Now}  {Time.frameCount}");
            GlobalConfigManager.FlushAll();

            if(isFocus)
            {
                if(Time.time - m_TimeToLostFocus > 5)
                {
                    m_NeedReconnect = true;
                    NetManager.Instance.Shutdown();
                }
            }
            else
            {
                m_TimeToLostFocus = Time.time;
            }
        }

        async Task AutoSendingForDebug()
        {
            {
                int index = 0;
                while (UnityEngine.Application.isPlaying && NetManager.Instance != null && NetManager.Instance.state == ConnectState.Connected)
                {
                    ++index;
                    StoreRequest msg = new StoreRequest();
                    msg.Name = "1233";
                    msg.Num = 3;
                    msg.Result = 4;
                    if (index % 2 == 0)
                        msg.MyList.Add("22222222222");
                    if (index % 3 == 0)
                        msg.MyList.Add("33333333333333");
                    // UnityEngine.Debug.LogWarning("");
                    if (!NetManager.Instance.SendData(msg))
                        break;

                    await Task.Yield();
                }
                Debug.Log("退出模拟发包");
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MainLoop))]
    public class MainLoop_Inspector : UnityEditor.Editor
    {
        SerializedProperty m_TheFirstGameProp;
        SerializedProperty m_ScenePathProp;
        SerializedProperty m_BundlePathProp;
        SerializedProperty m_IPProp;
        SerializedProperty m_PortProp;
        SerializedProperty m_AutoConnectProp;

        void OnEnable()
        {
            m_TheFirstGameProp = serializedObject.FindProperty("TheFirstGame");
            m_ScenePathProp = serializedObject.FindProperty("ScenePath");
            m_BundlePathProp = serializedObject.FindProperty("BundlePath");
            m_IPProp = serializedObject.FindProperty("Ip");
            m_PortProp = serializedObject.FindProperty("Port");
            m_AutoConnectProp = serializedObject.FindProperty("AutoConnect");
        }

        async public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_TheFirstGameProp);
            EditorGUILayout.PropertyField(m_ScenePathProp);
            EditorGUILayout.PropertyField(m_BundlePathProp);
            EditorGUILayout.PropertyField(m_IPProp);
            EditorGUILayout.PropertyField(m_PortProp);
            EditorGUILayout.PropertyField(m_AutoConnectProp);

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
                ((MainLoop)target).Shutdown();
                await ((MainLoop)target).Reconnect();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}