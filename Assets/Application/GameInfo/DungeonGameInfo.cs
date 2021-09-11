using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Application.Runtime
{
    public class DungeonGameInfo : GameInfo
    {
        public override GameState Id { get { return GameState.Dungeon; } }

        public override void OnEnter(IState<GameState> prevState)
        { }

        public override void OnLeave(IState<GameState> nextState)
        { }

        public override void OnUpdate(float deltaTime)
        { }
    }
}