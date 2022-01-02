using Framework.Core;
using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Application.Runtime;

namespace Application.Logic
{
    public class NetModuleDungeon : NetBaseModule
    {
        public NetModuleDungeon(NetModuleManager manager) : base(manager)
        {
        }

        public override int GetModuleID()
        {
            return (int)ModuleType.ModuleDungeon;
        }

        public override void InitMsgFunc()
        {
            // EventManager.AddEventListener(SceneEvent.OnLoaded, OnLoadedSceneData);

            AddReceiver(NetMsgIds.DungeonModule.DungeonInfo, OnDungeonInfo);
            AddReceiver(NetMsgIds.DungeonModule.EnterDungeon, OnEnterDungeon);
            AddReceiver(NetMsgIds.DungeonModule.SyncRefreshEntity, OnSyncRefreshEntity);
            AddReceiver(NetMsgIds.DungeonModule.ClientReady, OnClientReady);
            AddReceiver(NetMsgIds.DungeonModule.SyncEntityInfo, OnSyncEntityInfo);

            
        }

        public void EnterDungeonReq(int dungeonIndex)
        {
            // var dungeonData = GameData.Instance.dungeonData;
            // if(dungeonIndex >= 0 && dungeonIndex < dungeonData.allDungeonInfos.Count)
            // {
            //     var req = new EnterDungeonReq();
            //     req.DungeonID = dungeonData.allDungeonInfos[dungeonIndex].DungeonID;
            //     Debug.Log($"enter dungeon {req.DungeonID} index {dungeonIndex}");
            //     NetReqs.Send(req);
            // }
            // else
            // {
            //     GameDebug.LogError($"Invalid dungeon index {dungeonIndex}");
            // }

        }


        private void OnDungeonInfo(IMessage msg, NetMsgData data)
        {
            // var ack = (DungeonInfoAck)msg;
            // GameData.Instance.dungeonData.Update(ack);

            // UIWindowManager.Instance.OpenWindow(WindowType.UISelectSencesWindow);

            //if (ack.Dungeons.Count > 0)
            //{
            //    EnterDungeonReq(0);
            //}
            //else
            //{
            //    GameDebug.LogError($"Invalid dungeon info.");
            //}

        }

        public static string TestSceneName = "Dungeon01";


        private void OnEnterDungeon(IMessage msg, NetMsgData data)
        {
            // var ack = (EnterDungeonAck)msg;
            // GameData.Instance.dungeonData.Update(ack);

            // var sceneConfig = ConfigManager.Instance.GetDungeonStageConfigByID(ack.DungeonID);

            // // add scene manager here
            // // we may change the init op in other location
            // InGameSceneManager.Instance.Init();

            // // 需要从返回的配置中加载
            // // for test fix
            // var sceneName = sceneConfig.stage_editor_id;
            // if (sceneName == "test")
            // {
            //     sceneName = "z01";
            // }
            // InGameSceneManager.Instance.EnterNextScene(sceneName);
            //InGameSceneManager.Instance.EnterNextScene("z01");
            //InGameSceneManager.Instance.EnterNextScene("Test01");
        }

        private void OnLoadedSceneData(EventArgs args)
        {
            //TriggerManager.Instance.Init();

            // 初始化随从

            // 初始化场景角色

            // 都准备好了，通知服务器场景开始
            NetReqs.Send(NetMsgIds.DungeonModule.ClientReady);

        }

        private void OnSyncRefreshEntity(IMessage msg, NetMsgData data)
        {
            // var ack = (SyncRefreshEntityAck)msg;

            // var dungeonData = GameData.Instance.dungeonData;

            // dungeonData.Update(ack);

        }

        private void OnClientReady(IMessage msg, NetMsgData data)
        {
            // 收到服务器场景开始标志
            GameDebug.Log($"On client ready");


        }

        private void OnSyncEntityInfo(IMessage msg, NetMsgData data)
        {
            // var ack = (SyncEntityInfoAck)msg;

            // var dungeonData = GameData.Instance.dungeonData;

            // dungeonData.Update(ack);
        }
    }

}
