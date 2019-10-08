using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 对象缓存池
    /// </summary>
    public abstract class MonoPoolBase : MonoBehaviour, IPool
    {
        public abstract IPooledObject Get();

        public abstract void Return(IPooledObject item);
    }
}