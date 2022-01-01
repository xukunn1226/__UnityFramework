using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.HotFix
{
    public class GameModeManager : Singleton<GameModeManager>
    {
        static public GameMode              gameMode            { get; private set; }
        private StateMachine<GameState>     m_StateMachine      = new StateMachine<GameState>();        

        protected override void InternalInit()
        {
            m_StateMachine.AddState(new WorldGameMode());
            m_StateMachine.AddState(new DungeonGameMode());
        }

        protected override void OnDestroy()
        {}

        protected override void OnUpdate(float deltaTime)
        {
            m_StateMachine.Update(Time.deltaTime);
        }

        public void AddState(GameMode mode)
        {
            if(mode == null)
                throw new System.ArgumentNullException("mode");
            m_StateMachine.AddState(mode);
        }

        public void SwitchTo(GameState state)
        {
            m_StateMachine.SwitchTo(state);

            gameMode = (GameMode)m_StateMachine.curState;
        }

        public GameState GetCurGameState()
        {
            return gameMode?.Id ?? GameState.Invalid;
        }
    }
}