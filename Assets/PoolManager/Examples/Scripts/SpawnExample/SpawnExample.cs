using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

/// <summary>
/// 如何使用对象池实例化对象
/// </summary>
public class SpawnExample : MonoBehaviour
{
    public Stuff[]      Prefabs;

    void Update()
    {
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Q))
        {
            //SpawnStuff();
            SpawnStuff2();
        }
    }

    void Start()
    {
        StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
    {
        yield return new WaitForSeconds(1);

        SpawnStuff();

        StartCoroutine(Spawn());
    }

    // Method1：不创建特定Pool，使用MonoPooledObjectBase.Poo.Get()
    void SpawnStuff()
    {
        Stuff prefab = Prefabs[Random.Range(0, Prefabs.Length)];

        Stuff inst = (Stuff)prefab.Pool.Get();      // 内部默认创建了PrefabObjectPool

        inst.transform.localPosition = Random.insideUnitSphere * 5;
        inst.transform.localRotation = Random.rotation;
        inst.Body.useGravity = false;
    }

    // Method2：创建指定Pool，通过Pool实例化对象
    void SpawnStuff2()
    {
        Stuff prefab = Prefabs[Random.Range(0, Prefabs.Length)];

        // 内部查找操作影响性能，建议持有Pool对象
        //PrefabObjectPool pool = PoolManager.GetOrCreatePool<PrefabObjectPool>(prefab);
        //Stuff inst = (Stuff)pool.Get();

        //inst.transform.localPosition = Random.insideUnitSphere * 5;
        //inst.transform.localRotation = Random.rotation;
        //inst.Body.useGravity = false;
    }
}
