
using System;
using System.Collections.Generic;
namespace Application.HotFix
{



#region NetMsgDefines

    public class NetMsgDefines
    {
        public class NetMsgDefineInfo
        {
            public int msgId;

            public int moduleId;

            public int msgInModuleId;

            public string moduleName;

            public string msgName;

            public NetMsgDefineInfo(int moduleId, int msgInModuleId, string moduleName, string msgName)
            {
                this.moduleId = moduleId;
                this.msgInModuleId = msgInModuleId;
                this.msgId = moduleId << 16 | msgInModuleId;
                this.moduleName = moduleName;
                this.msgName = msgName;

            }
        }// end of NetMsgDefineInfo
       public static List<NetMsgDefineInfo> msgs = new List<NetMsgDefineInfo>()
        {
        // const msg id for module None

        // const msg id for module Login
		new NetMsgDefineInfo(1,0,"Login","LoginMsgIDNone"),
		new NetMsgDefineInfo(1,1,"Login","Login"),

        // const msg id for module Lobby
		new NetMsgDefineInfo(2,0,"Lobby","EnterLobby"),
		new NetMsgDefineInfo(2,1,"Lobby","CreateCharactor"),

        // const msg id for module Dungeon
		new NetMsgDefineInfo(3,0,"Dungeon","DungeonInfo"),
		new NetMsgDefineInfo(3,1,"Dungeon","EnterDungeon"),
		new NetMsgDefineInfo(3,2,"Dungeon","ExitDungeon"),
		new NetMsgDefineInfo(3,3,"Dungeon","ClientReady"),
		new NetMsgDefineInfo(3,4,"Dungeon","SyncRoleInfo"),
		new NetMsgDefineInfo(3,5,"Dungeon","SyncRefreshEntity"),
		new NetMsgDefineInfo(3,6,"Dungeon","SyncEntityInfo"),
		new NetMsgDefineInfo(3,7,"Dungeon","PreUseSkillStart"),
		new NetMsgDefineInfo(3,8,"Dungeon","PreUseSkillEnd"),
		new NetMsgDefineInfo(3,9,"Dungeon","ChangeBullet"),
		new NetMsgDefineInfo(3,10,"Dungeon","CancelChangeBullet"),
		new NetMsgDefineInfo(3,20,"Dungeon","UseSkill"),
		new NetMsgDefineInfo(3,21,"Dungeon","EndSkill"),
		new NetMsgDefineInfo(3,22,"Dungeon","MonsterPreUseSkillStart"),
		new NetMsgDefineInfo(3,23,"Dungeon","MonsterPreUseSkillEnd"),
		new NetMsgDefineInfo(3,24,"Dungeon","MonsterUseSkill"),
		new NetMsgDefineInfo(3,25,"Dungeon","MonsterEndSkill"),
		new NetMsgDefineInfo(3,30,"Dungeon","LostBuff"),
		new NetMsgDefineInfo(3,40,"Dungeon","DropItem"),
		new NetMsgDefineInfo(3,41,"Dungeon","PickDropItem"),
		new NetMsgDefineInfo(3,50,"Dungeon","UseItem"),
		new NetMsgDefineInfo(3,51,"Dungeon","GotItem"),
		new NetMsgDefineInfo(3,60,"Dungeon","GotPartner"),
		new NetMsgDefineInfo(3,70,"Dungeon","EquipWeapon"),
		new NetMsgDefineInfo(3,100,"Dungeon","StageSettlement"),

        // const msg id for module BattleVerify
		new NetMsgDefineInfo(4,0,"BattleVerify","NodeObjectInfos"),
		new NetMsgDefineInfo(4,1,"BattleVerify","TriggerNodeObject"),
		new NetMsgDefineInfo(4,2,"BattleVerify","NotifyNodeObjectChanged"),
		new NetMsgDefineInfo(4,3,"BattleVerify","InteractionCompleted"),
		new NetMsgDefineInfo(4,4,"BattleVerify","NotifyNpcDoAction"),


      };
  }// end of NetMsgsDefines
#endregion

}// end of namespace
