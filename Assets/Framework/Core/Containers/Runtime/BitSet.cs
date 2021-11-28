using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    public class BitSet
    {
        private const int   _ShrinkThreshold    = 256;
        private const int   BitsPerInt32        = 32;
        private const int   BytesPerInt32       = 4;
        private const int   BitsPerByte         = 8;
        private int[]       m_array;
        private int         m_length;

        private BitSet() {}

        public BitSet(int length) : this(length, false) {}

        public BitSet(int length, bool defaultValue)
        {
            if(length < 0)
                throw new ArgumentOutOfRangeException("length");

            m_array = new int[GetArrayLength(length, BitsPerInt32)];
            m_length = length;
            int fillValue = defaultValue ? unchecked(((int)0xffffffff)) : 0;
            for(int i = 0; i < m_array.Length; ++i)
            {
                m_array[i] = fillValue;
            }
        }

        public BitSet(byte[] bytes)
        {
            if(bytes == null)
                throw new ArgumentNullException("bytes");

            if(bytes.Length > int.MaxValue / BitsPerByte)
                throw new ArgumentException("bytes.Length > int.MaxValue / BitsPerByte");

            m_array = new int[GetArrayLength(bytes.Length, BytesPerInt32)];
            m_length = bytes.Length * BitsPerByte;

            int i = 0;
            int j = 0;
            while (bytes.Length - j >= 4)
            {
                m_array[i++] = (bytes[j] & 0xff) | ((bytes[j + 1] & 0xff) << 8) | ((bytes[j + 2] & 0xff) << 16) | ((bytes[j + 3] & 0xff) << 24);
                j += 4;
            }

            switch (bytes.Length - j)
            {
                case 3:
                    m_array[i] = ((bytes[j + 2] & 0xff) << 16);
                    goto case 2;
                    // fall through
                case 2:
                    m_array[i] |= ((bytes[j + 1] & 0xff) << 8);
                    goto case 1;
                    // fall through
                case 1:
                    m_array[i] |= (bytes[j] & 0xff);
            break;
            }
        }

        public BitSet(bool[] values)
        {
            if(values == null)
                throw new ArgumentNullException("values");

            m_array = new int[GetArrayLength(values.Length, BitsPerInt32)];
            m_length = values.Length;
            for(int i = 0; i < values.Length; ++i)
            {
                if(values[i])
                {
                    m_array[i/32] |= (1 << (i%32));
                }
            }
        }
        public BitSet(BitSet bits)
        {
            if(bits == null)
                throw new ArgumentNullException("bits");
            int length = GetArrayLength(bits.m_length, BitsPerInt32);
            m_array = new int[length];
            m_length = bits.m_length;
            System.Array.Copy(bits.m_array, m_array, length);
        }

        public int Count
        { 
            get
            {
                return m_length;
            }
        }

        static private int GetArrayLength(int n, int div)
        {
            return n > 0 ? ((n - 1) / div) + 1 : 0;
        }

        public bool this[int index]
        {
            get { return Get(index); }
            set { Set(index, value); }
        }

        public bool Get(int index)
        {
            if(index < 0 || index >= m_length)
                throw new ArgumentOutOfRangeException("index");
            return (m_array[index / 32] & (1 << (index % 32))) != 0;
        }

        public void Set(int index, bool value)
        {
            if(index < 0 || index >= m_length)
                throw new ArgumentOutOfRangeException("index");
            if(value)
            {
                m_array[index / 32] |= (1 << (index % 32));
            }
            else
            {
                m_array[index / 32] &= ~(1 << (index % 32));
            }
        }

        public void SetAll(bool value)
        {
            int fillValue = value ? unchecked(((int)0xffffffff)) : 0;
            int ints = GetArrayLength(m_length, BitsPerInt32);
            for (int i = 0; i < ints; i++) {
                m_array[i] = fillValue;
            } 
        }

        public BitSet And(BitSet value)
        {
            if (value==null)
                throw new ArgumentNullException("value");
            if (Length != value.Length)
                throw new ArgumentException("Arg_ArrayLengthsDiffer");
    
            int ints = GetArrayLength(m_length, BitsPerInt32);
            for (int i = 0; i < ints; i++)
            {
                m_array[i] &= value.m_array[i];
            }
            return this;
        }

        public BitSet Or(BitSet value)
        {
            if (value==null)
                throw new ArgumentNullException("value");
            if (Length != value.Length)
                throw new ArgumentException("Arg_ArrayLengthsDiffer");
    
            int ints = GetArrayLength(m_length, BitsPerInt32);
            for (int i = 0; i < ints; i++)
            {
                m_array[i] |= value.m_array[i];
            }
            return this;
        }

        public BitSet Xor(BitSet value)
        {
            if (value==null)
                throw new ArgumentNullException("value");
            if (Length != value.Length)
                throw new ArgumentException("Arg_ArrayLengthsDiffer");
    
            int ints = GetArrayLength(m_length, BitsPerInt32);
            for (int i = 0; i < ints; i++)
            {
                m_array[i] ^= value.m_array[i];
            }
            return this;
        }

        public BitSet Not()
        {
            int ints = GetArrayLength(m_length, BitsPerInt32);
            for (int i = 0; i < ints; i++)
            {
                m_array[i] = ~m_array[i];
            }
            return this;
        }

        public void CopyTo(BitSet target)
        {
            if(target == null)
                throw new System.ArgumentNullException("target");

            target.m_length = m_length;
            if(target.m_array == null || target.m_array.Length != m_array.Length)
            {
                target.m_array = new int[m_array.Length];
            }
            Array.Copy(m_array, target.m_array, m_array.Length);
        }

        public int Length
        {
            get
            {
                return m_length;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_NeedNonNegNum");
                }
 
                int newints = GetArrayLength(value, BitsPerInt32);
                if (newints > m_array.Length || newints + _ShrinkThreshold < m_array.Length)
                {
                    // grow or shrink (if wasting more than _ShrinkThreshold ints)
                    int[] newarray = new int[newints];
                    Array.Copy(m_array, newarray, newints > m_array.Length ? m_array.Length : newints);
                    m_array = newarray;
                }
                
                if (value > m_length)
                {
                    // clear high bit values in the last int
                    int last = GetArrayLength(m_length, BitsPerInt32) - 1;
                    int bits = m_length % 32;
                    if (bits > 0)
                    {
                        m_array[last] &= (1 << bits) - 1;
                    }
                    
                    // clear remaining int values
                    Array.Clear(m_array, last + 1, newints - last - 1);
                }
                
                m_length = value;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new BitSetEnumeratorSimple(this);
        }

        public IEnumerator GetFastEnumerator(int length = -1)
        {
            return new BitSetFastEnumerator(this, length);
        }

        private class BitSetEnumeratorSimple : IEnumerator
        {
            private BitSet bitset;
            private int index;
            private bool currentElement;
               
            internal BitSetEnumeratorSimple(BitSet bitarray)
            {
                this.bitset = bitarray;
                this.index = -1;
            }
            
            public virtual bool MoveNext()
            {
                if (index < (bitset.Count-1))
                {
                    index++;
                    currentElement = bitset.Get(index);
                    return true;
                }
                else
                    index = bitset.Count;
                
                return false;
            }
    
            public virtual System.Object Current
            {
                get
                {
                    return currentElement;
                }
            }
    
            public void Reset()
            {
                index = -1;
            }
        }

        private class BitSetFastEnumerator : IEnumerator
        {
            private BitSet  bitset;
            private int     bitIndex;
            private int     currentIndex;
            private int     bitLength;
            
            internal BitSetFastEnumerator(BitSet bitarray, int length)
            {
                this.bitset = bitarray;
                this.bitIndex = -1;
                this.bitLength = length <= 0 ? bitarray.m_length : System.Math.Min(length, bitarray.m_length);
            }
            
            public virtual bool MoveNext()
            {
                while (bitIndex < bitLength - 1)
                {
                    ++bitIndex;
                    int index = bitIndex / 32;
                    int offset = bitIndex % 32;
                    if(index >= bitset.m_array.Length)
                        return false;
                    if (bitset.m_array[index] == 0)
                    {
                        bitIndex = (index + 1) * 32 - 1;
                    }
                    else
                    {
                        if ((bitset.m_array[index] & (1 << offset)) != 0)
                        {
                            currentIndex = bitIndex;
                            return true;
                        }
                    }
                }
                return false;
            }
    
            public virtual System.Object Current
            {
                get
                {
                    return currentIndex;
                }
            }
    
            public void Reset()
            {
                bitIndex = -1;
            }
        }
    }
}