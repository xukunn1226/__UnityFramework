﻿using Framework.NetWork;

namespace Application.Runtime
{
    public class PacketBinary : IPacket<byte[]>
    {
        public bool Deserialize(byte[] data, int offset, int length, out int realLength, out byte[] msg)
        {
            realLength = 0;
            msg = null;
            return false;
        }

        public byte[] Serialize(int msgid, byte[] msg)
        {
            return null;
        }

        public void Serialize(int msgid, byte[] msg, System.IO.MemoryStream output)
        { }

        public int GetTotalPacketLen(byte[] msg)
        {
            return 0;
        }
    }
}