using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Application.Runtime
{
    public class GameInfoManager : SingletonMono<GameInfoManager>
    {
        static public GameInfo          gameInfo            { get; private set; }
        static public PlayerController  playerController    { get; private set; }

        private StateMachine<GameState> m_StateMachine      = new StateMachine<GameState>();

        public void AddState(GameInfo state)
        {
            if(state == null)
                throw new System.ArgumentNullException("state");
            m_StateMachine.AddState(state);
        }

        public void SwitchTo(GameState state)
        {
            m_StateMachine.SwitchTo(state);

            gameInfo = (GameInfo)m_StateMachine.curState;
            playerController = gameInfo?.playerController ?? null;
        }

        public GameState GetCurGameState()
        {
            return gameInfo?.Id ?? GameState.Invalid;
        }

        void Update()
        {
            m_StateMachine.Update(Time.deltaTime);
        }
    }

    public enum GameState
    {
        Invalid = -1,
        World,
        Dungeon,
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GameInfoManager))]
    public class GameStateManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GameState state = ((GameInfoManager)target).GetCurGameState();
            EditorGUILayout.LabelField("CurState", state.ToString());
        }
    }
#endif    
}