using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Application.Runtime
{
    public class GameInfo : MonoBehaviour, IState<GameState>
    {
        public PlayerController     playerController;

        void Start()
        {
            GameInfoManager.Instance.AddState(this);
            transform.parent = GameInfoManager.Instance.transform;
        }

        public virtual GameState Id { get { return GameState.Invalid; } }

        public virtual void OnEnter(IState<GameState> prevState)
        { }

        public virtual void OnLeave(IState<GameState> nextState)
        { }

        public virtual void OnUpdate(float deltaTime)
        { }
    }
}