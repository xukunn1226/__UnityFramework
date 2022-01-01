using Framework.Core;
using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using Application.Runtime;

namespace Application.HotFix
{
    public class NetModuleManager : Singleton<NetModuleManager>
    {
        private const int                       m_ModuleCount       = 10;
        private INetModule[]                    m_NetModules        = null;
        private Dictionary<string, INetModule>  m_NetModulesByName  = new Dictionary<string, INetModule>();

        //初始化所有的网络模块
        protected override void InternalInit()
        {
            m_NetModules = new INetModule[m_ModuleCount];
            RegisterModule(new NetModuleLogin(this));
            RegisterModule(new NetModuleLobby(this));
            RegisterModule(new NetModuleDungeon(this));
            RegisterModule(new NetModuleBattleVerify(this));
        }

        private void RegisterModule(INetModule module)
        {
            if (m_NetModules[module.GetModuleID()] != null)
            {
                UnityEngine.Debug.LogError($"RegisterModule msgid {module.GetModuleID()} already exist!");
                return;
            }
            
            m_NetModules[module.GetModuleID()] = module;
            m_NetModulesByName[module.GetType().Name] = module;
            module.InitMsgFunc();
        }

        public INetModule GetNetModule(int nModuleID)
        {
            INetModule module = m_NetModules[nModuleID];
            return module;
        }

        public T GetNetModule<T>()
        {
            string name = typeof(T).Name;
            if (m_NetModulesByName.TryGetValue(name, out INetModule netModule))
            {
                return (T)netModule;
            }
            return default(T);
        }

#if DEBUG
        private static Dictionary<int, NetMsgDefines.NetMsgDefineInfo> m_msgDefinesDict = new Dictionary<int, NetMsgDefines.NetMsgDefineInfo>();
        private static bool m_isInitDict = false;
#endif
        private static string GetMsgName(int msgId)
        {
#if DEBUG
            if (!m_isInitDict)
            {
                foreach(var msgInfo in NetMsgDefines.msgs)
                {
                    m_msgDefinesDict[msgInfo.msgId] = msgInfo;
                }
            }
            if(m_msgDefinesDict.TryGetValue(msgId,out NetMsgDefines.NetMsgDefineInfo outInfo))
            {
                return $"{outInfo.moduleName}.{outInfo.msgName}";
            }
            else
            {
                GameDebug.LogError($"Unkown msg [{msgId}]");
                return "Unknown";
            }
#else
            return string.Empty;
#endif
        }
        //将网络协议 分发给不同的模块
        public void DispatchMsg(NetMsgData data)
        {
#if DEBUG
            if (!m_ignoreMsgIgds.Contains(data.MsgID))
            {
                GameDebug.Log($"[Net]Receiving msg [{GetMsgName(data.MsgID)}]");
            }
#endif
            int moduleid = data.GetTypeID();

            if (m_NetModules[moduleid] != null)
            {
                m_NetModules[moduleid].OnMessage(data);
            }

            OnReceive(data);
        }

        public static int MakeMsgID(int typeid, int msgid)
        {
            return (typeid << 16 | msgid);
        }

        public static int MakeMsgID(ModuleType typeid, int msgid)
        {
            return MakeMsgID((int)typeid,msgid);
        }

        public void SendData(int msgid)
        {
#if DEBUG
            GameDebug.Log($"[Net]Sending msg [{GetMsgName(msgid)}]");
#endif
            NetManager.Instance.SendData(msgid);
        }

        private static HashSet<int> m_ignoreMsgIgds = new HashSet<int>()
        {
            NetMsgIds.DungeonModule.SyncRoleInfo,
            NetMsgIds.DungeonModule.SyncEntityInfo,
        };

        public void SendData(int msgid, IMessage req)
        {
#if DEBUG
            if (!m_ignoreMsgIgds.Contains(msgid))
            {
                GameDebug.Log($"[Net]Sending msg [{GetMsgName(msgid)}]");
            }
#endif
            NetManager.Instance.SendData(msgid, req);
        }

#region Msg Dispatcher Handlers
        private Dictionary<int, List<NetRecvHandler>> m_handlers = new Dictionary<int, List<NetRecvHandler>>();

        public void AddReceiver(int msgId, NetRecvHandler handler)
        {
            if(!m_handlers.TryGetValue(msgId,out List<NetRecvHandler> handlers))
            {
                handlers = new List<NetRecvHandler>();
                m_handlers.Add(msgId, handlers);
            }
            handlers.Add(handler);
        }

        public void RemoveReceiver(int msgId, NetRecvHandler handler)
        {
            if (!m_handlers.TryGetValue(msgId, out List<NetRecvHandler> handlers))
            {
                handlers = new List<NetRecvHandler>();
                m_handlers.Add(msgId, handlers);
            }
            handlers.Remove(handler);
        }

        private void OnReceive(NetMsgData data)
        {
            int msgId = data.MsgID;
            if (m_handlers.TryGetValue(msgId, out List<NetRecvHandler> handlers))
            {
                // 提前convert为msg,避免多次convert带来的消耗
                var ack = NetAcks.Convert(data);
                if(ack == null)
                {
#if DEBUG
                    GameDebug.Log($"received msg [{GetMsgName(msgId)}] parsing error.");
#endif
                }
                foreach (var handler in handlers)
                {
                    handler(ack,data);
                }
            }

        }


        //public static T Convert<T>(NetMsgData data) where T : IMessage, new()
        //{
        //    T msg = new T();
        //    msg.MergeFrom(data.MsgData, 0, data.MsgLen);


        //    return msg;
        //}

        #endregion

    }
}