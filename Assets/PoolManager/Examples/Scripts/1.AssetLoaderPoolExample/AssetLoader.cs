﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

[System.Serializable]
public class AssetLoader : IPooledObject
{
    static private ObjectPool<AssetLoader> m_Pool;
    static private int m_kInitSize = 20;

    [SerializeField] private int m_Value;

    public int value { get { return m_Value; } set { m_Value = value; } }

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
        {
            Debug.LogError($"Pool[{f.GetType().Name}] not exist");
            return;
        }

        m_Pool.Return(f);
    }
}