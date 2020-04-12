using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Cache;

namespace Tests
{
    public class TestLRU
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestLRUSimplePasses()
        {
            // Use the Assert class to test conditions
            //LRUQueue<int, string> lru = new LRUQueue<int, string>(3);
            //lru.OnDiscard += OnDiscard;

            //lru.GetOrCreate(1, "aa");
            //lru.PrintIt();

            //lru.GetOrCreate(2, "bb");
            //lru.PrintIt();

            //lru.GetOrCreate(3, "cc");
            //lru.PrintIt();

            //lru.GetOrCreate(2, "bb");
            //lru.PrintIt();

            //lru.GetOrCreate(2, "ssf");
            //lru.PrintIt();

            //lru.GetOrCreate(4, "dd");
            //lru.PrintIt();
        }
        
        private void OnDiscard(int key, string value)
        {
            Debug.Log($"OnDiscard {key} {value}");
        }

        [Test]
        public void TestLRUK()
        {
            LRUKQueue<int, string> lru = new LRUKQueue<int, string>(3, 2);
            lru.OnDiscard += OnDiscard;

            lru.GetOrCreate(1, "aa");
            lru.PrintIt();

            lru.GetOrCreate(2, "bb");
            lru.PrintIt();

            lru.GetOrCreate(3, "cc");
            lru.PrintIt();

            lru.GetOrCreate(2, "bb");       // 再次访问，放入缓存队列
            lru.PrintIt();

            lru.GetOrCreate(4, "dd");
            lru.PrintIt();

            lru.GetOrCreate(5, "ee");       // 从历史队列移动至缓存队列
            lru.PrintIt();

            lru.GetOrCreate(3, "cc");
            lru.PrintIt();
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TestLRUWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
