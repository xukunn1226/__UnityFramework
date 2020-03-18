using UnityEngine;
using Framework;

[RequireComponent(typeof(Rigidbody))]
public class Stuff : MonoPooledObjectBase
{
    public Rigidbody Body { get; private set; }

    MeshRenderer[] meshRenderers;

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