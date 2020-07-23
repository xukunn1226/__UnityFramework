using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Framework.Core;

namespace Framework.Core.Tests
{
    public class TestCircularQueue
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestCircularBuffer()
        {
            // Use the Assert class to test conditions
            CircularBuffer<int> buffer = new CircularBuffer<int>(-3);

            Debug.Log(buffer.GetNextIndex(-2));
            Debug.Log(buffer.GetNextIndex(-1));
            Debug.Log(buffer.GetNextIndex(2));
            Debug.Log(buffer.GetNextIndex(5));

            buffer.Capacity = 3;
        }

        [Test]
        public void TestCircularQueue2()
        {
            CircularQueue<int> cq = new CircularQueue<int>(3);

            cq.Push(1);
            cq.Push(2);
            cq.Push(3);
            cq.Pop();
            cq.Push(1);

            //Debug.Assert(cq[0] == 1);
            //Debug.Assert(cq[1] == 2);
            //Debug.Assert(!cq.IsFull());
            //Debug.Assert(cq.Count == 3);
            //Debug.Assert(cq.Capacity == 4);

            cq.Push(4);
            PrintCQ(cq);
            cq.Push(5);
            PrintCQ(cq);

            // 断言
            //Debug.Assert(cq[0] == 2);
            //Debug.Assert(cq[1] == 3);
            //Debug.Assert(cq[2] == 4);
            //Debug.Assert(cq.Peek() == 4);
            //Debug.Assert(cq.IsFull());
            //Debug.Assert(cq.Count == 3);
            //Debug.Assert(cq.Capacity == 3);
        }

        private void PrintCQ<T>(CircularQueue<T> cq)
        {
            string msg = "";
            for (int i = 0; i < cq.Count; ++i)
            {
                msg += cq[i].ToString() + "   ";
            }

            Debug.Log(msg);
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
