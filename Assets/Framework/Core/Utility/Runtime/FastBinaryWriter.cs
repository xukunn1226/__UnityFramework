using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using System;
using System.Linq;

namespace Framework.Core
{
    /// <summary>
    /// 速度更快的二进制读写，并减少使用的byte空间，参考protobuf改来
    /// 参考文档 https://stackoverflow.com/questions/2036718/fastest-way-of-reading-and-writing-binary
    /// 参考自知乎
    /// </summary>
    public sealed unsafe class FastBinaryWriter : IDisposable
    {
        private Encoding    _encoding;
        private Stream      _stream;
        private byte[]      _ioBuffer;          // temp space for writing to
        private byte*       _ioBufferPtr;
        private ulong       _bufferGCHandler;
        private int         _ioIndex;
        private int         _position;

#pragma warning disable CS0628 // 在密封类型中声明了新的保护成员
        protected FastBinaryWriter()
#pragma warning restore CS0628 // 在密封类型中声明了新的保护成员
        {
        }

        public FastBinaryWriter(Stream output, Encoding encoding)
        {
            if (output == null)
                throw new ArgumentNullException("output");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            _stream = output;
            _encoding = encoding;
            _ioBuffer = System.Buffers.ArrayPool<byte>.Shared.Rent(256);
            _ioBufferPtr = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(_ioBuffer, out _bufferGCHandler);
            _ioIndex = 0;
            _position = 0;
        }

        public FastBinaryWriter(Stream output) : this(output, new UTF8Encoding(false, true))
        {
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            Flush();
            System.Buffers.ArrayPool<byte>.Shared.Return(_ioBuffer);
            _ioBuffer = null;
            UnsafeUtility.ReleaseGCObject(_bufferGCHandler);
            _bufferGCHandler = 0;
        }

        public void Flush()
        {
            if (_ioIndex != 0)
            {
                _stream.Write(_ioBuffer, 0, _ioIndex);
                _ioIndex = 0;
            }
        }

        private void DemandSpace(int required)
        {
            if ((_ioBuffer.Length - _ioIndex) < required)
            {
                Flush(); // try emptying the buffer
                if ((_ioBuffer.Length - _ioIndex) >= required)
                {
                    return;
                }
                Debug.LogWarning($"Demand Space超过默认iobuffer长度，会引发重新分配。");

                // release old buffer
                System.Buffers.ArrayPool<byte>.Shared.Return(_ioBuffer);
                UnsafeUtility.ReleaseGCObject(_bufferGCHandler);

                _ioBuffer = System.Buffers.ArrayPool<byte>.Shared.Rent((int)(required * 1.2f));                
                _ioBufferPtr = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(_ioBuffer, out _bufferGCHandler);
                _ioIndex = 0;
                _position = 0;
            }
        }

        public void Seek(int offset, SeekOrigin origin)
        {
            Flush();
            _stream.Seek(offset, origin);
        }

        public long GetSeekPosition()
        {
            Flush();
            return _stream.Position;
        }

        public int StreamPosition
        {
            get
            {
                Flush();
                return (int)_stream.Position;
            }
        }

        // zigzag decode, ref https://gist.github.com/mfuerstenau/ba870a29e16536fdbaba
        internal uint Zigzag_encoding(int value)
        {
            return (uint)((value << 1) ^ (value >> 31));
        }

        internal ulong Zigzag_encoding(long value)
        {
            return (ulong)((value << 1) ^ (value >> 63));
        }

        public int Write(uint value)
        {
            DemandSpace(5);
            int count = 0;
            do
            {
                _ioBuffer[_ioIndex++] = (byte)((value & 0x7F) | 0x80);
                count++;
            } while ((value >>= 7) != 0);
            _ioBuffer[_ioIndex - 1] &= 0x7F;
            _position += count;
            return count;
        }
        
        public int Write(ulong value)
        {
            DemandSpace(10);
            int count = 0;
            do
            {
                _ioBuffer[_ioIndex++] = (byte)((value & 0x7F) | 0x80);
                count++;
            } while ((value >>= 7) != 0);
            _ioBuffer[_ioIndex - 1] &= 0x7F;
            _position += count;
            return count;
        }

        public int Write(int value)
        {
            return Write(Zigzag_encoding(value));
        }

        public int Write(long value)
        {
            return Write(Zigzag_encoding(value));
        }

        public int Write(ushort value)
        {
            return Write((uint)value);
        }

        public int Write(short value)
        {
            return Write((int)value);
        }

        public int Write(byte value)
        {
            return Write((uint)value);
        }

        public int Write(sbyte value)
        {
            return Write((int)value);
        }     

        public int Write(string value)
        {
            int len = value.Length;
            if (len == 0)
            {
                return Write(0);// just a header
            }
            int predicted = _encoding.GetByteCount(value);
            int count = Write((uint)predicted);
            DemandSpace(predicted);
            int actual = _encoding.GetBytes(value, 0, value.Length, _ioBuffer, _ioIndex);
            _ioIndex += actual;
            _position += actual;
            return count + actual;
        }

        public int Write(bool value)
        {
            return Write(value ? (uint)1 : (uint)0);
        }

        public unsafe int Write(float value)
        {
            return Write(*(uint*)(&value));
        }

        public unsafe int Write(double value)
        {
            return Write(*(ulong*)(&value));
        }               

        public unsafe int WriteArrayNative<T>(Array arr,int length) where T : unmanaged
        {
            int objSize = UnsafeUtility.SizeOf<T>();
            int allocateSize = length * objSize;
            int wcount = 0;
            wcount += Write(allocateSize);

            Flush();
            byte* ptrArr = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(arr, out ulong gcHandle);
            byte* endPtr = ptrArr + allocateSize;
            while (ptrArr != endPtr)
            {
                // todo: 逐byte写入，能提高写入性能吗？
                _stream.WriteByte(*ptrArr++);
            }
            UnsafeUtility.ReleaseGCObject(gcHandle);

            wcount += allocateSize;

            return wcount;
        }

        public unsafe int WriteArrayNative<T>(T[] arr) where T : unmanaged
        {
            return WriteArrayNative<T>((Array)arr, arr.Length);
        }

        public unsafe int WriteArrayNative<T>(T[] arr,int length) where T : unmanaged
        {
            return WriteArrayNative<T>((Array)arr, length);
        }

        public unsafe int WriteArrayNative<T>(T[,] arr) where T : unmanaged
        {
            return WriteArrayNative<T>((Array)arr, arr.Length);
        }

        public unsafe int WriteListNative<T>(List<T> list) where T : unmanaged
        {
            int size = 0;
            size += Write(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                var data = list[i];
                size += WriteNative<T>(ref data);
            }
            return size;
        }

        public unsafe int WriteNative<T>(ref T value) where T : unmanaged
        {
            int size = UnsafeUtility.SizeOf<T>();
            DemandSpace(size);
            UnsafeUtility.MemCpy(_ioBufferPtr + _ioIndex, UnsafeUtility.AddressOf(ref value), size);
            _ioIndex += size;
            return size;
        }

        public unsafe int WriteListNativeDirectly<T>(List<T> list) where T: unmanaged
        {
            list.Capacity = list.Count;
            var ptrList = GetUnderlyingArray(list);
            return WriteArrayNative(ptrList);

            //int objSize = UnsafeUtility.SizeOf<T>();
            //int allocateSize = list.Count * objSize;
            //var ptrList = GetUnderlyingArray(list);
            //byte[] buffForList = new byte[allocateSize];
            //fixed (void* ptrBuff = buffForList)
            //{
            //    fixed (void* ptrObj = ptrList)
            //    {
            //        UnsafeUtility.MemCpy(ptrBuff, ptrObj, allocateSize);
            //    }
            //}
            //return WriteBufferWithLength(buffForList);
        }

        public static T[] GetUnderlyingArray<T>(List<T> list) where T : unmanaged
        {
            var field = list.GetType().GetField("_items",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            return (T[])field.GetValue(list);
        }

        public int WriteBufferWithLength(byte[] buffer)
        {
            return WriteBufferWithLength(buffer, 0, buffer.Length);
        }

        public int WriteBufferWithLength(byte[] buffer,int offset,int count)
        {
            if (!BitConverter.IsLittleEndian)
            {
                throw new Exception("该平台字节序是【大端】!");
            }

            int wcount = Write(count);            
            wcount += Write(buffer, offset, count);
            return wcount;
        }

        public int Write(byte[] buffer)
        {
            return Write(buffer, 0, buffer.Length);
        }

        public int Write(byte[] buffer,int offset,int count)
        {
            Flush();
            _stream.Write(buffer, offset, count);
            return count;
        }        

        public int Write(Vector2 value)
        {
            int wcount = 0;
            wcount += Write(value.x);
            wcount += Write(value.y);
            return wcount;
        }

        public int Write(Vector3 value)
        {
            int wcount = 0;
            wcount += Write(value.x);
            wcount += Write(value.y);
            wcount += Write(value.z);
            return wcount;
        }

        public int Write(Vector4 value)
        {
            int wcount = 0;
            wcount += Write(value.x);
            wcount += Write(value.y);
            wcount += Write(value.z);
            wcount += Write(value.w);
            return wcount;
        }

        public int Write(Quaternion value)
        {
            int wcount = 0;
            wcount += Write(value.x);
            wcount += Write(value.y);
            wcount += Write(value.z);
            wcount += Write(value.w);
            return wcount;
        }

        public int Write(Bounds value)
        {
            int wcount = 0;
            wcount += Write(value.min);
            wcount += Write(value.max);
            return wcount;
        }

#if UNITY_EDITOR        
        //[UnityEditor.MenuItem("Test/Foo")]
        public static void Test()
        {
            byte v1 = 1;
            short v2 = -64;
            ushort v3 = 64;
            string v4 = "hello world";
            int v5 = -1000;
            uint v6 = 1000;
            float v7 = -100.0f;
            bool v8 = true;
            ulong v9 = 0;
            long v10 = -1000;
            byte[] vbuff = new byte[5] { 1,2,3,4,5 };
            int buffRepeatCount = 10;
            byte[] vlbuff = new byte[300];
            vlbuff[2] = 9;
            sbyte v11 = -1;
            short v12 = 0; 
            int v13 = -2000;
            uint v14 = 2000;
            string v15 = "++hello world";
            int[] arr1 = new int[] { 5, 6, 7 };
            int[,] arr2 = new int[,] { { 8, 9 },{ 10, 11 }, { 12, 13 } };
            List<int> list = new List<int>() { 0, 2, 4, -1, -3, -5 };

            var stream = new MemoryStream(1024);
            var writer = new FastBinaryWriter(stream);
            writer.Write(v1);
            writer.Write(v2);
            writer.Write(v3);
            writer.Write(v4);
            writer.Write(v5);
            writer.Write(v6);
            writer.Write(v7);
            writer.Write(v8);
            writer.Write(v9);
            writer.Write(v10);
            for (int i = 0; i < buffRepeatCount; i++)
            {
                writer.WriteBufferWithLength(vbuff);
                writer.WriteBufferWithLength(vlbuff);
            }
            writer.Write(v11);
            writer.Write(v12);
            writer.Write(v13);
            writer.Write(v14);
            writer.Write(v15);
            writer.WriteArrayNative(arr1);
            writer.WriteArrayNative(arr2);
            writer.WriteListNative(list);
            writer.Write("");

            writer.Flush();

            stream.Position = 0;
            var reader = new FastBinaryReader(stream);

            byte nv1 = reader.ReadByte();
            short nv2 = reader.ReadInt16();
            ushort nv3 = reader.ReadUInt16();
            string nv4 = reader.ReadString();
            int nv5 = reader.ReadInt32();
            uint nv6 = reader.ReadUInt32();
            float nv7 = reader.ReadFloat();
            bool nv8 = reader.ReadBoolean();
            ulong nv9 = reader.ReadUInt64();
            long nv10 = reader.ReadInt64();
            for (int i = 0; i < buffRepeatCount; i++)
            {
                byte[] nvbuff = reader.ReadBufferWithLength();
                for (int j = 0; j < vbuff.Length; j++)
                {
                    if (vbuff[j] != nvbuff[j])
                    {
                        Debug.Assert(false);
                        break;
                    }
                }
                byte[] nvlbuff = reader.ReadBufferWithLength();
                for (int j = 0; j < vbuff.Length; j++)
                {
                    if (vlbuff[j] != nvlbuff[j])
                    {
                        Debug.Assert(false);
                        break;
                    }
                }
            }
            sbyte nv11 = reader.ReadSByte();
            short nv12 = reader.ReadInt16();
            int nv13 = reader.ReadInt32();
            uint nv14 = reader.ReadUInt32();
            string nv15 = reader.ReadString();
            var narr1 = reader.ReadArrayNative<int>();
            var narr2 = reader.ReadArrayNative2D<int>(2);
            List<int> nlist = new List<int>();
            reader.ReadListNative<int>(nlist);
            var emptyStr = reader.ReadString();


            Debug.Assert(v1 == nv1); Debug.Assert(v2 == nv2); Debug.Assert(v3 == nv3); Debug.Assert(v4 == nv4);
            Debug.Assert(v5 == nv5); Debug.Assert(v6 == nv6); Debug.Assert(v7 == nv7); Debug.Assert(v8 == nv8);
            Debug.Assert(v9 == nv9); Debug.Assert(v10 == nv10);

            Debug.Assert(v11 == nv11); Debug.Assert(v12 == nv12);
            Debug.Assert(v13 == nv13); Debug.Assert(v14 == nv14); Debug.Assert(v15 == nv15);
            Debug.Assert(System.Linq.Enumerable.SequenceEqual(narr1, arr1));
            Debug.Assert(SequenceEquals<int>(narr2, arr2));

            Debug.Log($"比较完毕.");
        }

        internal static bool SequenceEquals<T>(T[,] a, T[,] b) => a.Rank == b.Rank
        && Enumerable.Range(0, a.Rank).All(d => a.GetLength(d) == b.GetLength(d))
        && a.Cast<T>().SequenceEqual(b.Cast<T>());

#endif
    }
}