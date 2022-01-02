using Framework.Core;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Runtime;

namespace Application.Logic
{
    public delegate void NetRecvHandler(IMessage msg, NetMsgData data);

    public class NetMsgReceiver 
    {
        private Dictionary<int, NetRecvHandler> m_addedHandlers = new Dictionary<int, NetRecvHandler>();


        public void Add(int msgId, NetRecvHandler handler)
        {
            Add(msgId, handler, NetModuleManager.Instance);
        }

        public void Add(int msgId, NetRecvHandler handler, NetModuleManager manager)
        {
            if(m_addedHandlers.TryGetValue(msgId,out NetRecvHandler lastHandler))
            {
                GameDebug.LogError($"Can not add handler with msg id which already exist in the receiver");
                manager.RemoveReceiver(msgId, lastHandler);
            }
            m_addedHandlers.Add(msgId, handler);
            manager.AddReceiver(msgId, handler);
        }

        public void Remove(int msgId)
        {
            if (m_addedHandlers.TryGetValue(msgId, out NetRecvHandler handler))
            {
                NetModuleManager.Instance.RemoveReceiver(msgId, handler);
            }
            else
            {
                GameDebug.LogError($"Can not found handler with msgid [{msgId}]");
            }
        }


        public void Dispose()
        {
            foreach(var kv in m_addedHandlers)
            {
                NetModuleManager.Instance.RemoveReceiver(kv.Key, kv.Value);

            }


        }


    }
}
