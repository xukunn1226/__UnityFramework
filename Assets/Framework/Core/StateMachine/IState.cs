using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    public interface IState<T> where T : System.Enum
    {
        T Id { get; }

        void OnEnter(IState<T> prevState);

        void OnLeave(IState<T> nextState);

        void OnUpdate(float deltaTime);
    }
}