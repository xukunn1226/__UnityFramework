using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using Application.Runtime;

namespace Application.HotFix
{
    public class DungeonGameMode : GameMode
    {
        public override GameState Id { get { return GameState.Dungeon; } }

        public override void OnEnter(IState<GameState> prevState)
        {
            StreamingLevelManager.onLevelLoadEnd += OnLevelLoadEnd;

            StreamingLevelManager.LevelContext ctx = new StreamingLevelManager.LevelContext();
            ctx.sceneName = "dungeon";
            ctx.scenePath = "assets/res/scenes/dungeon.unity";
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
            if(string.Compare(sceneName, "dungeon") == 0)
            {
                StreamingLevelManager.onLevelLoadEnd -= OnLevelLoadEnd;
                Launcher.Instance.Disable();        // 结束Launcher流程
            }
        }
    }
}