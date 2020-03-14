using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

/// <summary>
/// 对象池静态创建、动态创建、释放、查找等范例
/// </summary>
public class CreatePoolExample : MonoBehaviour
{
    public RectanglePooledObject    Prefab;
        
    public PrefabObjectPool         Pool;

    //IEnumerator Start()
    //{
    //    Debug.Log("Create Pool...");
    //    // 动态加载
    //    m_Pool = PoolManager.GetOrCreatePool(Prefab);
    //    m_Pool.PreAllocateAmount = 3;
    //    //m_Pool.LimitAmount = ...;
    //    //m_Pool.LimitInstance = ...;

    //    yield return new WaitForSeconds(1);

    //    Debug.Log("New PooledObject");
    //    m_Pool.Get();
    //    yield return new WaitForSeconds(1.5f);
    //    Debug.Log("New PooledObject");
    //    m_Pool.Get();
    //    yield return new WaitForSeconds(1.5f);
    //    Debug.Log("New PooledObject");
    //    MonoPoolBase pool = PoolManager.GetPool(Prefab);
    //    pool.Get();
    //    yield return new WaitForSeconds(1.5f);

    //    Debug.Log("Remove Pool...");
    //    PoolManager.RemovePool<PrefabObjectPool>(Prefab);
    //    PoolManager.RemovePool(m_Pool);

    //    yield break;
    //}

    private void Update()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            Pool.Get();
        }
    }

    private void OnGUI()
    {
        if(GUI.Button(new Rect(100, 100, 150, 80), "Create Pool"))
        {
            Pool = PoolManager.GetOrCreatePool<PrefabObjectPool>(Prefab);
            Pool.PreAllocateAmount = 3;
            Pool.Init();
        }

        if(GUI.Button(new Rect(100, 200, 150, 80), "Run"))
        {

        }

        if(GUI.Button(new Rect(100, 300, 150, 80), "Stop"))
        {
            Pool = PoolManager.GetOrCreatePool<PrefabObjectPool>(Prefab);

            // method 1.
            PoolManager.RemoveMonoPool(Pool);

            // method 2.
            //PoolManager.RemoveMonoPool<PrefabObjectPool>(Prefab);
        }
    }
}
