using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using Framework.NetWork;
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
    public class MainLoop : MonoBehaviour, INetManagerListener<IMessage>
    {
        private const string    kEmptySceneName    = "empty";
        private const string    kEmptyScenePath    = "assets/res/scenes/empty.unity";
        private const string    kBundlePath        = "assets/res/scenes.ab";

        public string           Ip                 = "192.168.2.7";
        public int              Port               = 11000;
        public bool             AutoConnect;
        private bool            m_isAwake;
        private float           m_TimeToLostFocus;

        async void Awake()
        {
            if (Launcher.Instance == null)
                throw new System.Exception("MainLoop: Launcher.Instance == null");
            if(NetManager.Instance == null)
                throw new System.Exception("MainLoop: NetManager.Instance == null");

            NetManager.Instance.SetListener(this);
            if(AutoConnect)
                await Connect();

            Launcher.Instance.Disable();        // 结束Launcher流程
        }

        public void ReturnToLauncher()
        {
            LevelManager.LevelContext ctx = new LevelManager.LevelContext();
            ctx.sceneName = kEmptySceneName;
            ctx.scenePath = kEmptyScenePath;
            ctx.additive = false;
            ctx.bundlePath = kBundlePath;
            LevelManager.Instance.LoadAsync(ctx);
        }
        
        async public Task Connect()
        {
            await NetManager.Instance.Connect(Ip, Port);
        }

        async public Task Reconnect()
        {
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
        void INetManagerListener<IMessage>.OnPeerDisconnected(Exception e)
        {
            Debug.LogError($"network error: {e.ToString()}");

            // if(m_isAwake)
            // {
            //     Reconnect();
            //     m_isAwake = false;
            // }
        }

        // 主动断开连接
        async void INetManagerListener<IMessage>.OnPeerClose()
        {
            Debug.Log($"connect shutdown:   {Time.frameCount}");

            if(m_isAwake)
            {
                await Task.Delay(500);          // hack: ensure that the socket is closed completely
                await Reconnect();
                m_isAwake = false;
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
            Debug.Log($"OnApplicationFocus      isFocus: {isFocus}       {System.DateTime.Now}  {Time.frameCount}");
            if(isFocus)
            {
                if(Time.time - m_TimeToLostFocus > 5)
                {
                    m_isAwake = true;
                    NetManager.Instance.Shutdown();
                }
            }
            else
            {
                m_TimeToLostFocus = Time.time;
            }
        }

        void OnApplicationPause(bool isPause)
        {
            Debug.Log($"======OnApplicationPause      isPause: {isPause}       {System.DateTime.Now}");
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
    public class MainLoop_Inspector : Editor
    {
        SerializedProperty m_IPProp;
        SerializedProperty m_PortProp;
        SerializedProperty m_AutoConnectProp;

        void OnEnable()
        {
            m_IPProp = serializedObject.FindProperty("Ip");
            m_PortProp = serializedObject.FindProperty("Port");
            m_AutoConnectProp = serializedObject.FindProperty("AutoConnect");
        }

        async public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_IPProp);
            EditorGUILayout.PropertyField(m_PortProp);
            EditorGUILayout.PropertyField(m_AutoConnectProp);

            if (GUILayout.Button("Restart"))
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