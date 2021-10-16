using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public sealed class SimpleState<T> : IState<T> where T : System.Enum
    {
        public delegate void StateEnterFunc (SimpleState<T> curState, SimpleState<T> prevState);
        public delegate void StateUpdateFunc(SimpleState<T> curState, float deltaTime);
        public delegate void StateLeaveFunc (SimpleState<T> curState, SimpleState<T> nextState);

        private readonly T          m_Id;
        private StateEnterFunc      m_EnterFunc;
        private StateUpdateFunc     m_UpdateFunc;
        private StateLeaveFunc      m_LeaveFunc;

        public SimpleState(T id, StateEnterFunc enter = null, StateUpdateFunc update = null, StateLeaveFunc leave = null)
        {
            m_Id = id;
            m_EnterFunc = enter;
            m_UpdateFunc = update;
            m_LeaveFunc = leave;
        }

        public T Id => m_Id;

        public void OnEnter(IState<T> prevState)
        {
            m_EnterFunc?.Invoke(this, (SimpleState<T>)prevState);
        }

        public void OnLeave(IState<T> nextState)
        {
            m_LeaveFunc?.Invoke(this, (SimpleState<T>)nextState);
        }

        public void OnUpdate(float deltaTime)
        {
            m_UpdateFunc?.Invoke(this, deltaTime);
        }
    }
}