using UnityEngine;
using CacheMech;

[RequireComponent(typeof(Rigidbody))]
public class Stuff : MonoPooledObjectBase
{
    public Rigidbody Body { get; private set; }

    MeshRenderer[] meshRenderers;

    private PrefabObjectPool m_Pool;

    public override IPool Pool
    {
        get
        {
            if(m_Pool == null)
            {
                m_Pool = PoolManager.GetOrCreatePool(this);
                m_Pool.PreAllocateAmount = 1;
                m_Pool.Init();
            }
            return m_Pool;
        }
        set
        {
            m_Pool = (PrefabObjectPool)value;
        }
    }

    public void SetMaterial(Material m)
    {
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material = m;
        }
    }

    void Awake()
    {
        Body = GetComponent<Rigidbody>();
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    void OnTriggerEnter(Collider enteredCollider)
    {
        if (enteredCollider.CompareTag("Kill Zone"))
        {
            ReturnToPool();
        }
    }
}