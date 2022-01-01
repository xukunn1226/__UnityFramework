using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;
using Application.Runtime;

namespace Application.HotFix
{
    public class NetModuleLobby : NetBaseModule
    {
        public NetModuleLobby(NetModuleManager manager) : base(manager)
        {
        }

        public override int GetModuleID()
        {
            return (int)ModuleType.ModuleLobby;
        }

        public override void InitMsgFunc()
        {
            //dicDelegates.Add((int)LobbyMsgID.EnterLobby, OnMessageEnterLobbyAck);
            //dicDelegates.Add((int)LobbyMsgID.CreateCharactor, OnCreateCharactorAck);

            AddReceiver(NetMsgIds.LobbyModule.EnterLobby, OnMessageEnterLobbyAck);
            AddReceiver(NetMsgIds.LobbyModule.CreateCharactor, OnCreateCharactorAck);

        }

        private void OnMessageEnterLobbyAck(IMessage msg,NetMsgData data)
        {
            //EnterLobbyAck ack = new EnterLobbyAck();
            //ack.MergeFrom(data.MsgData, 0, data.MsgLen);
            var ack = (EnterLobbyAck)msg;

            //����]�н�ɫ���l�̈́��ǵ�Ո��
            if (ack.ErrorCode == ErrorCode.ServerLobbyHasnotRole)
            {
                //������Ҫ��ʾ��ɫ��UI,��ʱû��

                var req = new CreateCharactorReq();
                req.SelRoleConfigID = "1";
                NetReqs.Send(req);
                //int msgid = NetModuleManager.MakeMsgID((int)ModuleType.ModuleLobby, (int)LobbyMsgID.CreateCharactor);
                //NetManager.Instance.SendData(msgid, req);
                return;
            }
            else
            {
                NetReqs.Send(NetMsgIds.DungeonModule.DungeonInfo);
            }
            
        }

        private void OnCreateCharactorAck(IMessage msg, NetMsgData data)
        {
            //CreateCharactorAck ack = new CreateCharactorAck();
            //ack.MergeFrom(data.MsgData, 0, data.MsgLen);
            var ack = (CreateCharactorAck)msg;

            if (ack.ErrorCode != ErrorCode.None)
            {
                Debug.LogError($"OnCreateCharactorAck: error {ack.ErrorCode}");
            }
            else
            {
                Debug.LogError("create charactor success ~~~~");


            }
        }

        
    }

}
