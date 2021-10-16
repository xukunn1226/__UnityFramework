using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Application.Runtime
{
    public abstract class GameMode : IState<GameState>
    {
        public virtual GameState Id { get { return GameState.Invalid; } }

        public abstract void OnEnter(IState<GameState> prevState);

        public abstract void OnLeave(IState<GameState> nextState);

        public abstract void OnUpdate(float deltaTime);
    }
    
    public enum GameState
    {
        Invalid = -1,
        World,
        Dungeon,
    }
}