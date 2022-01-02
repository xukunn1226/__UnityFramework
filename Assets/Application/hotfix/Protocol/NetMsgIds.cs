
using System;
using System.Collections.Generic;
namespace Application.Logic
{

#region NetMsgIds
   public static class NetMsgIds
   {
       public static class NoneModule
       {
       }
       public static class LoginModule
       {
           public static int LoginMsgIDNone = 65536;//module 1,msg 0 
           public static int Login = 65537;//module 1,msg 1 
       }
       public static class LobbyModule
       {
           public static int EnterLobby = 131072;//module 2,msg 0 
           public static int CreateCharactor = 131073;//module 2,msg 1 
       }
       public static class DungeonModule
       {
           public static int DungeonInfo = 196608;//module 3,msg 0 
           public static int EnterDungeon = 196609;//module 3,msg 1 
           public static int ExitDungeon = 196610;//module 3,msg 2 
           public static int ClientReady = 196611;//module 3,msg 3 
           public static int SyncRoleInfo = 196612;//module 3,msg 4 
           public static int SyncRefreshEntity = 196613;//module 3,msg 5 
           public static int SyncEntityInfo = 196614;//module 3,msg 6 
           public static int PreUseSkillStart = 196615;//module 3,msg 7 
           public static int PreUseSkillEnd = 196616;//module 3,msg 8 
           public static int ChangeBullet = 196617;//module 3,msg 9 
           public static int CancelChangeBullet = 196618;//module 3,msg 10 
           public static int UseSkill = 196628;//module 3,msg 20 
           public static int EndSkill = 196629;//module 3,msg 21 
           public static int MonsterPreUseSkillStart = 196630;//module 3,msg 22 
           public static int MonsterPreUseSkillEnd = 196631;//module 3,msg 23 
           public static int MonsterUseSkill = 196632;//module 3,msg 24 
           public static int MonsterEndSkill = 196633;//module 3,msg 25 
           public static int LostBuff = 196638;//module 3,msg 30 
           public static int DropItem = 196648;//module 3,msg 40 
           public static int PickDropItem = 196649;//module 3,msg 41 
           public static int UseItem = 196658;//module 3,msg 50 
           public static int GotItem = 196659;//module 3,msg 51 
           public static int GotPartner = 196668;//module 3,msg 60 
           public static int EquipWeapon = 196678;//module 3,msg 70 
           public static int StageSettlement = 196708;//module 3,msg 100 
       }
       public static class BattleVerifyModule
       {
           public static int NodeObjectInfos = 262144;//module 4,msg 0 
           public static int TriggerNodeObject = 262145;//module 4,msg 1 
           public static int NotifyNodeObjectChanged = 262146;//module 4,msg 2 
           public static int InteractionCompleted = 262147;//module 4,msg 3 
           public static int NotifyNpcDoAction = 262148;//module 4,msg 4 
       }
  }// end of NetMsgIds
#endregion

}// end of namespace
