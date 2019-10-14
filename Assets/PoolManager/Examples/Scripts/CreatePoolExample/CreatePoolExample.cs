using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

/// <summary>
/// 对象池静态加载、动态加载、释放、查找等范例
/// </summary>
public class CreatePoolExample : MonoBehaviour
{
    public RectanglePooledObject    Prefab;
    
    private PrefabObjectPool        m_Pool;

    IEnumerator Start()
    {
        Debug.Log("Create Pool...");
        // 动态加载
        m_Pool = PoolManager.GetOrCreatePool(Prefab);
        m_Pool.PreAllocateAmount = 3;
        //m_Pool.LimitAmount = ...;
        //m_Pool.LimitInstance = ...;

        yield return new WaitForSeconds(1);

        Debug.Log("New PooledObject");
        m_Pool.Get();
        yield return new WaitForSeconds(1.5f);
        Debug.Log("New PooledObject");
        m_Pool.Get();
        yield return new WaitForSeconds(1.5f);
        Debug.Log("New PooledObject");
        MonoPoolBase pool = PoolManager.GetPool(Prefab);
        pool.Get();
        yield return new WaitForSeconds(1.5f);

        Debug.Log("Remove Pool...");
        PoolManager.RemovePool<PrefabObjectPool>(Prefab);
        PoolManager.RemovePool(m_Pool);

        yield break;
    }
}
