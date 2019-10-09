using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
using System;

/// <summary>
/// 对象池静态加载、动态加载、释放、查找等范例
/// </summary>
public class CreatePoolExample : MonoBehaviour
{
    public MonoPooledObjectBase Prefab;

    private MonoPoolBase m_Pool;

    void Start()
    {
        m_Pool = PoolManager.GetOrCreatePool(Prefab);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if(Input.GetKeyDown(KeyCode.Backspace))
            {
                if (m_Pool != null)
                    PoolManager.RemovePool(m_Pool);
            }

            if(Input.GetKeyDown(KeyCode.F))
            {
                MonoPoolBase pool = PoolManager.GetPool(Prefab);
            }
        }
    }
}
