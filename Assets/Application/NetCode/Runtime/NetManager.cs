using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Framework.NetWork.Log;
using Framework.NetWork;

/// <summary>
/// 负责网络数据发送，主线程同步接收数据，子线程异步发送数据
/// 测试用例：
/// 1、连接服务器失败       [PASS]
/// 6、主动/被动断开连接         [PASS]
/// 2、关闭服务器，再发送消息   [PASS]
/// 3、客户端异常断开连接（参数错误、断电等）
/// 4、断线重连
/// 5、任何异常情况能否退出WriteAsync    
/// 7、持续的发送协议时重复1-6
/// 8、测试RequestBufferToWrite
/// 9、同时开启wifi，4G时，先断开wifi，再断开4G，然后再逐一连接
/// </summary>
public class NetManager<TMessage> where TMessage : class
{
    private NetClient           m_NetClient;
    private IPacket<TMessage>   m_Parser;
    private List<TMessage>      m_MessageList = new List<TMessage>();

    protected NetManager() { }

    public NetManager(IPacket<TMessage> parser)
    {
        Trace.EnableConsole();
        m_Parser = parser;
    }

    async public Task Connect(string host, int port)
    {
        m_NetClient = new NetClient(host, port, 4096, 2048, OnConnected, OnDisconnected);
        await m_NetClient.Connect();
    }

    async public Task Reconnect()
    {
        if (m_NetClient == null)
            throw new ArgumentNullException();
        await m_NetClient.Reconnect();
    }

    public void Close(bool isImmediately = false)
    {
        if (m_NetClient == null)
            throw new ArgumentNullException();
        m_NetClient.Close(isImmediately);
    }

    public ConnectState state { get { return m_NetClient?.state ?? ConnectState.Disconnected; } }

    private void OnConnected(Exception e)
    {
        if(e != null)
        {
            Trace.Debug(e.ToString());
        }
        else
        {
            Trace.Debug($"connect servier... ");
        }
    }

    private void OnDisconnected(Exception e)
    {
        if(e != null)
            Trace.Debug(e.ToString());
        Trace.Debug("...Disconnected");
    }

    public void Tick()
    {
        if (m_NetClient == null)
            return;

        m_NetClient.Tick();
        ReceiveData();
    }

    public bool SendData(TMessage data)
    {
        // method 1. 序列化到新的空间，有GC
        //byte[] buf = m_Parser.Serialize(data);
        //m_NetClient.Send(buf);

        // method 2. 序列化到stream，因buff已预先分配、循环利用，无GC
        int length = m_Parser.CalculateSize(data);
        MemoryStream stream;
        if(m_NetClient.RequestBufferToWrite(length, out stream))
        {
            m_Parser.Serialize(data, stream);
            m_NetClient.FinishBufferWriting(length);
            return true;
        }
        return false;
    }

    private void ReceiveData()
    {
        int offset;
        int length;     // 已接收的消息长度
        ref readonly byte[] data = ref m_NetClient.FetchBufferToRead(out offset, out length);
        if (length == 0)
            return;

        int totalRealLength = 0;            // 实际解析的总长度(byte)
        int startOffset = offset;
        int totalLength = length;
        m_MessageList.Clear();
        while (true)
        {
            int realLength;                 // 单次解析的长度(byte)
            TMessage msg;
            bool success = m_Parser.Deserialize(in data, startOffset, totalLength, out realLength, out msg);
            if (success)
                m_MessageList.Add(msg);

            totalRealLength += realLength;
            startOffset += realLength;
            totalLength -= realLength;

            if (!success || totalRealLength == length)
                break;                      // 解析失败或者已接收的消息长度解析完了
        }
        m_NetClient.FinishRead(totalRealLength);

        // dispatch
        // foreach (var msg in m_MessageList)
        // {
        //     //Dispatch(msg);
        //     Trace.Debug($"Receive:=== {msg}");
        // }
    }
}