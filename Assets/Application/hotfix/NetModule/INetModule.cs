
using Google.Protobuf;

namespace Application.Runtime
{
    public delegate void MsgHandler(NetMsgData data);
    public interface INetModule
    {
        //ȡ��ģ��ID
        public abstract int GetModuleID();

        //��ʼ�����к���
        public abstract void InitMsgFunc();

        //������Ϣ�����
        public abstract void OnMessage(NetMsgData data);

        //������Ϣ
        public abstract void SendMsg(int msgid, IMessage msg);

    }
}

