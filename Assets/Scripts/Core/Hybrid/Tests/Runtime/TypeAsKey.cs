using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TypeAsKey : MonoBehaviour
{
    Dictionary<Type, int> m_dict = new Dictionary<Type, int>();

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Profiling.Profiler.BeginSample("222222222222222");
        m_dict.Add(typeof(Test1), 1);
        m_dict.Add(typeof(Test2), 2);
        UnityEngine.Profiling.Profiler.EndSample();
    }

    // Update is called once per frame
    void Update()
    {
        UnityEngine.Profiling.Profiler.BeginSample("11111111111111111");
        int v;
        m_dict.TryGetValue(typeof(Test1), out v);
        UnityEngine.Profiling.Profiler.EndSample();
    }
}

public class Test1
{
    public int ii;
}

public class Test2
{
    public string str;
}