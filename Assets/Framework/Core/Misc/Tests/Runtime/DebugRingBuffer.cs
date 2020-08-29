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
        string[] results = new string[10];

        IEnumerator Start()
        {
            //Test();
            //TestDataSwitch();
            //Test1();
            //Test2();
            //Test3();
            //Test4();
            //Test5();

            //LRUQueue<string, float> lru = new LRUQueue<string, float>(1);

            // yield return new WaitForSeconds(3);
            yield break;
        }

        void Update()
        {
            // TestUnsafeFunc("122RDdfdfEE");

            UnityEngine.Profiling.Profiler.BeginSample("666666666666");
            string cccc = "12sdADCDEdf123234234212sdADCDEdf123234234212sdADCDEdf123234234212sdADCDEdf1232342342".ToString();
            string dddd = "sdfsdfsdf".ToString();
            string cd = string.Intern(cccc) + string.Intern(dddd);
            UnityEngine.Profiling.Profiler.EndSample();
            // Debug.Log($"----------{cccc.GetHashCode()}");

            // string a = "111";
            // string aIntern = string.Intern(a);
            // Debug.Log($"");

            // string b = "222";
            // string c = a + b;
            // string d = "111222";
            // // bool bb = string.IsInterned(c);
            // Debug.Log($"{object.ReferenceEquals(c, d)}      {c.GetHashCode()}       {d.GetHashCode()}");
        }
        string cc = "12sdADCDEdf";
        public void TestUnsafeFunc(string str)
        {        
            UnityEngine.Profiling.Profiler.BeginSample("11111111111111");
            string bb = str.ToLower_NoAlloc();
            Utility.ToLower_NoAlloc(str);
            Utility.ToLower_NoAlloc(str);
            Utility.ToLower_NoAlloc(str);
            Utility.ToLower_NoAlloc(str);
            UnityEngine.Profiling.Profiler.EndSample();
            // Debug.Log($"{str}   {Time.frameCount}");

            string str2 = str + "ABcdEFcddf";
            UnityEngine.Profiling.Profiler.BeginSample("222222222222222");            
            string bb2 = str.ToLower_NoAlloc();
            Utility.ToLower_NoAlloc(str2);
            Utility.ToLower_NoAlloc(str2);
            Utility.ToLower_NoAlloc(str2);
            Utility.ToLower_NoAlloc(str2);
            UnityEngine.Profiling.Profiler.EndSample();   
            // Debug.Log($"{str2}   {Time.frameCount}");

            
            string split = "232&112&&3345&4334&345345&34234&fsdf";
            UnityEngine.Profiling.Profiler.BeginSample("3333333333333333");            
            int count = split.Split_NoAlloc('&', results);
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("44444444444444");
            string s = "ADSDFE从大是大非的";
            Debug.Log($"{s.GetHashCode()}");

            s.SetLength(7);
            Debug.Log($"{s.GetHashCode()}-----------");
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("5555555555555555");
            // string cc = "12sdADCDEdf";
            // cc = Utility.Substring(cc, 2, 5);
            // Debug.Log($"{cc.GetHashCode()}");
            UnityEngine.Profiling.Profiler.EndSample();
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