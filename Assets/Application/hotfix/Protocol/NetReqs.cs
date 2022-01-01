
using System;
using System.Collections.Generic;
namespace Application.HotFix
{

#region NetReqs
   public static class NetReqs
   {
        public static void Send(int msgId)
        {
          NetModuleManager.Instance.SendData(msgId);
        }
        // sendings for module None
        // sendings for module Login
        public static void Send(LoginReq req)
        {
          const int msgId = 65537;
          NetModuleManager.Instance.SendData(msgId, req);
        }
        // sendings for module Lobby
        public static void Send(EnterLobbyReq req)
        {
          const int msgId = 131072;
          NetModuleManager.Instance.SendData(msgId, req);
        }
        public static void Send(CreateCharactorReq req)
        {
          const int msgId = 131073;
          NetModuleManager.Instance.SendData(msgId, req);
        }
        // sendings for module Dungeon
        public static void Send(DungeonInfoReq req)
        {
          const int msgId = 196608;
          NetModuleManager.Instance.SendData(msgId, req);
        }
        public static void Send(EnterDungeonReq req)
        {
          const int msgId = 196609;
          NetModuleManager.Instance.SendData(msgId, req);
        }
        public static void Send(ExitDungeonReq req)
        {
          const int msgId = 196610;
          NetModuleManager.Instance.SendData(msgId, req);
        }
        public static void Send(ClientReadyReq req)
        {
          const int msgId = 196611;
          NetModuleManager.Instance.SendData(msgId, req);
        }
        public static void Send(SyncRoleInfoReq req)
        {
          const int msgId = 196612;
          NetModuleManager.Instance.SendData(msgId, req);
        }
        public static void Send(PreUseSkillStartReq req)
        {
          const int msgId = 196615;
          NetModuleManager.Instance.SendData(msgId, req);
        }
        public static void Send(PreUseSkillEndReq req)
        {
          const int msgId = 196616;
          NetModuleManager.Instance.SendData(msgId, req);
        }
        public static void Send(ChangeBulletReq req)
        {
          const int msgId = 196617;
          NetModuleManager.Instance.SendData(msgId, req);
        }
        public static void Send(CancelChangeBulletReq req)
        {
          const int msgId = 196618;
          NetModuleManager.Instance.SendData(msgId, req);
        }
        public static void Send(UseSkillReq req)
        {
          const int msgId = 196628;
          NetModuleManager.Instance.SendData(msgId, req);
        }
        public static void Send(EndSkillReq req)
        {
          const int msgId = 196629;
          NetModuleManager.Instance.SendData(msgId, req);
        }
        public static void Send(PickDropItemReq req)
        {
          const int msgId = 196649;
          NetModuleManager.Instance.SendData(msgId, req);
        }
        public static void Send(UseItemReq req)
        {
          const int msgId = 196658;
          NetModuleManager.Instance.SendData(msgId, req);
        }
        public static void Send(EquipWeaponReq req)
        {
          const int msgId = 196678;
          NetModuleManager.Instance.SendData(msgId, req);
        }
        // sendings for module BattleVerify
        public static void Send(NodeObjectInfosReq req)
        {
          const int msgId = 262144;
          NetModuleManager.Instance.SendData(msgId, req);
        }
        public static void Send(TriggerNodeObjectReq req)
        {
          const int msgId = 262145;
          NetModuleManager.Instance.SendData(msgId, req);
        }
        public static void Send(InteractionCompletedReq req)
        {
          const int msgId = 262147;
          NetModuleManager.Instance.SendData(msgId, req);
        }
  }// end of NetReqs
#endregion

}// end of namespace
