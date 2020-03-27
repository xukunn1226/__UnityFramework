using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Core;
using Cache;

namespace Tests
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

        [SetUp]
        public void SetupBetterLinkedList()
        {
            //Foo f = m_List.AddFirst();
            //f.Value = 1;

            //f = m_List.AddFirst();
            //f.Value = 3;

            //m_Value = m_List.AddFirst();
            //m_Value.Value = 4;

            //f = m_List.AddFirst();
            //f.Value = 2;

            //Assert.AreEqual("m_List.Count == 4", m_List.Count == 4);
        }

        [TearDown]
        public void TearDownBetterLinkedList()
        {
            //m_List.Clear();
            //PoolManager.UnregisterObjectPool(typeof(Foo));
        }

        [Test]
        public void BetterLinkedListTestSimplePasses()
        {
            try
            {
                Foo f = m_List.AddFirst();
                f.Value = 1;

                f = m_List.AddFirst();
                f.Value = 3;

                m_Value = m_List.AddFirst();
                m_Value.Value = 4;

                f = m_List.AddFirst();
                f.Value = 2;

                Assert.AreEqual(4, m_List.Count, "m_List.Count == 4");

                m_List.Clear();
            }
            finally
            {
                PoolManager.UnregisterObjectPool(typeof(Foo));
            }

            //m_List.Remove(m_Value);

            //m_List.RemoveFirst();
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
