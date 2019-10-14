using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class DecalExample : MonoBehaviour
{
    public Decal Prefab;

    private LivingPrefabObjectPool m_Pool;

    private void Start()
    {
        if (Prefab != null)
        {
            m_Pool = PoolManager.GetOrCreatePool<LivingPrefabObjectPool>(Prefab);
            m_Pool.LimitAmount = 5;
            m_Pool.Speed = 3;
        }
    }

    private void Spawn()
    {
        if (m_Pool == null)
            return;

        Decal d = (Decal)m_Pool.Get();
        d.transform.position = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);
    }
    
    void Update()
    {
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
        {
            Spawn();
        }
    }
}
