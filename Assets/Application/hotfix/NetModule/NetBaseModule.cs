using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Runtime
{
    public abstract class NetBaseModule : INetModule
    {
        public delegate void MsgHandler(NetMsgData data);
        protected NetModuleManager m_manager;
        public NetBaseModule(NetModuleManager manager)
        {
            m_manager = manager;
        }

        public abstract int GetModuleID();
        public abstract void InitMsgFunc();

        protected Dictionary<int, MsgHandler> dicDelegates = new Dictionary<int, MsgHandler>();

        protected NetMsgReceiver m_netReceiver = new NetMsgReceiver();

        protected void AddReceiver(int msgId, NetRecvHandler handler)
        {
            m_netReceiver.Add(msgId, handler,m_manager);
        }

        public void OnMessage(NetMsgData data)
        {
            int msgid = data.GetMsgID();
            if (!dicDelegates.ContainsKey(msgid))
            {
                return;
            }

            dicDelegates[msgid](data);
        }

        public void SendMsg(int msgid, IMessage msg)
        {
            msgid = NetModuleManager.MakeMsgID(GetModuleID(), msgid);
            NetManager.Instance.SendData(msgid, msg);
        }
    }
}
