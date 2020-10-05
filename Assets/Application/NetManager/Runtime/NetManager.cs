using System;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using System.Threading.Tasks;
using NetProtocol;
using Framework.NetWork;

public class NetManager : MonoBehaviour, INetListener<IMessage>
{
    static private IPacket<IMessage> s_Parser = new PacketProtobuf();
    private NetClient<IMessage> m_NetClient;

    private bool m_Quit;

    void Awake()
    {
        m_NetClient = new NetClient<IMessage>(this);        
    }

    async void OnEnable()
    {
        await m_NetClient.Connect("192.168.5.3", 11000);

        await AutoSending();
    }

    void Update()
    {
        m_NetClient?.Tick();
    }

    void OnDisable()
    {
        m_Quit = true;
        m_NetClient.Close(true);
    }

    void INetListener<IMessage>.OnNetworkReceive(in List<IMessage> msgs)
    {
        foreach(var msg in msgs)
        {
            // Debug.Log($"====Receive: {msg}");
        }
    }

    void INetListener<IMessage>.OnPeerConnected()
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
    
    int INetListener<IMessage>.sendBufferSize { get { return 4096; } }
    int INetListener<IMessage>.receiveBufferSize { get { return 2048; } }
    IPacket<IMessage> INetListener<IMessage>.parser { get { return s_Parser; } }

    async Task AutoSending()
    {
        int index = 0;
        while (!m_Quit && m_NetClient.state == ConnectState.Connected)
        {
            string data = "Hello world..." + index++;
            // Debug.Log("\n Sending...:" + data);
            StoreRequest msg = new StoreRequest();
            msg.Name = "1233";
            msg.Num = 3;
            msg.Result = 4;
            if (index % 2 == 0)
                msg.MyList.Add("22222222222");
            if (index % 3 == 0)
                msg.MyList.Add("33333333333333");

            if(!m_NetClient.SendData(msg))
                break;
            
            await Task.Delay(10);
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
