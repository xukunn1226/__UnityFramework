using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public interface IPool
    {
        IPooledObject Get();

        void Return(IPooledObject item);
    }
}