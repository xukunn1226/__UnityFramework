using Google.Protobuf;
using System;
using System.Collections.Generic;
using Application.Runtime;

namespace Application.HotFix
{

#region NetAcks
   public static class NetAcks
   {
        public delegate IMessage Creator();

        public static IMessage Convert(NetMsgData data)
        {
            int msgId = data.MsgID;
            if(m_converters.TryGetValue(msgId,out Creator creator))
            {
                var ack = creator();
                ack.MergeFrom(data.MsgData, 0, data.MsgLen);
                return ack;
            }
            return null;
        }

        private static Dictionary<int, Creator> m_converters = new Dictionary<int, Creator>()
        {
        // acks for module None
        // acks for module Login
        { 65537,CreateLoginAck},
        // acks for module Lobby
        { 131072,CreateEnterLobbyAck},
        { 131073,CreateCreateCharactorAck},
        // acks for module Dungeon
        { 196608,CreateDungeonInfoAck},
        { 196609,CreateEnterDungeonAck},
        { 196610,CreateExitDungeonAck},
        { 196611,CreateClientReadyAck},
        { 196612,CreateSyncRoleInfoAck},
        { 196613,CreateSyncRefreshEntityAck},
        { 196614,CreateSyncEntityInfoAck},
        { 196615,CreatePreUseSkillStartAck},
        { 196616,CreatePreUseSkillEndAck},
        { 196617,CreateChangeBulletAck},
        { 196618,CreateCancelChangeBulletAck},
        { 196628,CreateUseSkillAck},
        { 196629,CreateEndSkillAck},
        { 196638,CreateLostBuffAck},
        { 196648,CreateDropItemAck},
        { 196649,CreatePickDropItemAck},
        { 196658,CreateUseItemAck},
        { 196659,CreateGotItemAck},
        { 196668,CreateGotPartnerAck},
        { 196678,CreateEquipWeaponAck},
        { 196708,CreateStageSettlementAck},
        // acks for module BattleVerify
        { 262144,CreateNodeObjectInfosAck},
        { 262145,CreateTriggerNodeObjectAck},
        { 262146,CreateNotifyNodeObjectChangedAck},
        { 262147,CreateInteractionCompletedAck},
        { 262148,CreateNotifyNpcDoActionAck},
        };
        // acks for module None
        // acks for module Login
        public static IMessage CreateLoginAck()
        {
          var ack = new LoginAck();
          return ack;
        }
        // acks for module Lobby
        public static IMessage CreateEnterLobbyAck()
        {
          var ack = new EnterLobbyAck();
          return ack;
        }
        public static IMessage CreateCreateCharactorAck()
        {
          var ack = new CreateCharactorAck();
          return ack;
        }
        // acks for module Dungeon
        public static IMessage CreateDungeonInfoAck()
        {
          var ack = new DungeonInfoAck();
          return ack;
        }
        public static IMessage CreateEnterDungeonAck()
        {
          var ack = new EnterDungeonAck();
          return ack;
        }
        public static IMessage CreateExitDungeonAck()
        {
          var ack = new ExitDungeonAck();
          return ack;
        }
        public static IMessage CreateClientReadyAck()
        {
          var ack = new ClientReadyAck();
          return ack;
        }
        public static IMessage CreateSyncRoleInfoAck()
        {
          var ack = new SyncRoleInfoAck();
          return ack;
        }
        public static IMessage CreateSyncRefreshEntityAck()
        {
          var ack = new SyncRefreshEntityAck();
          return ack;
        }
        public static IMessage CreateSyncEntityInfoAck()
        {
          var ack = new SyncEntityInfoAck();
          return ack;
        }
        public static IMessage CreatePreUseSkillStartAck()
        {
          var ack = new PreUseSkillStartAck();
          return ack;
        }
        public static IMessage CreatePreUseSkillEndAck()
        {
          var ack = new PreUseSkillEndAck();
          return ack;
        }
        public static IMessage CreateChangeBulletAck()
        {
          var ack = new ChangeBulletAck();
          return ack;
        }
        public static IMessage CreateCancelChangeBulletAck()
        {
          var ack = new CancelChangeBulletAck();
          return ack;
        }
        public static IMessage CreateUseSkillAck()
        {
          var ack = new UseSkillAck();
          return ack;
        }
        public static IMessage CreateEndSkillAck()
        {
          var ack = new EndSkillAck();
          return ack;
        }
        public static IMessage CreateLostBuffAck()
        {
          var ack = new LostBuffAck();
          return ack;
        }
        public static IMessage CreateDropItemAck()
        {
          var ack = new DropItemAck();
          return ack;
        }
        public static IMessage CreatePickDropItemAck()
        {
          var ack = new PickDropItemAck();
          return ack;
        }
        public static IMessage CreateUseItemAck()
        {
          var ack = new UseItemAck();
          return ack;
        }
        public static IMessage CreateGotItemAck()
        {
          var ack = new GotItemAck();
          return ack;
        }
        public static IMessage CreateGotPartnerAck()
        {
          var ack = new GotPartnerAck();
          return ack;
        }
        public static IMessage CreateEquipWeaponAck()
        {
          var ack = new EquipWeaponAck();
          return ack;
        }
        public static IMessage CreateStageSettlementAck()
        {
          var ack = new StageSettlementAck();
          return ack;
        }
        // acks for module BattleVerify
        public static IMessage CreateNodeObjectInfosAck()
        {
          var ack = new NodeObjectInfosAck();
          return ack;
        }
        public static IMessage CreateTriggerNodeObjectAck()
        {
          var ack = new TriggerNodeObjectAck();
          return ack;
        }
        public static IMessage CreateNotifyNodeObjectChangedAck()
        {
          var ack = new NotifyNodeObjectChangedAck();
          return ack;
        }
        public static IMessage CreateInteractionCompletedAck()
        {
          var ack = new InteractionCompletedAck();
          return ack;
        }
        public static IMessage CreateNotifyNpcDoActionAck()
        {
          var ack = new NotifyNpcDoActionAck();
          return ack;
        }
  }// end of NetAcks
#endregion

}// end of namespace
