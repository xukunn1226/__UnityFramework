using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using System.Diagnostics;

namespace Application.Runtime
{
    public class WorldGameInfo : GameInfo
    {
        public override GameState Id { get { return GameState.World; } }

        public override void OnEnter(IState<GameState> prevState)
        {
            base.OnEnter(prevState);
            playerController.enabled = true;
        }

        public override void OnLeave(IState<GameState> nextState)
        {
            playerController.enabled = false;
            base.OnLeave(nextState);
        }

        public override void OnUpdate(float deltaTime)
        {
            InputForDebug();
        }

        [Conditional("UNITY_EDITOR")]
        private void InputForDebug()
        {
            if(Input.GetKeyDown(KeyCode.F1))
            {
                ((WorldPlayerController)playerController).FocusToBase();
            }
        }
    }
}