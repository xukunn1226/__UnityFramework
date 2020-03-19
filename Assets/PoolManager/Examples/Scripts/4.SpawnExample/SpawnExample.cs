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
    public bool         UseMethod1;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(1);

        StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
    {
        yield return new WaitForSeconds(1);

        if(UseMethod1)
            SpawnStuff();
        else
            SpawnStuff2();

        StartCoroutine(Spawn());
    }

    // Method1：不创建特定Pool，使用MonoPooledObjectBase.Poo.Get()
    void SpawnStuff()
    {
        Stuff prefabAsset = Prefabs[Random.Range(0, Prefabs.Length)];
        
        Stuff inst = (Stuff)PoolManager.GetOrCreatePool(prefabAsset).Get();

        inst.transform.localPosition = Random.insideUnitSphere * 5;
        inst.transform.localRotation = Random.rotation;
        inst.Body.useGravity = true;
    }

    // Method 2: 不显式的创建Pool，而是由PooledObject脚本中默认创建，见Stuff(public override IPool Pool)，但会带来使用上的隐患，见MonoPooledObjectBase.Pool
    void SpawnStuff2()
    {
        Stuff prefabAsset = Prefabs[Random.Range(0, Prefabs.Length)];
        
        Stuff inst = (Stuff)prefabAsset.Pool.Get();

        inst.transform.localPosition = Random.insideUnitSphere * 5;
        inst.transform.localRotation = Random.rotation;
        inst.Body.useGravity = true;
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(100, 100, 150, 80), "Clear Pool"))
        {
            foreach(var prefabAsset in Prefabs)
            {
                PoolManager.RemoveMonoPool((MonoPoolBase)prefabAsset.Pool);
            }
        }
    }
}
