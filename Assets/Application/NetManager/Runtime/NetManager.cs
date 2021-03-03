using System;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using System.Threading.Tasks;
using NetProtocol;
using Framework.NetWork;
using Framework.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Application.Runtime
{
    public class NetManager : SingletonMono<NetManager>, INetListener<IMessage>
    {
        static private IPacket<IMessage>    s_Parser    = new PacketProtobuf();
        private NetClient<IMessage>         m_NetClient;

        private bool                        m_Quit;

        public string                       Ip          = "192.168.5.14";
        public int                          Port        = 11000;

        protected async override void Awake()
        {
            base.Awake();

            m_NetClient = new NetClient<IMessage>(this);
            await AutoSending();
        }

        //async void OnEnable()
        //{
        //    m_Quit = false;

        //    await AutoSending();
        //}

        //void OnDisable()
        //{
        //    m_Quit = true;
        //    m_NetClient.Close();
        //}

        void Update()
        {
            m_NetClient?.Tick();
        }

        async public void Connect()
        {
            m_Quit = false;
            await m_NetClient.Connect(Ip, Port);
        }

        public void Disconnect()
        {
            m_Quit = true;
            m_NetClient?.Shutdown();
        }

        public void Reconnect()
        {
            m_NetClient?.Reconnect();
        }

        void INetListener<IMessage>.OnNetworkReceive(in List<IMessage> msgs)
        {
            foreach (var msg in msgs)
            {
                Debug.Log($"====Receive: {msg}");
            }
        }

        void INetListener<IMessage>.OnPeerConnectSuccess()
        {
            Debug.Log($"connected    {Ip}:{Port}");
        }
        void INetListener<IMessage>.OnPeerConnectFailed(Exception e)
        {
            Debug.LogWarning($"connect failed: {e.ToString()}");
        }
        void INetListener<IMessage>.OnPeerDisconnected(Exception e)
        {
            Debug.LogError($"network error: {e.ToString()}");
        }
        void INetListener<IMessage>.OnPeerClose()
        {
            Debug.Log("connect shutdown");
        }

        int INetListener<IMessage>.sendBufferSize { get { return 4096; } }
        int INetListener<IMessage>.receiveBufferSize { get { return 2048; } }
        IPacket<IMessage> INetListener<IMessage>.parser { get { return s_Parser; } }

        async Task AutoSending()
        {
            int index = 0;
            System.Random r = new System.Random();
            while (UnityEngine.Application.isPlaying)
            {
                while (!m_Quit && m_NetClient?.state == ConnectState.Connected)
                {
                    //string data = "Hello world..." + index++;
                    // Debug.Log("\n Sending...:" + data);
                    //++index;
                    StoreRequest msg = new StoreRequest();
                    msg.Name = "1233";
                    msg.Num = 3;
                    msg.Result = 4;
                    if (index % 2 == 0)
                        msg.MyList.Add("22222222222");
                    if (index % 3 == 0)
                        msg.MyList.Add("33333333333333");
                    UnityEngine.Debug.LogWarning("");
                    if (!m_NetClient.SendData(msg))
                        break;

                    await Task.Yield();
                }
                await Task.Yield();
            }
        }

        void OnApplicationFocus(bool isFocus)
        {
            Debug.Log($"OnApplicationFocus      isFocus: {isFocus}       {System.DateTime.Now}");
        }

        void OnApplicationPause(bool isPause)
        {
            Debug.Log($"======OnApplicationPause      isPause: {isPause}       {System.DateTime.Now}");
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(NetManager))]
    public class NetManager_Inspector : Editor
    {
        SerializedProperty m_IsPersistentProp;
        
        private void OnEnable()
        {
            m_IsPersistentProp = serializedObject.FindProperty("isPersistent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_IsPersistentProp);

            EditorGUI.BeginChangeCheck();

            ((NetManager)target).Ip = EditorGUILayout.TextField("Ip", ((NetManager)target).Ip);
            ((NetManager)target).Port = EditorGUILayout.IntField("Port", ((NetManager)target).Port);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            }

            if (GUILayout.Button("Connect"))
            {
                ((NetManager)target).Connect();
            }
            if (GUILayout.Button("Disconnect"))
            {
                ((NetManager)target).Disconnect();
            }
            if (GUILayout.Button("Reconnect"))
            {
                ((NetManager)target).Reconnect();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}