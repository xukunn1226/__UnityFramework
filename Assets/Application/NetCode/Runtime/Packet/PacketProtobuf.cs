using System.IO;
using Framework.NetWork.Log;
using Google.Protobuf;
using NetProtocol;
using Framework.NetWork;

/// <summary>
/// Packet format: [length | message]
/// </summary>
public class PacketProtobuf : IPacket<IMessage>
{
    private const int m_HeadLength = 2;         // 报头数据占用的字节长度

    public bool Deserialize(in byte[] data, int offset, int length, out int realLength, out IMessage msg)
    {
        // 解析报头数据，占两字节
        if (length <= m_HeadLength)
        {
            realLength = 0;
            msg = default(IMessage);
            return false;
        }

        // 报头数据：整个packet的字节流长度
        ushort packetLength = System.BitConverter.ToUInt16(data, offset);
        if (packetLength <= m_HeadLength)
        {
            Trace.Error("msg head length <= 2");

            realLength = 0;
            msg = default(IMessage);
            return false;
        }

        // 接收到的字节流长度(length)小于消息字节数(packetLength)，数据量不够解析，继续等待数据
        if (length < packetLength)
        {
            realLength = 0;
            msg = default(IMessage);
            return false;
        }

        msg = new StoreRequest();
        msg.MergeFrom(data, offset + m_HeadLength, packetLength - m_HeadLength);
        realLength = packetLength;
        return true;
    }

    public byte[] Serialize(IMessage msg)
    {
        // assemble packet            
        int packetLength = msg.CalculateSize() + m_HeadLength;                  // proto buf消息长度
        byte[] buf = new byte[packetLength];

        System.BitConverter.GetBytes((ushort)packetLength).CopyTo(buf, 0);      // copy "len" to packet
        msg.ToByteArray().CopyTo(buf, m_HeadLength);                            // copy "data" to packet

        return buf;
    }

    public void Serialize(IMessage msg, MemoryStream output)
    {
        ushort packetLength = (ushort)(msg.CalculateSize() + m_HeadLength);

        // write "packetLength" to packet
        output.WriteByte((byte)packetLength);
        output.WriteByte((byte)(packetLength >> 8));

        // write "data" to packet
        msg.WriteTo(output);
    }

    public int CalculateSize(IMessage msg)
    {
        return m_HeadLength + msg.CalculateSize();
    }
}