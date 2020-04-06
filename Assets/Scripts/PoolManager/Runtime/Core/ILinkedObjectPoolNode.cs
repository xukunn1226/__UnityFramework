using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cache
{
    public interface ILinkedObjectPoolNode<T> where T : class, ILinkedObjectPoolNode<T>, IPooledObject, new()
    {
        LinkedObjectPool<T>         List { get; set; }
        ILinkedObjectPoolNode<T>    Next { get; set; }
        ILinkedObjectPoolNode<T>    Prev { get; set; }
    }
}