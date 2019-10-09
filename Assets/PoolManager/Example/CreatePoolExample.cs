using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

/// <summary>
/// 对象池静态加载、动态加载、释放、查找等范例
/// </summary>
public class CreatePoolExample : MonoBehaviour
{
    public MonoPooledObjectBase Prefab;
    
    private PrefabObjectPool    m_Pool;

    void Start()
    {
        // 动态加载
        m_Pool = PoolManager.GetOrCreatePool(Prefab);
        m_Pool.PreAllocateAmount = 3;
        //m_Pool.LimitAmount = ...;
        //m_Pool.LimitInstance = ...;
    }

    void Update()
    {
        // 释放
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Backspace))
        {
            //PoolManager.RemovePool(m_Pool);

            PoolManager.RemovePool<PrefabObjectPool>(Prefab);
        }

        // 查找
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
        {
            MonoPoolBase pool = PoolManager.GetPool(Prefab);
        }
    }
}
