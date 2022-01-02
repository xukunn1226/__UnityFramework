using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using Application.Runtime;

namespace Application.Logic
{
    public class NetModuleLogin : NetBaseModule
    {
        public NetModuleLogin(NetModuleManager manager) : base(manager)
        {
        }

        public override int GetModuleID()
        {
            return (int)ModuleType.ModuleLogin;
        }

        public override void InitMsgFunc()
        {
            //dicDelegates.Add((int)LoginMsgID.Login, OnLoginAck);

            AddReceiver(NetMsgIds.LoginModule.Login, OnLoginAck);
        }

        public void OnConnected()
        {

        }

        private void OnLoginAck(IMessage msg,NetMsgData data)
        {
            //msg.MergeFrom(data, offset + m_HeadLength, packetLength - m_HeadLength);
            //LoginAck ack = new LoginAck();
            //ack.MergeFrom(data.MsgData, 0, data.MsgLen);

            var ack = (LoginAck)msg;
            //Framework.NetWork.Log.Trace.Debug($" dungeon server ip: {ack.DungeonServerIP}, prot : {ack.DungeonPort}");

            if (ack.ErrorCode != (int)ErrorCode.None)
            {
                Debug.LogError($"OnLoginAck error {ack.ErrorCode}");
                return;
            }

            //����������
            NetReqs.Send(NetMsgIds.LobbyModule.EnterLobby);
            EnterLobbyReq req = new EnterLobbyReq();
            int msgid = NetModuleManager.MakeMsgID((int)ModuleType.ModuleLobby, (int)LobbyMsgID.EnterLobby);
            NetManager.Instance.SendData(msgid, req);
        }
    }

}
