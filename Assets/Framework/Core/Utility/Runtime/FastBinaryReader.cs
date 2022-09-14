using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using Unity.Collections.LowLevel.Unsafe;

namespace Framework.Core
{
    /// <summary>
    /// 速度更快的二进制读写，并减少使用的byte空间，参考protobuf改来
    /// 参考文档 https://stackoverflow.com/questions/2036718/fastest-way-of-reading-and-writing-binary
    ///          https://jacksondunstan.com/articles/3318
    ///          https://zhuanlan.zhihu.com/p/39478710
    ///  参考自知乎
    /// </summary>
    public class FastBinaryReader : IDisposable
    {
        private Stream      _source;
        private Encoding    _encoding;
        private byte[]      _ioBuffer;
        private int         _ioIndex;
        private int         _position;
        private int         _available;
        private bool        m_isMemoryStream;

        protected FastBinaryReader()
        { }

        public FastBinaryReader(Stream input, Encoding encoding)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            _source = input;
            _encoding = encoding;
            _ioBuffer = System.Buffers.ArrayPool<byte>.Shared.Rent(256);
            _ioIndex = 0;
            _available = 0;
            _position = 0;
            m_isMemoryStream = _source.GetType() == typeof(MemoryStream);
        }

        public FastBinaryReader(Stream input) : this(input, new UTF8Encoding(false, true))
        {
        }

        private const int Int32Msb = ((int)1) << 31;
        private int Zigzag_decoding(uint ziggedValue)
        {
            int value = (int)ziggedValue;
            return (-(value & 0x01)) ^ ((value >> 1) & ~Int32Msb);
        }

        private const long Int64Msb = ((long)1) << 63;
        private long Zigzag_decoding(ulong ziggedValue)
        {
            long value = (long)ziggedValue;
            return (-(value & 0x01L)) ^ ((value >> 1) & ~Int64Msb);
        }

        public uint ReadUInt32()
        {
            return ReadUInt32Variant(false);
        }

        public int ReadInt32()
        {
            return Zigzag_decoding(ReadUInt32Variant(true));
        }

        public ulong ReadUInt64()
        {
            return ReadUInt64Variant();
        }

        public long ReadInt64()
        {
            return Zigzag_decoding(ReadUInt64Variant());
        }

        private uint ReadUInt32Variant(bool trimNegative)
        {
            uint value;
            int read = TryReadUInt32VariantWithoutMoving(trimNegative, out value);
            if (read > 0)
            {
                _ioIndex += read;
                _available -= read;
                _position += read;
                return value;
            }
            throw EoF();
        }

        internal int TryReadUInt32VariantWithoutMoving(bool trimNegative, out uint value)
        {
            if (_available < 10) Ensure(10, false);
            if (_available == 0)
            {
                value = 0;
                return 0;
            }
            int readPos = _ioIndex;
            value = _ioBuffer[readPos++];
            if ((value & 0x80) == 0) return 1;
            value &= 0x7F;
            if (_available == 1) throw EoF();

            uint chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 7;
            if ((chunk & 0x80) == 0) return 2;
            if (_available == 2) throw EoF();

            chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 14;
            if ((chunk & 0x80) == 0) return 3;
            if (_available == 3) throw EoF();

            chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 21;
            if ((chunk & 0x80) == 0) return 4;
            if (_available == 4) throw EoF();

            chunk = _ioBuffer[readPos];
            value |= chunk << 28; // can only use 4 bits from this chunk
            if ((chunk & 0xF0) == 0) return 5;

            if (trimNegative // allow for -ve values
                && (chunk & 0xF0) == 0xF0
                && _available >= 10
                    && _ioBuffer[++readPos] == 0xFF
                    && _ioBuffer[++readPos] == 0xFF
                    && _ioBuffer[++readPos] == 0xFF
                    && _ioBuffer[++readPos] == 0xFF
                    && _ioBuffer[++readPos] == 0x01)
            {
                return 10;
            }
            throw CreateException("OverflowException");
        }

        private ulong ReadUInt64Variant()
        {
            ulong value;
            int read = TryReadUInt64VariantWithoutMoving(out value);
            if (read > 0)
            {
                _ioIndex += read;
                _available -= read;
                _position += read;
                return value;
            }
            throw EoF();
        }

        private int TryReadUInt64VariantWithoutMoving(out ulong value)
        {
            if (_available < 10) Ensure(10, false);
            if (_available == 0)
            {
                value = 0;
                return 0;
            }
            int readPos = _ioIndex;
            value = _ioBuffer[readPos++];
            if ((value & 0x80) == 0) return 1;
            value &= 0x7F;
            if (_available == 1) throw EoF();

            ulong chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 7;
            if ((chunk & 0x80) == 0) return 2;
            if (_available == 2) throw EoF();

            chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 14;
            if ((chunk & 0x80) == 0) return 3;
            if (_available == 3) throw EoF();

            chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 21;
            if ((chunk & 0x80) == 0) return 4;
            if (_available == 4) throw EoF();

            chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 28;
            if ((chunk & 0x80) == 0) return 5;
            if (_available == 5) throw EoF();

            chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 35;
            if ((chunk & 0x80) == 0) return 6;
            if (_available == 6) throw EoF();

            chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 42;
            if ((chunk & 0x80) == 0) return 7;
            if (_available == 7) throw EoF();


            chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 49;
            if ((chunk & 0x80) == 0) return 8;
            if (_available == 8) throw EoF();

            chunk = _ioBuffer[readPos++];
            value |= (chunk & 0x7F) << 56;
            if ((chunk & 0x80) == 0) return 9;
            if (_available == 9) throw EoF();

            chunk = _ioBuffer[readPos];
            value |= chunk << 63; // can only use 1 bit from this chunk

            if ((chunk & ~(ulong)0x01) != 0) throw CreateException("OverflowException");
            return 10;
        }

        internal void Ensure(int count, bool strict)
        {
            if (count > _ioBuffer.Length)
            {
                throw new Exception("too big byte block needs");
            }
            if (_ioIndex + count >= _ioBuffer.Length)
            {
                Buffer.BlockCopy(_ioBuffer, _ioIndex, _ioBuffer, 0, _available);
                _ioIndex = 0;
            }
            count -= _available;
            int writePos = _ioIndex + _available, bytesRead;
            int canRead = _ioBuffer.Length - writePos;

            while (count > 0 && canRead > 0 && (bytesRead = _source.Read(_ioBuffer, writePos, canRead)) > 0)
            {
                _available += bytesRead;
                count -= bytesRead;
                canRead -= bytesRead;
                writePos += bytesRead;
            }
            if (strict && count > 0)
            {
                throw EoF();
            }
        }

        public string ReadString()
        {
            int bytes = (int)ReadUInt32Variant(false);
            if (bytes == 0) return "";
            if (_available < bytes) Ensure(bytes, true);
            string s = _encoding.GetString(_ioBuffer, _ioIndex, bytes);
            _available -= bytes;
            _position += bytes;
            _ioIndex += bytes;
            return s;
        }

        public bool ReadBoolean()
        {
            switch (ReadUInt32())
            {
                case 0: return false;
                case 1: return true;
                default: throw CreateException("Unexpected boolean value");
            }
        }

        public unsafe float ReadFloat()
        {
            var value = ReadUInt32();
            return *(float*)(&value);
        }

        public unsafe double ReadDouble()
        {
            var value = ReadUInt64();
            return *(double*)(&value);
        }

        public ushort ReadUInt16()
        {
            return (ushort)ReadUInt32();
        }

        public short ReadInt16()
        {
            return (short)ReadInt32();
        }

        public byte ReadByte()
        {
            return (byte)ReadUInt32();
        }

        public sbyte ReadSByte()
        {
            return (sbyte)ReadInt32();
        }

        public byte[] ReadBuffer(int length, byte[] inbuff = null)
        {
            byte[] buff = inbuff ?? new byte[length];
            if (buff.Length < length)
            {
                throw new Exception($"buff长度不足!");
            }

            if (_available > length)
            {
                Buffer.BlockCopy(_ioBuffer, _ioIndex, buff, 0, length);
                _available -= length;
                _position += length;
                _ioIndex += length;
            }
            else
            {
                // reset stream to proper position
                _source.Position = _source.Position - _available;

                int bytesRead = 0, offset = 0;
                while (length > 0 && (bytesRead = _source.Read(buff, offset, length)) > 0)
                {
                    length -= bytesRead;
                    offset += bytesRead;
                }

                if (length > 0)
                    throw new Exception($"仍有{length}长度数据未读取");

                // reset buff index
                _available = 0;
                _position = 0;
                _ioIndex = 0;
            }

            return buff;
        }

        public byte[] ReadBufferWithLength(byte[] inbuff = null)
        {
            int count = ReadInt32();
            return ReadBuffer(count, inbuff);
        }

        private const int kNativeBufferSize = 512;
        private static byte[] sNativeBuffer = new byte[kNativeBufferSize];
        public unsafe void ReadBufferNative(int length, byte* inbuff)
        {
            int i = 0;
            for (; i < length - kNativeBufferSize; i += kNativeBufferSize)
            {
                ReadBuffer(kNativeBufferSize, sNativeBuffer);
                fixed (void* ptr = sNativeBuffer)
                {
                    UnsafeUtility.MemCpy(inbuff + i, (byte*)ptr, kNativeBufferSize);
                }
            }
            int left = length - i;
            if (left > 0)
            {
                ReadBuffer(left, sNativeBuffer);
                fixed (void* ptr = sNativeBuffer)
                {
                    UnsafeUtility.MemCpy(inbuff + i, (byte*)ptr, left);
                }
            }
        }

        public unsafe void ReadBufferNativeWithLength(byte* inbuff)
        {
            int count = ReadInt32();
            ReadBufferNative(count, inbuff);
        }

        public unsafe void ReadStructNative<T>(ref T value) where T : unmanaged
        {
            int objSize = UnsafeUtility.SizeOf<T>();
            ReadBuffer(objSize, sNativeBuffer);
            fixed (void* ptr = sNativeBuffer)
            {
                UnsafeUtility.MemCpy(UnsafeUtility.AddressOf(ref value), (byte*)ptr, objSize);
            }
        }

        public unsafe T[] ReadArrayNative<T>() where T : unmanaged
        {
            int count = ReadInt32();
            int objSize = UnsafeUtility.SizeOf<T>();
            int elementCount = count / objSize;
            T[] arr = new T[elementCount];
            //if (elementCount > arr.Length)
            //{
            //    throw new Exception($"缓冲区长度不匹配，缓冲区 {elementCount},传入数组长度 {arr.Length}");
            //}

            byte* ptrArr = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(arr, out ulong gcHandle);
            ReadBufferNative(count, ptrArr);
            UnsafeUtility.ReleaseGCObject(gcHandle);
            return arr;
        }

        public unsafe T[,] ReadArrayNative2D<T>(int dimCol) where T : unmanaged
        {
            int count = ReadInt32();
            int objSize = UnsafeUtility.SizeOf<T>();
            int elementCount = count / objSize;
            int rowCount = Mathf.CeilToInt((float)elementCount / dimCol);
            T[,] arr = new T[rowCount, dimCol];
            //if (elementCount > arr.Length)
            //{
            //    throw new Exception($"缓冲区长度不匹配，缓冲区 {elementCount},传入数组长度 {arr.Length}");
            //}

            byte* ptrArr = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(arr, out ulong gcHandle);
            ReadBufferNative(count, ptrArr);
            UnsafeUtility.ReleaseGCObject(gcHandle);
            return arr;
        }

        public unsafe void ReadListNative<T>(List<T> list) where T : unmanaged
        {
            int count = ReadInt32();
            T temp = new T();
            for (int i = 0; i < count; i++)
            {
                ReadStructNative<T>(ref temp);
                list.Add(temp);
            }
        }

        /// <summary>
        /// 不同于ReadBuffer,这里不从流中读出到缓冲区，而是直接指针指向流缓冲区，少了一次缓冲区拷贝
        /// !! 注意，流必须是【拥有完整缓冲区】的MemoryStream!!不能是FileStream，构造函数保证了这一点
        /// </summary>
        /// <param name="func"></param>
        public void PinBufferWithLength(Action<byte[], int> func)
        {
            int length = ReadInt32();
            PinBuffer(length, func);
        }

        public void PinBuffer(int length, Action<byte[], int> func)
        {
            if (!m_isMemoryStream)
                throw new InvalidOperationException($"该操作只能作用于memory stream");

            MemoryStream stream = _source as MemoryStream;

            if (!BitConverter.IsLittleEndian)
            {
                throw new Exception("该平台字节序是【大端】!");
            }

            if (_available > length)
            {
                func(_ioBuffer, _ioIndex);
                _available -= length;
                _position += length;
                _ioIndex += length;
            }
            else
            {
                _source.Position = _source.Position - _available;

                func(stream.GetBuffer(), (int)_source.Position);

                // reset stream to proper position
                _source.Position = _source.Position + length;

                // reset buff index
                _available = 0;
                _position = 0;
                _ioIndex = 0;
            }
        }

        public Vector2 ReadVector2()
        {
            Vector2 vec = new Vector2();
            vec.x = ReadFloat();
            vec.y = ReadFloat();
            return vec;
        }

        public Vector3 ReadVector3()
        {
            Vector3 vec = new Vector3();
            vec.x = ReadFloat();
            vec.y = ReadFloat();
            vec.z = ReadFloat();
            return vec;
        }

        public Vector4 ReadVector4()
        {
            Vector4 vec = new Vector4();
            vec.x = ReadFloat();
            vec.y = ReadFloat();
            vec.z = ReadFloat();
            vec.w = ReadFloat();
            return vec;
        }

        public Quaternion ReadQuaternion()
        {
            Quaternion quaternion = new Quaternion();
            quaternion.x = ReadFloat();
            quaternion.y = ReadFloat();
            quaternion.z = ReadFloat();
            quaternion.w = ReadFloat();
            return quaternion;
        }

        public Bounds ReadBounds()
        {
            Bounds bounds = new Bounds();
            var min = ReadVector3();
            var max = ReadVector3();
            bounds.SetMinMax(min, max);
            return bounds;
        }

        private Exception CreateException(string message)
        {
            return new Exception(message);
        }

        private Exception EoF()
        {
            return new EndOfStreamException();
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            var arrayPool = System.Buffers.ArrayPool<byte>.Shared;
            arrayPool.Return(_ioBuffer);
            _ioBuffer = null;
            _source = null;
        }
    }

    public class FastBinaryReaderExtend : FastBinaryReader
    {
        public byte[] buff;

        public void Init(byte[] buff)
        {
            this.buff = buff;
            var stream = new MemoryStream(buff);
            //base.Init(stream);
        }

        
    }
}