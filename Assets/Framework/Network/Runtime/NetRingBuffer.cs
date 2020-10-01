using System;

namespace Framework.NetWork
{
    /// <summary>
    /// 非通用ringbuffer，仅适配网络传输用
    /// 在多线程中负责数据传递，因需要保持数据稳定性，故不支持动态扩容
    /// </summary>
    internal class NetRingBuffer
    {
        private const int       m_MinCapacity   = 1024;
        private byte[]          m_Buffer;

        protected byte[]        Buffer          { get { return m_Buffer; } }
        protected int           Head            { get; private set; }
        protected int           Tail            { get; private set; }
        protected int           Fence           { get; private set; }
        private int             IndexMask       { get; set; }

        internal NetRingBuffer(int capacity = 8 * 1024)
        {
            Init(capacity);
        }

        private void Init(int min)
        {
            if (m_Buffer != null && m_Buffer.Length > 0)
                throw new Exception("NetRingBuffer has already init");

            int newCapacity = Math.Min(Math.Max(m_MinCapacity, min), Int32.MaxValue);
            newCapacity = NextPowerOfTwo(newCapacity);

            m_Buffer = new byte[newCapacity];
            IndexMask = m_Buffer.Length - 1;
        }

        protected void Reset()
        {
            Head = 0;
            Tail = 0;
            Fence = 0;
        }

        protected bool IsEmpty()
        {
            return Head == Tail;
        }

        protected bool IsFull()
        {
            return ((Head + 1) & IndexMask) == Tail;
        }

        private int GetMaxCapacity()
        {
            return m_Buffer.Length - 1;
        }

        /// <summary>
        /// 获取空闲空间大小，不考虑连续性
        /// </summary>
        /// <returns></returns>
        private int GetUnusedCapacity()
        {
            return GetMaxCapacity() - GetUsedCapacity();
        }

        /// <summary>
        /// 获取使用中的空间大小，不考虑连续性
        /// </summary>
        /// <returns></returns>
        private int GetUsedCapacity()
        {
            return Head >= Tail ? Head - Tail : m_Buffer.Length - (Tail - Head);
        }

        protected int GetUsedCapacity(int head)
        {
            return head >= Tail ? head - Tail : m_Buffer.Length - (Tail - head);
        }

        private int NextPowerOfTwo(int n)
        {
            n--;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            n++;
            return n;
        }

        /// <summary>
        /// expand buffer, keep the m_Head, m_Tail, m_Fence unchanged
        /// </summary>
        /// <param name="min"></param>
        //private void EnsureCapacity(int min)
        //{
        //    if (m_Buffer == null || m_Buffer.Length < min)
        //    {
        //        int newCapacity = m_Buffer == null || m_Buffer.Length == 0 ? m_MinCapacity : m_Buffer.Length * 2;
        //        if ((uint)newCapacity > Int32.MaxValue)
        //            newCapacity = Int32.MaxValue;
        //        if (newCapacity < min)
        //            newCapacity = min;
        //        newCapacity = NextPowerOfTwo(newCapacity);

        //        // expand buffer
        //        byte[] newBuf = new byte[newCapacity];
        //        if (Head > Tail)
        //        {
        //            System.Buffer.BlockCopy(m_Buffer, Tail, newBuf, Tail, Head - Tail);
        //            //m_Tail = m_Tail;      // no change
        //            //m_Head = m_Head;      // no change
        //        }
        //        else if (Head < Tail)
        //        {
        //            int countToEnd = m_Buffer.Length - Tail;
        //            System.Buffer.BlockCopy(m_Buffer, Tail, newBuf, newBuf.Length - countToEnd, countToEnd);

        //            if (Head > 0)
        //                System.Buffer.BlockCopy(m_Buffer, 0, newBuf, 0, Head);

        //            Tail = newBuf.Length - countToEnd;
        //            //m_Head = m_Head;      // no change
        //            if (Fence > 0)
        //            {
        //                if (Fence < Tail)
        //                    throw new ArgumentException($"m_Fence{Fence} < m_Tail{Tail}");
        //                Fence = newBuf.Length - (m_Buffer.Length - Fence);
        //            }
        //        }
        //        m_Buffer = newBuf;
        //        m_IndexMask = m_Buffer.Length - 1;
        //    }
        //}

        // get continous free capacity from head to buffer end
        private int GetConsecutiveUnusedCapacityToEnd()
        {
            return Math.Min(GetUnusedCapacity(), Head >= Tail ? m_Buffer.Length - Head : 0);
        }

        // get continous free capacity from buffer start to tail
        private int GetConsecutiveUnusedCapacityFromStart()
        {
            int count = 0;
            if (Head < Tail)
            {
                count = Tail - Head;
            }
            else if (Head > Tail)
            {
                count = Tail;
            }
            else
            {
                count = m_Buffer.Length - Head;
            }

            return Math.Min(GetUnusedCapacity(), count);
        }

        /// <summary>
        /// 获取连续空闲空间大小
        /// </summary>
        /// <returns></returns>
        protected int GetConsecutiveUnusedCapacity()
        {
            int count = GetConsecutiveUnusedCapacityToEnd();
            if (count == 0)
                count = GetConsecutiveUnusedCapacityFromStart();
            return count;
        }

        private int GetConsecutiveUsedCapacity()
        {
            return Head >= Tail ? Head - Tail : m_Buffer.Length - Tail;
        }

        protected void AdvanceHead(int length)
        {
            Head = (Head + length) & IndexMask;
        }

        protected void AdvanceTail(int length)
        {
            Tail = (Tail + length) & IndexMask;
        }

        /// <summary>
        /// 获取已接收到的网络数据
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected ref readonly byte[] BeginRead(out int offset, out int length)
        {
            offset = Tail;
            length = GetUsedCapacity();
            return ref m_Buffer;
        }

        protected void EndRead(int length)
        {
            AdvanceTail(length);
        }

        /// <summary>
        /// 写入待发送的数据，主线程调用
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        protected void Write(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException("data == null");

            // 传入参数的合法性检查:可写入空间大小的检查
            if (offset + length > data.Length)
                throw new ArgumentOutOfRangeException("offset + length > data.Length");

            if (length > GetUnusedCapacity())
                throw new System.ArgumentOutOfRangeException("NetRingBuffer is FULL, can't write anymore");
            
            if (Head + length <= m_Buffer.Length)
            {
                System.Buffer.BlockCopy(data, offset, m_Buffer, Head, length);
            }
            else
            {
                int countToEnd = m_Buffer.Length - Head;
                System.Buffer.BlockCopy(data, offset, m_Buffer, Head, countToEnd);
                System.Buffer.BlockCopy(data, countToEnd, m_Buffer, 0, length - countToEnd);
            }
            AdvanceHead(length);
        }

        /// <summary>
        /// 同上
        /// </summary>
        /// <param name="data"></param>
        protected void Write(byte[] data)
        {
            Write(data, 0, data.Length);
        }

        /// <summary>
        /// 获取连续地、制定大小(length)的buff，返回给上层写入数据，主线程调用
        /// </summary>
        /// <param name="length">the length of write, expand buffer's capacity internally if necessary</param>
        /// <param name="buf">buffer to write</param>
        /// <param name="offset">the position where can be written</param>
        protected void BeginWrite(int length, out byte[] buf, out int offset)
        {
            if (length > GetConsecutiveUnusedCapacityToEnd() && length > GetConsecutiveUnusedCapacityFromStart())
                throw new ArgumentOutOfRangeException($"NetRingBuffer: no space to receive data {length}");

            int countToEnd = GetConsecutiveUnusedCapacityToEnd();
            if(countToEnd > 0 && length > countToEnd)
            { // need consecutive space, so skip the remaining buffer, start from beginning
                Fence = Head;
                Head = 0;
            }

            offset = Head;
            buf = m_Buffer;
        }

        /// <summary>
        /// 数据写入缓存完成，与FetchBufferToWrite对应，同一帧调用
        /// </summary>
        /// <param name="length"></param>
        protected void EndWrite(int length)
        {
            AdvanceHead(length);
        }

        /// <summary>
        /// 撤销fence，主线程调用
        /// </summary>
        protected void ResetFence()
        {
            Fence = 0;
        }

        /// <summary>
        /// 发送数据完成，子线程调用
        /// </summary>
        /// <param name="length"></param>
        protected void FinishBufferSending(int length)
        {
            // 数据包发送完成更新m_Tail
            AdvanceTail(length);
        }
    }
}
