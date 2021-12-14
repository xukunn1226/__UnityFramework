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
    public class NetManager : SingletonMono<NetManager>, INetListener<NetMsgData>
    {
        static private IPacket<NetMsgData>      s_Parser    = new PacketProtobuf();
        private NetClient<NetMsgData>           m_NetClient;
        private INetManagerListener<NetMsgData> m_Listener;

        public string                           Ip          = "192.168.2.7";
        public int                              Port        = 11000;

        public ConnectState state
        {
            get
            {
                return m_NetClient?.state ?? ConnectState.Disconnected;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            m_NetClient = new NetClient<NetMsgData>(this);
        }

        protected override void OnDestroy()
        {
            Shutdown();
            base.OnDestroy();
        }

        void Update()
        {
            m_NetClient?.Tick();

            //if(Input.GetKeyDown(KeyCode.Space))
            //if (state == ConnectState.Connected)
            //{
            //    // 测试发数据
            //    SendData(1);
            //}
        }

        public void SetListener(INetManagerListener<NetMsgData> listener)
        {
            m_Listener = listener;
        }

        async public Task Connect(string ip, int port)
        {
            Ip = ip;
            Port = port;
            await Connect();
        }

        async public Task Connect()
        {
            await m_NetClient?.Connect(Ip, Port);
        }

        public void Shutdown()
        {
            m_NetClient?.Shutdown();
        }

        async public Task Reconnect()
        {
            await m_NetClient?.Reconnect();
        }

        public void SendData(int msgid)
        {
            NetMsgData msg = NetMsgData.Get();
            msg.MsgID = msgid;

            m_NetClient?.SendData(msgid, msg);
            NetMsgData.Release(msg);
        }

        public void SendData(int msgid, IMessage data)
        {
            int nLen = data.CalculateSize();
            byte[] byData = data.ToByteArray();
            
            NetMsgData msg = NetMsgData.Get();
            msg.MsgID = msgid;
            msg.CopyFrom(byData, 0, byData.Length);

            m_NetClient?.SendData(msgid, msg);
            NetMsgData.Release(msg);
        }

        void INetListener<NetMsgData>.OnNetworkReceive(in List<NetMsgData> msgs)
        {
            //Debug.Log($"receive data: {msgs.Count}");
            m_Listener?.OnNetworkReceive(msgs);
        }

        // 连接成功
        void INetListener<NetMsgData>.OnPeerConnectSuccess()
        {
            //Debug.Log($"NetManager: connect success!");
            m_Listener?.OnPeerConnectSuccess();
        }

        // 连接失败
        void INetListener<NetMsgData>.OnPeerConnectFailed(Exception e)
        {
            //Debug.Log($"NetManager: connect failed {e.Message}");
            m_Listener?.OnPeerConnectFailed(e);
        }

        // 异常断开连接
        void INetListener<NetMsgData>.OnPeerDisconnected(Exception e)
        {
            //Debug.Log($"NetManager: connect disconnected {e.Message}");
            m_Listener?.OnPeerDisconnected(e);
        }

        // 主动断开连接
        void INetListener<NetMsgData>.OnPeerClose()
        {
            //Debug.Log($"NetManager: connect close!");
            m_Listener?.OnPeerClose();
        }

        int INetListener<NetMsgData>.sendBufferSize       { get { return 8192; } }
        int INetListener<NetMsgData>.receiveBufferSize    { get { return 2048; } }
        IPacket<NetMsgData> INetListener<NetMsgData>.parser { get { return s_Parser; } }
    }

    public interface INetManagerListener<TMessage> where TMessage : class
    {
        void OnPeerConnectSuccess();
        void OnPeerConnectFailed(Exception e);
        void OnPeerDisconnected(Exception e);
        void OnPeerClose();
        void OnNetworkReceive(in List<TMessage> msgs);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(NetManager))]
    public class NetManager_Inspector : UnityEditor.Editor
    {
        async public override void OnInspectorGUI()
        {
            serializedObject.Update();

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
                await ((NetManager)target).Connect();
            }
            if (GUILayout.Button("Disconnect"))
            {
                ((NetManager)target).Shutdown();
            }
            if (GUILayout.Button("Reconnect"))
            {
                await ((NetManager)target).Reconnect();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}