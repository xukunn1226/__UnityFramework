using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cache;

namespace Core
{
    public interface IBetterLinkedListNode<T> where T : class, IBetterLinkedListNode<T>, IPooledObject, new()
    {
        BetterLinkedList<T>         List { get; set; }
        IBetterLinkedListNode<T>    Next { get; set; }

        IBetterLinkedListNode<T>    Prev { get; set; }
    }
}