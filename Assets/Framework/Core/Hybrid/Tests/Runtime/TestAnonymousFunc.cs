using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using UnityEngine.Profiling;

namespace Tests
{
    public class TestAnonymousFunc : MonoBehaviour
    {
        Dictionary<int, int> table = new Dictionary<int, int>();
        public int count;

        void Start()
        {
            table.Add(1, 1);
            table.Add(2, 2);
            table.Add(3, 3);
            table.Add(4, 4);
            table.Add(5, 5);
            count = 0;

            _cb = onCompleteCB;
        }

        void Update()
        {
            Profiler.BeginSample("1111111111111111");
            SetMesh();
            Profiler.EndSample();            
        }

        System.Action<int> _cb;
        int m_Value;

        void SetMesh()
        {
            LoadAsync("assets/res/cube.asset", _cb);
        }

        void onCompleteCB(int i)
        {
            m_Value = i;
        }

        void LoadAsync(string assetPath, System.Action<int> onComplete)
        {
            onComplete?.Invoke(3);
        }

        void Foo(int i, float v)
        {
            
        }





        void Running()
        {
            Profiler.BeginSample("AnonymousWithoutParam");  // 未使用外部变量的匿名函数
            AnonymousWithoutVariable();
            Profiler.EndSample();
            Profiler.BeginSample("FunctionWithoutVariable"); // 未使用外部变量的成员函数
            FunctionWithoutVariable();
            Profiler.EndSample();
            Profiler.BeginSample("AnonymousParam"); // 使用外部变量的匿名函数
            AnonymousVariable();
            Profiler.EndSample();
            Profiler.BeginSample("FunctionVariable"); // 使用外部变量的成员函数
            FunctionVariable();
            Profiler.EndSample();
        }

        void AnonymousWithoutVariable()
        {
            table.Forecah((k, v) =>
            {
                int c = 0;
                c = k + v;
            });
        }        
        
        void FunctionWithoutVariable()
        {
            table.Forecah(AddWithoutVariable);
        }
        
        void AddWithoutVariable(int k, int v)
        {
            int c = 0;
            c = k + v;
        }

        void AnonymousVariable()
        {
            table.Forecah((k, v) =>
            {
                count = k + v;
            });
        }

        void FunctionVariable()
        {
            table.Forecah(AddtVariable);
        }

        void AddtVariable(int k, int v)
        {
            count = k + v;
        }
    }
}