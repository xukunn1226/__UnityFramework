using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Core;
using Cache;

namespace Tests
{

    public class Foo<T> : IBetterLinkedListNode<T> where T : class, IBetterLinkedListNode<T>, IPooledObject, new()
    {
        public BetterLinkedList<T> List { get; set; }

        public IBetterLinkedListNode<T> Next { get; set; }

        public IBetterLinkedListNode<T> Prev { get; set; }
    }

    public class BetterLinkedListTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void BetterLinkedListTestSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator BetterLinkedListTestWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
