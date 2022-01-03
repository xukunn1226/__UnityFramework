﻿using System.IO;
using System.Text;
using Framework.NetWork;

namespace Application.Runtime
{
    public class PacketString : IPacket<string>
    {
        public bool Deserialize(byte[] data, int offset, int length, out int realLength, out string msg)
        {
            realLength = length;
            msg = Encoding.ASCII.GetString(data, offset, length);
            return true;
        }

        public byte[] Serialize(int msgid, string msg)
        {
            return Encoding.ASCII.GetBytes(msg);
        }

        public void Serialize(int msgid, string msg, MemoryStream output)
        {
            byte[] data = Encoding.ASCII.GetBytes(msg);
            output.Write(data, 0, data.Length);
        }

        public int GetTotalPacketLen(string msg)
        {
            return Encoding.ASCII.GetByteCount(msg);
        }
    }
}