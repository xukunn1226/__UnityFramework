using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Core;

namespace Tests
{
    public class TestCircularQueue
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestCircularBufferSimplePasses()
        {
            // Use the Assert class to test conditions
            CircularBuffer<int> buffer = new CircularBuffer<int>(-3);

            Debug.Log(buffer.GetNextIndex(-2));
            Debug.Log(buffer.GetNextIndex(-1));
            Debug.Log(buffer.GetNextIndex(2));
            Debug.Log(buffer.GetNextIndex(5));

            buffer.Capacity = 3;
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TestCircularBufferWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
