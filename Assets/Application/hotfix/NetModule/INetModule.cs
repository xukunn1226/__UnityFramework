using Application.Runtime;
using Google.Protobuf;

namespace Application.Logic
{    
    public interface INetModule
    {
        // 取得模块ID
        public abstract int GetModuleID();

        // 初始化所有函数
        public abstract void InitMsgFunc();

        // 处理消息的入口
        public abstract void OnMessage(NetMsgData data);

        // 发送信息
        public abstract void SendMsg(int msgid, IMessage msg);
    }
}