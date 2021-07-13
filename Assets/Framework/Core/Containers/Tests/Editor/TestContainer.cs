using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using System.Diagnostics;
using Framework.Cache;
using System;

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
                    UnityEngine.Debug.Log(node.Value);
                }
                pool.Clear();

                foreach(var node in pool)
                {
                    UnityEngine.Debug.Log("---------: " + node.Value);
                }
                UnityEngine.Debug.Log("Count: " + pool.Count);

                BetterLinkedList<Boo> pool2 = new BetterLinkedList<Boo>();
                pool2.AddLast(new Boo(1.2f));
            }
            finally
            {
                PoolManager.RemoveAllObjectPools();
            }
        }

        [Test]
        public void TestHeap()
        {
            List<int> arr = new List<int>() {4, 6, 8, 5, 9, 2, 3};
            BinaryHeap<int> heap = new BinaryHeap<int>(arr);
            heap.Push(1);
            int index = heap.FindIndex(6);
            heap[index] = 1;

            // UnityEngine.Debug.Log(heap.FindIndex(0));
            // UnityEngine.Debug.Log(heap.FindIndex(1));
            UnityEngine.Debug.Log(heap.FindIndex(3));
            // UnityEngine.Debug.Log(heap.FindIndex(6));
            // UnityEngine.Debug.Log(heap.FindIndex(4));
            // UnityEngine.Debug.Log(heap.FindIndex(7));
            UnityEngine.Debug.Log(heap.FindIndex(8));
        }

        public class PriorityFoo : IPriorityItem
        {
            public int value;

            public int GetPriority()
            {
                return value;
            }
        }
        [Test]
        public void TestPriorityQueue()
        {
            PriorityQueue<PriorityFoo> pq = new PriorityQueue<PriorityFoo>(5, false);
            PriorityQueueNode<PriorityFoo> node1 = pq.Push(new PriorityFoo() {value = 4});
            PriorityQueueNode<PriorityFoo> node2 = pq.Push(new PriorityFoo() {value = 6});
            PriorityQueueNode<PriorityFoo> node3 = pq.Push(new PriorityFoo() {value = 8});
            PriorityQueueNode<PriorityFoo> node4 = pq.Push(new PriorityFoo() {value = 5});
            PriorityQueueNode<PriorityFoo> node5 = pq.Push(new PriorityFoo() {value = 9});
            PriorityQueueNode<PriorityFoo> node6 = pq.Push(new PriorityFoo() {value = 2});
            PriorityQueueNode<PriorityFoo> node7 = pq.Push(new PriorityFoo() {value = 3});
            // pq.Remove(node2);
            node4.Key.value = 10;
            pq.UpdatePriority(node4);
            UnityEngine.Debug.Log(pq.Pop().Priority);
            UnityEngine.Debug.Log(pq.Pop().Priority);
            UnityEngine.Debug.Log(pq.Pop().Priority);
            UnityEngine.Debug.Log(pq.Pop().Priority);
            UnityEngine.Debug.Log(pq.Pop().Priority);
            UnityEngine.Debug.Log(pq.Pop().Priority);
            UnityEngine.Debug.Log(pq.Pop().Priority);
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

        public class DelayedTask : IDelayed<DelayedTask>
        {
            static private DateTime m_Original = new DateTime(1970,1,1,0,0,0, DateTimeKind.Local);

            private long m_Delay;
            public long Expire { get; private set; }
            public string Msg { get; private set; }

            public DelayedTask(float seconds, string msg)
            {
                m_Delay = (long)(seconds * 1000);                
                Expire = GetCurrentDateTime() + m_Delay;
                Msg = msg;
            }

            public long GetDelay()
            {
                return Expire - GetCurrentDateTime();
            }

            private long GetCurrentDateTime()
            {
                return (long)DateTime.Now.Subtract(m_Original).TotalMilliseconds;
            }

            public int CompareTo(DelayedTask other)
            {
                if(Expire == other.Expire)
                    return 0;
                return Expire > other.Expire ? 1 : -1;
            }
        }

        [Test]
        public void TestDelayQueue()
        {
            DelayQueue<DelayedTask> queue = new DelayQueue<DelayedTask>();

            DelayedTask task1 = new DelayedTask(5.1f, "33333");
            DelayedTask task2 = new DelayedTask(3.1f, "11111");
            DelayedTask task3 = new DelayedTask(2.2f, "22222");
            queue.Push(task1);
            queue.Push(task2);
            queue.Push(task3);

            UnityEngine.Debug.Log($"start: {DateTime.Now.ToString()}");
            while(queue.Count > 0)
            {
                DelayedTask task = queue.Poll();
                if(task != null)
                {
                    UnityEngine.Debug.Log($"{task.Msg} ----- {DateTime.Now.ToString()}");
                }
            }
        }

        [Test]
        public void TestSkipList()
        {
            Stopwatch sw = new Stopwatch();

            System.Random rdm = new System.Random();
            SkipList<int> list = new SkipList<int>();
            // list.Add(3);
            // list.Add(3);
            // list.Add(6);
            // list.Add(7);
            // list.Add(9);
            // list.Add(12);
            // list.Add(17);
            // list.Add(19);
            // list.Add(21);
            // list.Add(25);
            // list.Add(26);
            // list.Add(18);
            // list.Remove(6);
            // list.Remove(3);
            // list.Remove(26);

            sw.Start();
            int cnt = 1000;
            while(cnt > 0)
            {
                --cnt;
                list.Add(rdm.Next(1, 10000));
            }
            sw.Stop();

            UnityEngine.Debug.Log($"elapsedTime: {sw.Elapsed.TotalMilliseconds}ms");

            list.PrintIt();
        }
    }
}
