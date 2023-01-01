using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using Application.Runtime;
using Cinemachine;
using Framework.AssetManagement.Runtime;

namespace Application.Logic
{
    public class DungeonGameMode : GameMode
    {
        public override GameState Id { get { return GameState.Dungeon; } }

        private MyPlayerLogic m_PlayerLogic;
        private SceneOperationHandle m_Op;

        public override void OnEnter(IState<GameState> prevState)
        {
            //StreamingLevelManager.onLevelLoadEnd += OnLevelLoadEnd;

            //StreamingLevelManager.LevelContext ctx = new StreamingLevelManager.LevelContext();
            //ctx.sceneName = "dungeon";
            //ctx.scenePath = "assets/res/scenes/dungeon.unity";
            //ctx.additive = false;
            //ctx.fromBundle = true;
            //StreamingLevelManager.Instance.LoadAsync(ctx);

            m_Op = AssetManagerEx.LoadSceneAsync("assets/res/scenes/dungeon.unity");
            m_Op.Completed += OnLevelLoadEnd;
        }

        public override void OnLeave(IState<GameState> nextState)
        {}

        public override void OnUpdate(float deltaTime)
        {
            m_PlayerLogic?.Update(deltaTime);
        }
        
        private void OnLevelLoadEnd(SceneOperationHandle op)
        {
            //if(string.Compare(sceneName, "dungeon") == 0)
            if(op.status == EOperationStatus.Succeed)
            {
                //StreamingLevelManager.onLevelLoadEnd -= OnLevelLoadEnd;
                Launcher.Instance.Disable();        // 结束Launcher流程

                m_PlayerLogic = MyPlayerLogic.Create(1);

                // bind Player to camera follow target
                CinemachineVirtualCamera vc = GameObject.FindObjectOfType<CinemachineVirtualCamera>();
                vc.m_Follow = m_PlayerLogic.playerCameraRoot;
            }
        }
    }
}