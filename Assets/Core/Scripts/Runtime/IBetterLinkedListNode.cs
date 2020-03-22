using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cache;

public interface IBetterLinkedListNode<T> where T : class, IBetterLinkedListNode<T>, IPooledObject, new()
{
    BetterLinkedList<T>         List    { get; }

    IBetterLinkedListNode<T>    Next    { get; }

    IBetterLinkedListNode<T>    Prev    { get; }
}


//public class Foo<T> : IBetterLinkedListNode<T> where T : class, IBetterLinkedListNode<T>, IPooledObject, new()
//{

//    public BetterLinkedList<T> List { get; }

//    public IBetterLinkedListNode<T> Next { get; }

//    public IBetterLinkedListNode<T> Prev { get; }
//}