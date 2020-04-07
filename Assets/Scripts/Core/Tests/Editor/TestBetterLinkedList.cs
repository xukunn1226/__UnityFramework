using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Cache;

namespace Core.Runtime.Tests
{
    public class Foo
    {
        public int Value;

        public Foo(int v)
        {
            Value = v;
        }
    }

    public class Boo
    {
        public float Value;

        public Boo(float v)
        {
            Value = v;
        }
    }

    public class TestBetterLinkedList
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestBetterLinkedListSimplePasses()
        {
            // Use the Assert class to test conditions
            try
            {
                BetterLinkedList<Foo> pool = new BetterLinkedList<Foo>();

                pool.AddFirst(new Foo(1));
                pool.AddFirst(new Foo(2));
                BetterLinkedList<Foo>.BetterLinkedListNode node3 = pool.AddFirst(new Foo(3));
                pool.AddFirst(new Foo(4));
                BetterLinkedList<Foo>.BetterLinkedListNode node5 = pool.AddFirst(new Foo(5));
                pool.AddFirst(new Foo(6));

                pool.Remove(node3);
                pool.Remove(node5.Item);

                foreach(var node in pool)
                {
                    Debug.Log(node.Value);
                }
                pool.Clear();

                foreach(var node in pool)
                {
                    Debug.Log("---------: " + node.Value);
                }
                Debug.Log("Count: " + pool.Count);

                BetterLinkedList<Boo> pool2 = new BetterLinkedList<Boo>();
                pool2.AddLast(new Boo(1.2f));
            }
            finally
            {
                PoolManager.RemoveAllObjectPools();
            }
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TestBetterLinkedListWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
