using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using Application.Runtime;
using Framework.AssetManagement.Runtime;

namespace Application.Logic
{
    public class WorldGameMode : GameMode
    {
        public override GameState Id { get { return GameState.World; } }
        private SceneOperationHandle m_Op;

        public override void OnEnter(IState<GameState> prevState)
        {
            //StreamingLevelManager.onLevelLoadEnd += OnLevelLoadEnd;

            //StreamingLevelManager.LevelContext ctx = new StreamingLevelManager.LevelContext();
            //ctx.sceneName = "world";
            //ctx.scenePath = "assets/res/scenes/world.unity";
            //ctx.additive = false;
            //ctx.fromBundle = true;
            //StreamingLevelManager.Instance.LoadAsync(ctx);

            m_Op = AssetManagerEx.LoadSceneAsync("assets/res/scenes/world.unity");
            m_Op.Completed += OnLevelLoadEnd;
        }

        public override void OnLeave(IState<GameState> nextState)
        {}

        public override void OnUpdate(float deltaTime)
        {}

        private void OnLevelLoadEnd(SceneOperationHandle op)
        {
            //if(string.Compare(sceneName, "world") == 0)
            if(op.status == EOperationStatus.Succeed)
            {
                //StreamingLevelManager.onLevelLoadEnd -= OnLevelLoadEnd;
                Launcher.Instance.Disable();        // 结束Launcher流程
            }
        }
    }
}