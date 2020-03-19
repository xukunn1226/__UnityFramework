using UnityEngine;
using Cache;

public class RectanglePooledObject : MonoPooledObjectBase
{
    public float m_LifeTime = 15;

    private float m_StartTime;

    void Awake()
    {
        m_StartTime = Time.time;
    }

    void Update()
    {
        if (Time.time - m_StartTime > m_LifeTime)
        {
            ReturnToPool();
        }
    }

    public override void OnGet()
    {
        base.OnGet();

        transform.localScale = new Vector3(Random.Range(0.5f, 3), Random.Range(0.5f, 3), Random.Range(0.5f, 3));
        transform.localRotation = Random.rotation;
        transform.localPosition = Random.insideUnitSphere * 5;
        transform.parent = null;

        m_StartTime = Time.time;
        m_LifeTime = Random.Range(2, 15);
        Debug.Log("OnGet");
    }

    public override void OnRelease()
    {
        base.OnRelease();
        Debug.Log("OnRelease");
    }
}