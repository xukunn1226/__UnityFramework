using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;

namespace Framework.Core.Tests
{
    enum KeyEnum
    {
        K1,
        K2,
        K3,
        K4,
    }

    struct KeyEnumComparer : IEqualityComparer<KeyEnum>
    {
        public bool Equals(KeyEnum x, KeyEnum y)
        {
            return x == y;
        }

        public int GetHashCode(KeyEnum obj)
        {
            // you need to do some thinking here,
            return (int)obj;
        }
    }




    /// <summary>
    /// sizeof(KeyStruct) = 12B
    /// 测试Dictionary的Contains()和[]
    /// 1.不override GetHashCode()和实现IEquatable<T>
    ///     Contains():84B
    ///     []:84B
    /// 2.只实现实现IEquatable<T>
    ///     Contains():28B
    ///     []:28B
    /// 3.只override GetHashCode()
    ///     Contains():56B
    ///     []:56B
    /// 4.override GetHashCode()和实现IEquatable<T>
    ///     Contains():0
    ///     []:0
    /// </summary>
    struct KeyStruct : IEquatable<KeyStruct>
    {
        public int a;
        public int b;
        public int c;

        public KeyStruct(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public override int GetHashCode()
        {
            return this.a * 100 + this.b * 10 + this.c;
        }

        public bool Equals(KeyStruct obj)
        {
            return this.a == obj.a && this.b == obj.b && this.c == obj.c;
        }
    }

    public class TestGC
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestEnumGC()
        {
            // Use the Assert class to test conditions
            Dictionary<KeyEnum, KeyEnum> enumDict2 = new Dictionary<KeyEnum, KeyEnum>(new KeyEnumComparer())
            {
                {KeyEnum.K1,KeyEnum.K1},
                {KeyEnum.K2,KeyEnum.K1},
                {KeyEnum.K3,KeyEnum.K1},
                {KeyEnum.K4,KeyEnum.K1}
            };

            UnityEngine.Profiling.Profiler.BeginSample("2222");
            enumDict2.ContainsKey(KeyEnum.K1);
            UnityEngine.Profiling.Profiler.EndSample();
        }

        [Test]
        public void TestStructGC()
        {
            List<KeyStruct> m_Datas = new List<KeyStruct>();
            m_Datas.Add(new KeyStruct(1, 1, 1));
            m_Datas.Add(new KeyStruct(1, 2, 3));
            m_Datas.Add(new KeyStruct(1, 3, 4));
            m_Datas.Add(new KeyStruct(1, 4, 5));
            m_Datas.Add(new KeyStruct(1, 5, 6));


            Dictionary<KeyStruct, int> m_dicDatas = new Dictionary<KeyStruct, int>();
            m_dicDatas.Add(new KeyStruct(1, 1, 1), 1);
            m_dicDatas.Add(new KeyStruct(1, 2, 3), 2);
            m_dicDatas.Add(new KeyStruct(1, 3, 4), 3);
            m_dicDatas.Add(new KeyStruct(1, 4, 5), 4);
            m_dicDatas.Add(new KeyStruct(1, 5, 6), 5);

            UnityEngine.Profiling.Profiler.BeginSample("11111");
            m_Datas.Contains(new KeyStruct(1, 3, 4));
            m_dicDatas.ContainsKey(new KeyStruct(1, 3, 4));
            int v = m_dicDatas[new KeyStruct(1, 3, 4)];
            UnityEngine.Profiling.Profiler.EndSample();
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TestGCWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
