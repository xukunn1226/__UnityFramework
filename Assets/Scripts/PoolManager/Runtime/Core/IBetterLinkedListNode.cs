using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cache
{
    public interface IBetterLinkedListNode<T> where T : class, IBetterLinkedListNode<T>, IPooledObject, new()
    {
        LinkedObjectPool<T>         List { get; set; }
        IBetterLinkedListNode<T>    Next { get; set; }
        IBetterLinkedListNode<T>    Prev { get; set; }
    }
}