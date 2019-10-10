using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class Foo : IPooledObject
{
    static private ObjectPool<Foo> m_Pool;

    public void OnInit()
    {
        Debug.Log("Foo::OnInit");
    }

    public void OnGet()
    {
        Debug.Log("Foo::OnGet");
    }

    public void OnRelease()
    {
        Debug.Log("Foo::OnRelease");
    }

    public void ReturnToPool()
    {
        //Debug.Log("Foo::ReturnPool");
        throw new System.NotImplementedException();
    }

    public IPool Pool
    {
        get
        {
            throw new System.AccessViolationException();
        }
        set
        {
            throw new System.AccessViolationException();
        }
    }

    public static Foo Get()
    {
        if(m_Pool == null)
        {
            m_Pool = new ObjectPool<Foo>(1);
        }

        return (Foo)m_Pool.Get();
    }

    public static void Release(Foo f)
    {
        if (m_Pool == null)
            return;

        m_Pool.Return(f);
    }
}
