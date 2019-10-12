using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class DecalExample : MonoBehaviour
{
    public Decal Prefab;

    private AdjustedPrefabObjectPool m_Pool;

    private void Start()
    {
        if(Prefab != null)
            m_Pool = PoolManager.GetOrCreatePool<AdjustedPrefabObjectPool>(Prefab);
    }
    private void Spawn()
    {
        if (m_Pool == null)
            return;

        //Decal d = (Decal)m_Pool.Get();
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
        {
            Spawn();
        }
    }
}
