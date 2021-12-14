using System;

namespace Framework.NetWork
{
    abstract internal class NetStream : NetRingBuffer, IDisposable
    {
        protected bool                      m_Disposed;

        internal NetStream(int capacity = 8 * 1024)
            : base(capacity)
        {
        }

        ~NetStream()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_Disposed)
                return;

            if (disposing)
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
}
