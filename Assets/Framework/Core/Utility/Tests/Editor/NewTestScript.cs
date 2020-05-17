using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Framework.Core.Tests
{
    public class NewTestScript
    {
        IEnumerable<int> GenerateFibonacci(int n)
        {
            if (n >= 1) yield return 1;

            int a = 1, b = 0;
            for(int i = 2; i <= n; ++i)
            {
                int t = b;
                b = a;
                a += t;
                yield return a;
            }
        }

        // A Test behaves as an ordinary method
        [Test]
        public void TestFibonacci()
        {
            // Use the Assert class to test conditions
            IEnumerable<int> result = GenerateFibonacci(5);

            foreach(int i in result)
            {
                Debug.Log(i);
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
