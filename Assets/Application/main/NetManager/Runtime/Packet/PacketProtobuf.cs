using System.IO;
using Framework.NetWork;

namespace Application.Runtime
{
    /// <summary>
    /// Packet format: [length | message]
    /// </summary>
    public class PacketProtobuf : IPacket<NetMsgData>
    {
        // private const int m_HeadLength = 2;         // 报头数据占用的字节长度

        // public bool Deserialize(in byte[] data, int offset, int length, out int realLength, out IMessage msg)
        // {
        //     // 解析报头数据，占两字节
        //     if (length <= m_HeadLength)
        //     {
        //         realLength = 0;
        //         msg = default(IMessage);
        //         return false;
        //     }

        //     // 报头数据：整个packet的字节流长度
        //     ushort packetLength = System.BitConverter.ToUInt16(data, offset);
        //     if (packetLength <= m_HeadLength)
        //     {
        //         Trace.Error("msg head length <= 2");

        //         realLength = 0;
        //         msg = default(IMessage);
        //         return false;
        //     }

        //     // 接收到的字节流长度(length)小于消息字节数(packetLength)，数据量不够解析，继续等待数据
        //     if (length < packetLength)
        //     {
        //         realLength = 0;
        //         msg = default(IMessage);
        //         return false;
        //     }

        //     msg = new StoreRequest();
        //     msg.MergeFrom(data, offset + m_HeadLength, packetLength - m_HeadLength);
        //     realLength = packetLength;
        //     return true;
        // }

        // public byte[] Serialize(IMessage msg)
        // {
        //     // assemble packet            
        //     int packetLength = msg.CalculateSize() + m_HeadLength;                  // proto buf消息长度
        //     byte[] buf = new byte[packetLength];

        //     System.BitConverter.GetBytes((ushort)packetLength).CopyTo(buf, 0);      // copy "len" to packet
        //     msg.ToByteArray().CopyTo(buf, m_HeadLength);                            // copy "data" to packet

        //     return buf;
        // }

        // public void Serialize(IMessage msg, MemoryStream output)
        // {
        //     ushort packetLength = (ushort)(msg.CalculateSize() + m_HeadLength);

        //     // write "packetLength" to packet
        //     output.WriteByte((byte)packetLength);
        //     output.WriteByte((byte)(packetLength >> 8));

        //     // write "data" to packet
        //     msg.WriteTo(output);
        // }

        // public int CalculateSize(IMessage msg)
        // {
        //     return m_HeadLength + msg.CalculateSize();
        // }

        private const int m_HeadLength = 8;         // 报头数据占用的字节长度(前4个字节是msgdata长度，后4个字节是msgid)

        public bool Deserialize(byte[] data, int offset, int length, out int realLength, out NetMsgData msg)
        {
            // 解析报头数据
            if (length < m_HeadLength)
            {
                realLength = 0;
                msg = default(NetMsgData);
                return false;
            }

            // packetLength：msg data's length
            int packetLength = (int)System.BitConverter.ToUInt32(data, offset);
            if (packetLength >= NetMsgData.MsgMaxSize)
            {
                UnityEngine.Debug.LogError($"packetLength >= NetMsgData.MsgMaxSize, len {packetLength}");
                realLength = 0;
                msg = default(NetMsgData);
                return false;
            }

            // 接收到的字节流长度(length)小于消息字节数(packetLength)，数据量不够解析，继续等待数据
            if (length < (packetLength + m_HeadLength))
            {
                realLength = 0;
                msg = default(NetMsgData);
                return false;
            }

            uint msgid = System.BitConverter.ToUInt32(data, offset + 4);
            msg = NetMsgData.Get();
            msg.MsgID = (int)msgid;
            msg.CopyFrom(data, offset + m_HeadLength, packetLength);

            realLength = packetLength + m_HeadLength;
            return true;
        }

        public byte[] Serialize(int msgid, NetMsgData msg)
        {

            // assemble packet            

            /*
            int packetLength = CalculateSize(msg);                  // proto buf消息长度
            byte[] buf = new byte[packetLength];

            System.BitConverter.GetBytes(packetLength).CopyTo(buf, 0);      // copy "len" to packet
            System.BitConverter.GetBytes(msgid).CopyTo(buf, sizeof(int));   // copy msgid to packer
            msg.ToByteArray().CopyTo(buf, m_HeadLength);                            // copy "data" to packet
            */


            //这个函数有new,但发现这个函数其实没有被调用过。先注释掉吧
            /*
            int totalLen = msg.MsgLen + m_HeadLength;
            byte[] buf = new byte[totalLen];
            System.BitConverter.GetBytes(msg.MsgLen).CopyTo(buf, 0);
            System.BitConverter.GetBytes(msgid).CopyTo(buf, sizeof(int));
            msg.MsgData.CopyTo(buf, 8);

            return buf;
            */
            return null;
        }

        public void Serialize(int msgid, NetMsgData msg, MemoryStream output)
        {
            int packetLength = msg.MsgLen;

            // write "packetLength" to packet
            output.WriteByte((byte)packetLength);
            output.WriteByte((byte)(packetLength >> 8));
            output.WriteByte((byte)(packetLength >> 16));
            output.WriteByte((byte)(packetLength >> 24));

            // write "msgid" to packet
            output.WriteByte((byte)msgid);
            output.WriteByte((byte)(msgid >> 8));
            output.WriteByte((byte)(msgid >> 16));
            output.WriteByte((byte)(msgid >> 24));

            // write "data" to packet
            output.Write(msg.MsgData, 0, msg.MsgLen);
        }

        public int GetTotalPacketLen(NetMsgData msg)
        {
            return msg.MsgLen + m_HeadLength;
        }
    }
}