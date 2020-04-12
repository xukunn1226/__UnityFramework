using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Core;

namespace Tests
{
    public class TestRingBuffer
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestRingBufferSimplePasses()
        {
            RingBuffer rb = new RingBuffer(16);

            byte[] w1 = System.Text.Encoding.UTF8.GetBytes("EFGABCDEFF");
            rb.Write(w1);

            byte[] w = System.Text.Encoding.UTF8.GetBytes("我c");
            rb.Write(w);




            byte[] r = new byte[w.Length];
            rb.Read(r);
            string s = System.Text.Encoding.UTF8.GetString(r);
            Debug.Log(s);

            byte[] r1 = new byte[w1.Length];
            rb.Read(r1);
            s = System.Text.Encoding.UTF8.GetString(r1);
            Debug.Log(s);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TestRingBufferWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
