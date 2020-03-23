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
        private BetterLinkedList<Foo> m_List;

        [UnitySetUp]
        public void SetupBetterLinkedList()
        {

        }

        [UnityTearDown]
        public void TearDownBetterLinkedList()
        {

        }

        [Test]
        public void BetterLinkedListTestSimplePasses()
        {

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
