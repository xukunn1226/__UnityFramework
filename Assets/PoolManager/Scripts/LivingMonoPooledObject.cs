using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class LivingMonoPooledObject : MonoPooledObjectBase, ILifeTime
    {
        public float        LifeTime        { get; set; }

        private float       m_LifeTime;

        private float       m_ElapsedTime;

        protected float     m_Speed = 1;
        
        protected virtual void Update()
        {
            m_ElapsedTime += Time.deltaTime * m_Speed;
            if (m_ElapsedTime > m_LifeTime)
            {
                ReturnToPool();
            }
        }

        public override void OnGet()
        {
            base.OnGet();

            m_LifeTime = LifeTime;
            m_ElapsedTime = 0;
        }

        public virtual void SetSpeed(float speed)
        {
            m_Speed = speed;
        }
    }
}