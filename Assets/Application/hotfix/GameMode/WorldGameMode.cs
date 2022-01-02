using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using Application.Runtime;

namespace Application.Logic
{
    public class WorldGameMode : GameMode
    {
        public override GameState Id { get { return GameState.World; } }

        public override void OnEnter(IState<GameState> prevState)
        {
            StreamingLevelManager.onLevelLoadEnd += OnLevelLoadEnd;

            StreamingLevelManager.LevelContext ctx = new StreamingLevelManager.LevelContext();
            ctx.sceneName = "world";
            ctx.scenePath = "assets/res/scenes/world.unity";
            ctx.additive = false;
            ctx.bundlePath = "assets/res/scenes.ab";
            StreamingLevelManager.Instance.LoadAsync(ctx);
        }

        public override void OnLeave(IState<GameState> nextState)
        {}

        public override void OnUpdate(float deltaTime)
        {}

        private void OnLevelLoadEnd(string sceneName)
        {
            if(string.Compare(sceneName, "world") == 0)
            {
                StreamingLevelManager.onLevelLoadEnd -= OnLevelLoadEnd;
                Launcher.Instance.Disable();        // 结束Launcher流程
            }
        }
    }
}