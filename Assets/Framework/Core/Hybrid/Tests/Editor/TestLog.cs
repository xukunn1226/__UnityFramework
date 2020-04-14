using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Core;

namespace Tests
{
    public class TestLog
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestLogSimplePasses()
        {
            GameDebug.Init();
            FileLogOutput m_FileOutput = new FileLogOutput();
            GameDebug.RegisterOutputDevice(m_FileOutput);



            GameDebug.LogError("{0}  {1}", "sfsd", "3223");
            GameDebug.Log("{0}  {1}", "11111111我们1", "ABCCCC");
            GameDebug.Log("{0}  {1}", "122222222221我们1", "ABCCCC");
            GameDebug.LogError("45454====888822222222");
            GameDebug.Flush();



            GameDebug.UnregisterOutputDevice(m_FileOutput);
            GameDebug.Shutdown();
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TestLogWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
