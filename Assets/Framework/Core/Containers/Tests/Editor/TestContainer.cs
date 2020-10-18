using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Framework.Cache;

namespace Framework.Core.Tests
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

    public class TestContainer
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

        [Test]
        public void TestPriorityQueue()
        {
            List<int> arr = new List<int>() {4, 6, 8, 5, 9, 2, 3};
            Heap<int> heap = new Heap<int>(arr);
            heap.Push(1);            
        }

        private int DescendingComparer(int left, int right)
        {
            if(left > right)
                return -1;
            else if(left < right)
                return 1;
            else
                return 0;
        }

        // [Test]
        // public void TestPriorityQueue()
        // {
        //     PriorityQueue<int> pq = new PriorityQueue<int>(10, Comparer<int>.Default);
        //     pq.Push(4);
        //     pq.Push(6);
        //     pq.Push(8);
        //     pq.Push(5);
        //     pq.Push(9);

        //     int a = pq.Top;
        //     pq.Pop();
        //     a = pq.Top;
        //     pq.Pop();
        //     a = pq.Top;
        //     pq.Pop();
        //     a = pq.Top;
        //     pq.Pop();
        //     a = pq.Top;
        // }
    }
}
