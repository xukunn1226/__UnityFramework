using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.HotFix
{
    public class StateMachine<T> where T : System.Enum
    {
        // todo: optimized, see also: https://stackoverflow.com/questions/26280788/dictionary-enum-key-performance
        private Dictionary<T, IState<T>>        m_States = new Dictionary<T, IState<T>>();

        private IState<T>                       m_CurState;

        public IState<T>                        curState => m_CurState;

        public StateMachine()
        {
            Init();
        }

        protected virtual void Init() { }

        public void AddState(IState<T> state)
        {
            if (m_States.ContainsKey(state.Id))
            {
                Debug.LogWarning($"AddState: {state.Id} has already exist!");
                return;
            }

            m_States.Add(state.Id, state);
        }

        public void Update(float deltaTime)
        {
            m_CurState?.OnUpdate(deltaTime);
        }

        public IState<T> GetState(T id)
        {
            return m_States.ContainsKey(id) ? m_States[id] : null;
        }

        public void SwitchTo(T id)
        {
            if (!m_States.ContainsKey(id))
            {
                Debug.LogWarning($"SwitchTo: state {id} not exist");
                return;
            }

            IState<T> nextState = GetState(id);
            if (m_CurState == nextState)
            {
                Debug.LogWarning($"m_CurState == GetState({id})");
                return;
            }
            if (m_CurState != null)
                m_CurState.OnLeave(nextState);

            IState<T> cache = m_CurState;
            m_CurState = nextState;

            m_CurState.OnEnter(cache);
        }
    }
}