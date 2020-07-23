using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Framework.Core.Tests
{
    public class DebugRingBuffer : MonoBehaviour
    {
        void Start()
        {
            //Test();
            //TestDataSwitch();
            //Test1();
            //Test2();
            //Test3();
            //Test4();
            //Test5();

            //LRUQueue<string, float> lru = new LRUQueue<string, float>(1);
        }

        //private void Test()
        //{
        //    RingBuffer rb = new RingBuffer(16);

        //    byte[] w = System.Text.Encoding.UTF8.GetBytes("a我们bc");
        //    rb.Write(w);

        //    byte[] r = new byte[w.Length];
        //    rb.Read(r);
        //    string s = System.Text.Encoding.UTF8.GetString(r);
        //    Debug.Log(s);


        //    byte[] w1 = System.Text.Encoding.UTF8.GetBytes("EFGABCDEFFF");
        //    rb.Write(w1);




        //    byte[] r1 = new byte[w1.Length];
        //    rb.Read(r1);
        //    s = System.Text.Encoding.UTF8.GetString(r1);
        //    Debug.Log(s);
        //}

        [System.Serializable]
        class Foo
        {
            int i = 1;
            int j = 2;

            public void Print()
            {
                Debug.Log($"It is a fool: {i}   {j}");
            }
        }

        // 对象序列化与反序列化
        private void TestDataSwitch()
        {
            Foo f = new Foo();
            byte[] b = ObjectToBinary(f);
            object obj = BytesToObject(b);
            Foo f1 = obj as Foo;
            f1.Print();
        }

        private byte[] ObjectToBinary(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                return ms.GetBuffer();
            }
        }

        public static object BytesToObject(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                IFormatter formatter = new BinaryFormatter();

                return formatter.Deserialize(ms);
            }
        }
    }
}