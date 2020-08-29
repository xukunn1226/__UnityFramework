using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.NetWork
{
    public class MessageDefines
    {
        private static Dictionary<ushort, System.Type> s_dicMsgId2Msg = new Dictionary<ushort, System.Type>();
        private static Dictionary<System.Type, ushort> s_dicMsg2MsgId = new Dictionary<System.Type, ushort>();

        public static ushort GetMessageId(System.Type t)
        {
            ushort iMsgId = 0;
            s_dicMsg2MsgId.TryGetValue(t, out iMsgId);
            return iMsgId;
        }

        public static System.Type GetMessageType(ushort iMsgId)
        {
            System.Type msgType = null;
            s_dicMsgId2Msg.TryGetValue(iMsgId, out msgType);
            return msgType;
        }

        static private void RecodeType(ushort msgID, System.Type t)
        {
            s_dicMsgId2Msg[msgID] = t;
            s_dicMsg2MsgId[t] = msgID;
        }

        static public void Initialzie()
        {
            RecodeType(1, typeof(LoginReq));
        }
    }

    public class LoginReq
    {
        public string m_Username;
        public string m_Password;
    }
}