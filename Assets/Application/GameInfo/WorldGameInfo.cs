using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Application.Runtime
{
    public class WorldGameInfo : GameInfo
    {
        public override GameState Id { get { return GameState.World; } }

        public override void OnEnter(IState<GameState> prevState)
        {
            base.OnEnter(prevState);
        }

        public override void OnLeave(IState<GameState> nextState)
        {
            base.OnLeave(nextState);
        }

        public override void OnUpdate(float deltaTime)
        { }
    }
}