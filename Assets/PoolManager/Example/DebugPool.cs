using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class DebugPool : MonoBehaviour
{
    public CubePooledObject Prefab;

    private CubePool m_Pool;

    public float SpawnDelay = 3;
    private float m_TimeSinceLastSpawn;

    void Start()
    {
        m_Pool = gameObject.GetComponent<CubePool>();
        if (m_Pool == null)
        {
            m_Pool = gameObject.AddComponent<CubePool>();
            m_Pool.PrefabAsset = Prefab;
        }

        Spawn();
    }

    void Update()
    {
        //m_TimeSinceLastSpawn += Time.deltaTime;
        //if (m_TimeSinceLastSpawn > SpawnDelay)
        //{
        //    Spawn();

        //    m_TimeSinceLastSpawn = 0;
        //}

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Spawn();
        }
    }

    private void Spawn()
    {
        CubePooledObject obj = (CubePooledObject)m_Pool.GetObject();
        if(obj != null)
            obj.transform.parent = transform;

        CubePooledObject o = (CubePooledObject)m_Pool.GetObject(Vector3.zero);
    }
}
