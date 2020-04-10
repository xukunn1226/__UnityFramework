using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class EqualTest : MonoBehaviour
{
    private DaichoKey m_K1;
    private DaichoKey m_K2;
    private object m_K3;

    private Dictionary<DaichoKey, int> m_dict = new Dictionary<DaichoKey, int>();

    // Start is called before the first frame update
    void Start()
    {
        m_K1 = new DaichoKey();
        m_K1.ID = 1;
        m_K1.SubID = 1;

        m_K2 = new DaichoKey();
        m_K2.ID = 1;
        m_K2.SubID = 1;

        m_K3 = new object();

        m_dict.Add(m_K1, 1);
        m_dict.Add(m_K2, 2);
    }

    // Update is called once per frame
    void Update()
    {
        //m_dict.ContainsKey(m_K1);

        //if(m_K1.Equals(m_K2))
        //{
        //    int ii = 0;
        //}

        //if(m_K1 == m_K3)
        //{
        //    int ii = 0;
        //}

        string a = "aasdsdsa";
        string b = "aasdsdsa";
        Debug.Log(object.ReferenceEquals(a, b));

        SDaichoKey sd = new SDaichoKey();
        sd.ID = 1;
        sd.SubID = 1;

        DaichoKey ss = sd;
        ss.Equals(sd);

        m_K1.Equals(sd);
    }
}

public class DaichoKey : IEquatable<DaichoKey>
{
    public int ID { get; set; }
    public int SubID { get; set; }

    public bool Equals(DaichoKey other)
    {
        if (ReferenceEquals(null, other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())        // 子类对象可以通过as转化为基类对象，从而造成不同类型对象可以进行判等操作
            return false;

        return this.ID == other.ID && this.SubID == other.SubID;
    }

    // 值类型也可以重载此函数，但因为参数类型不可变（object obj），会引起装箱操作影响效率，值类型比较推荐使用operator == 
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (GetType() != obj.GetType())        // 子类对象可以通过as转化为基类对象，从而造成不同类型对象可以进行判等操作
            return false;

        return Equals(obj as DaichoKey);
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();//return object's hashcode
    }
}

public class SDaichoKey : DaichoKey
{ }