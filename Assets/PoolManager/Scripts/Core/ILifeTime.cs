using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public interface ILifeTime
    {
        float LifeTime { get; }

        float Speed { get; }

        void SetSpeed(float speed);
    }
}