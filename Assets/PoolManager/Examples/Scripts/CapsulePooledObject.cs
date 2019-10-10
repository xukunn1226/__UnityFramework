using UnityEngine;

namespace Framework
{
    public class CapsulePooledObject : MonoPooledObjectBase
    {
        public float m_LifeTime = 5;

        private float m_StartTime;

        void Awake()
        {
            m_StartTime = Time.time;
        }

        void Update()
        {
            if(Time.time - m_StartTime > m_LifeTime)
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

            m_StartTime = Time.time;
        }
    }
}