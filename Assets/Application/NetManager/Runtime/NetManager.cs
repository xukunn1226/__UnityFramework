﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using System.Threading.Tasks;
using NetProtocol;
using Framework.NetWork;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NetManager : MonoBehaviour, INetListener<IMessage>
{
    static private IPacket<IMessage>    s_Parser        = new PacketProtobuf();
    private NetClient<IMessage>         m_NetClient;

    private bool m_Quit;

    public string   Ip      = "192.168.5.14";
    public int      Port    = 11000;

    void Awake()
    {
        m_NetClient = new NetClient<IMessage>(this);        
    }

    async void OnEnable()
    {
        m_Quit = false;

        await AutoSending();
    }

    void Update()
    {
        m_NetClient?.Tick();
    }

    void OnDisable()
    {
        m_Quit = true;
        m_NetClient.Close();
    }

    async public void Connect()
    {
        m_Quit = false;
        await m_NetClient.Connect(Ip, Port);
    }

    public void Disconnect()
    {
        m_Quit = true;
        m_NetClient?.Close();
    }

    public void Reconnect()
    {
        m_NetClient?.Reconnect();
    }

    void INetListener<IMessage>.OnNetworkReceive(in List<IMessage> msgs)
    {
        foreach(var msg in msgs)
        {
            //Debug.Log($"====Receive: {msg}");
        }
    }

    void INetListener<IMessage>.OnPeerConnectSuccess()
    {
        Debug.Log("connected");
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
    
    int INetListener<IMessage>.sendBufferSize       { get { return 4096; } }
    int INetListener<IMessage>.receiveBufferSize    { get { return 2048; } }
    IPacket<IMessage> INetListener<IMessage>.parser { get { return s_Parser; } }

    async Task AutoSending()
    {
        int index = 0;
        System.Random r = new System.Random();
        while (true)
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
    public override void OnInspectorGUI()
    {
        ((NetManager)target).Ip = EditorGUILayout.TextField("Ip", ((NetManager)target).Ip);
        ((NetManager)target).Port = EditorGUILayout.IntField("Port", ((NetManager)target).Port);

        if (GUILayout.Button("Connect"))
        {
            ((NetManager)target).Connect();
        }
        if(GUILayout.Button("Disconnect"))
        {
            ((NetManager)target).Disconnect();
        }
        if (GUILayout.Button("Reconnect"))
        {
            ((NetManager)target).Reconnect();
        }
    }
}
#endif