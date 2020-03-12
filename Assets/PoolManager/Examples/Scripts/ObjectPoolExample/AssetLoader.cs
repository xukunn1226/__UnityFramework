using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class AssetLoader : IPooledObject
{
    static private ObjectPool<AssetLoader> m_Pool;
    static private int m_kInitSize = 20;

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
        Debug.Log("Foo::ReturnPool");
        Release(this);
    }

    public IPool Pool
    {
        get
        {
            if (m_Pool == null)
            {
                m_Pool = new ObjectPool<AssetLoader>(m_kInitSize);
            }
            return m_Pool;
        }
        set
        {
            throw new System.AccessViolationException();
        }
    }

    public static AssetLoader Get()
    {
        if(m_Pool == null)
        {
            m_Pool = new ObjectPool<AssetLoader>(m_kInitSize);
        }

        return (AssetLoader)m_Pool.Get();
    }

    public static void Release(AssetLoader f)
    {
        if (m_Pool == null)
            return;

        m_Pool.Return(f);
    }
}
