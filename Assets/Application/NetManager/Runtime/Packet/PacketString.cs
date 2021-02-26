using System.IO;
using System.Text;
using Framework.NetWork;

namespace Application.Runtime
{
public class PacketString : IPacket<string>
{
    public bool Deserialize(in byte[] data, int offset, int length, out int realLength, out string msg)
    {
        realLength = length;
        msg = Encoding.ASCII.GetString(data, offset, length);
        return true;
    }

    public byte[] Serialize(string msg)
    {
        return Encoding.ASCII.GetBytes(msg);
    }

    public void Serialize(string msg, MemoryStream output)
    {
        byte[] data = Encoding.ASCII.GetBytes(msg);
        output.Write(data, 0, data.Length);
    }

    public int CalculateSize(string msg)
    {
        return Encoding.ASCII.GetByteCount(msg);
    }
}
}