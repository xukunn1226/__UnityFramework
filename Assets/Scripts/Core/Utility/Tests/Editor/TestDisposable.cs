using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;

namespace Tests
{
    public class DisposableBase : IDisposable
    {
        private bool m_Disposed = false;

        ~DisposableBase()
        {
            Dispose(false);         // 终结器回收不需要关心托管内存的释放
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_Disposed)
                return;

            if(disposing)
            {
                // free managed resources
            }

            // free unmanaged resources

            m_Disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class DerivedDisposable : DisposableBase
    {
        private bool m_Disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (m_Disposed)
                return;

            if(disposing)
            {
                // free managed resources
            }

            // free unmanaged resources

            m_Disposed = true;

            base.Dispose(disposing);
        }
    }

    public class TestDisposable
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestDisposableSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TestDisposableWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
