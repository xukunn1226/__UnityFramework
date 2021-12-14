using System;

namespace Framework.NetWork
{
    /// <summary>
    /// 非通用ringbuffer，仅适配网络传输用
    /// 在多线程中负责数据传递，因需要保持数据稳定性，故不支持动态扩容
    /// </summary>
    internal class NetRingBuffer
    {
        private const int       m_MinCapacity   = 16;
        private byte[]          m_Buffer;

        protected byte[]        Buffer          { get { return m_Buffer; } }
        protected int           Head            { get; set; }
        protected int           Tail            { get; set; }
        protected int           Fence           { get; set; }
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
            return IsEmpty(Head, Tail);
        }

        protected bool IsEmpty(int head, int tail)
        {
            return head == tail;
        }

        protected bool IsFull()
        {
            return IsFull(Head, Tail);
        }

        protected bool IsFull(int head, int tail)
        {
            return ((head + 1) & IndexMask) == tail;
        }

        private int GetMaxCapacity()
        {
            return m_Buffer.Length - 1;
        }

        /// <summary>
        /// 获取空闲空间大小，不考虑连续性
        /// </summary>
        /// <returns></returns>
        protected int GetUnusedCapacity()
        {
            return GetUnusedCapacity(Head, Tail);
        }

        private int GetUnusedCapacity(int head, int tail)
        {
            return GetMaxCapacity() - GetUsedCapacity(head, tail);
        }

        /// <summary>
        /// 获取使用中的空间大小，不考虑连续性
        /// </summary>
        /// <returns></returns>
        protected int GetUsedCapacity()
        {
            return GetUsedCapacity(Head, Tail);
        }

        protected int GetUsedCapacity(int head, int tail)
        {
            return (head - tail + m_Buffer.Length) % m_Buffer.Length;
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

        protected bool IsSpan()
        {
            return IsSpan(Head, Tail);
        }

        protected bool IsSpan(int head, int tail)
        {
            return head > 0 && head < tail;
        }

        /// <summary>
        /// 获取从Head至end（Buffer End OR Tail）的连续空闲空间大小，不跨界
        /// </summary>
        /// <returns></returns>
        protected int GetConsecutiveUnusedCapacityFromHeadToEnd()
        {
            return GetConsecutiveUnusedCapacityFromHeadToEnd(Head, Tail);
        }

        protected int GetConsecutiveUnusedCapacityFromHeadToEnd(int head, int tail)
        {
            int count = head >= tail ? m_Buffer.Length - head : tail - head - 1;
            return Math.Min(GetUnusedCapacity(head, tail), count);
        }

        /// <summary>
        /// 获取从Start至end（Buffer End OR Tail）的连续空闲空间大小，不跨界
        /// </summary>
        /// <returns></returns>
        protected int GetConsecutiveUnusedCapacityFromStartToEnd()
        {
            int count = 0;
            if(!IsSpan())
            {
                if(Head == 0 && Tail == 0)
                {
                    count = m_Buffer.Length;
                }
                else
                {
                    count = Tail;
                }
            }
            return Math.Min(GetUnusedCapacity(), count);
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
        /// reset Fence
        /// </summary>
        protected void ResetFence()
        {
            Fence = 0;
        }
    }
}
