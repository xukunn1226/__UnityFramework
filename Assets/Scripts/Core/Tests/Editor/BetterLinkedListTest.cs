using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Cache;

namespace Core.Editor.Tests
{
    public class Foo : IBetterLinkedListNode<Foo>, IPooledObject
    {
        public BetterLinkedList<Foo>        List { get; set; }

        public IBetterLinkedListNode<Foo>   Next { get; set; }

        public IBetterLinkedListNode<Foo>   Prev { get; set; }

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

    public class BetterLinkedListTest
    {
        private BetterLinkedList<Foo> m_List = new BetterLinkedList<Foo>();
        private Foo m_Value;

        private DummyEditorScript m_Test = new DummyEditorScript();

        [SetUp]
        public void SetupBetterLinkedList()
        {
        }

        [TearDown]
        public void TearDownBetterLinkedList()
        {
        }

        [Test]
        public void BetterLinkedListTest1()
        {
            try
            {
                foreach (var foo in m_List)
                {
                    Debug.Log("-------" + foo.Value);
                }

                Foo f = m_List.AddFirst();
                f.Value = 1;

                f = m_List.AddLast();
                f.Value = 3;

                m_Value = m_List.AddFirst();
                m_Value.Value = 4;

                f = m_List.AddFirst();
                f.Value = 2;

                Foo b = m_List.AddBefore(m_Value);
                b.Value = 5;

                m_List.Remove(m_Value);

                Assert.AreEqual(4, m_List.Count, "m_List.Count == 4");

                foreach (var foo in m_List)
                {
                    Debug.Log(foo.Value);
                }

                BetterLinkedList<Foo>.Enumerator e = m_List.GetEnumerator();
                while(e.MoveNext())
                {
                    Debug.Log("========" + e.Current.Value);
                }
                e.Reset();
                while(e.MoveNext())
                {
                    Debug.Log("++++++++++" + e.Current.Value);
                }
                e.Dispose();

                m_List.Clear();
            }
            finally
            {
                PoolManager.RemoveObjectPool(typeof(Foo));
            }
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
