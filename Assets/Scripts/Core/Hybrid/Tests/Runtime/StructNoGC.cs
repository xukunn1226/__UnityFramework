using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StructNoGC : MonoBehaviour
{
    private List<KeyStruct> m_Datas = new List<KeyStruct>();

    private Dictionary<KeyStruct, int> m_dicDatas = new Dictionary<KeyStruct, int>();

    private void Start()
    {
        m_Datas.Add(new KeyStruct(1, 1, 1));
        m_Datas.Add(new KeyStruct(1, 2, 3));
        m_Datas.Add(new KeyStruct(1, 3, 4));
        m_Datas.Add(new KeyStruct(1, 4, 5));
        m_Datas.Add(new KeyStruct(1, 5, 6));

        m_dicDatas.Add(new KeyStruct(1, 1, 1), 1);
        m_dicDatas.Add(new KeyStruct(1, 2, 3), 2);
        m_dicDatas.Add(new KeyStruct(1, 3, 4), 3);
        m_dicDatas.Add(new KeyStruct(1, 4, 5), 4);
        m_dicDatas.Add(new KeyStruct(1, 5, 6), 5);
    }

    // Update is called once per frame
    void Update()
    {
        UnityEngine.Profiling.Profiler.BeginSample("11111");
        m_Datas.Contains(new KeyStruct(1, 3, 4));
        m_dicDatas.ContainsKey(new KeyStruct(1, 3, 4));
        int v = m_dicDatas[new KeyStruct(1, 3, 4)];
        UnityEngine.Profiling.Profiler.EndSample();
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
