using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Cache.Editor.Tests
{
    public class LinkedObjectPoolTest
    {
        public class Foo : IBetterLinkedListNode<Foo>, IPooledObject
        {
            public LinkedObjectPool<Foo> List { get; set; }

            public IBetterLinkedListNode<Foo> Next { get; set; }

            public IBetterLinkedListNode<Foo> Prev { get; set; }

            public int Value;

            public void OnInit() { }

            /// <summary>
            /// 从对象池中拿出时的回调
            /// </summary>
            public void OnGet() { }

            /// <summary>
            /// 放回对象池时的回调
            /// </summary>
            public void OnRelease() { }

            /// <summary>
            /// 放回对象池
            /// </summary>
            public void ReturnToPool() { }

            public IPool Pool { get; set; }
        }

        // A Test behaves as an ordinary method
        [Test]
        public void TestLinkedObjectPool_0()
        {
            // Use the Assert class to test conditions
            try
            {
                LinkedObjectPool<Foo> pool = new LinkedObjectPool<Foo>(2);

                Foo f1 = (Foo)pool.Get();
                f1.Value = 1;

                Foo f2 = (Foo)pool.Get();
                f2.Value = 2;

                Foo f3 = (Foo)pool.Get();
                f3.Value = 3;

                Foo f4 = (Foo)pool.Get();
                f4.Value = 4;

                Foo f5 = (Foo)pool.Get();
                f5.Value = 5;

                pool.Return(f2);
                pool.Return(f4);
            }
            finally
            {
                PoolManager.RemoveObjectPool(typeof(Foo));
            }
        }

        [Test]
        public void TestLinkedObjectPool_1()
        {
            try
            {
                LinkedObjectPool<Foo> pool = new LinkedObjectPool<Foo>(2);

                foreach (var foo in pool)
                {
                    Debug.Log("-------" + foo.Value);
                }

                Foo f = pool.AddFirst();
                f.Value = 1;

                f = pool.AddLast();
                f.Value = 3;

                Foo m_Value = pool.AddFirst();
                m_Value.Value = 4;

                f = pool.AddFirst();
                f.Value = 2;

                Foo b = pool.AddBefore(m_Value);
                b.Value = 5;

                pool.Remove(m_Value);

                Assert.AreEqual(4, pool.Count, "pool.Count == 4");

                foreach (var foo in pool)
                {
                    Debug.Log(foo.Value);
                }

                LinkedObjectPool<Foo>.Enumerator e = pool.GetEnumerator();
                while (e.MoveNext())
                {
                    Debug.Log("========" + e.Current.Value);
                }
                e.Reset();
                while (e.MoveNext())
                {
                    Debug.Log("++++++++++" + e.Current.Value);
                }
                e.Dispose();

                pool.Clear();
            }
            finally
            {
                PoolManager.RemoveObjectPool(typeof(Foo));
            }
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator NewTestScriptWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
