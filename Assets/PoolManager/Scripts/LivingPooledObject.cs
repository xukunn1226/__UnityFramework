using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cache
{
    public class LivingPooledObject : MonoPooledObjectBase, ILifeTime
    {
        public float            m_LifeTime;                                     // serialized field, if less than zero, mean it has unlimit life cycle, never recycle
        public float            LifeTime        { get; set; }                   // remaindered life cycle
        
        public float            m_InitSpeed     = 1;                            // serialized field, init speed
        public virtual float    Speed           { get; set; }

        protected virtual void Awake()
        {
            Reset();
        }

        protected virtual void Update()
        {
            if (LifeTime > 0)
            {
                LifeTime -= Time.deltaTime * Speed;
                if(LifeTime < 0)
                {
                    LifeTime = 0;
                    ReturnToPool();
                }
            }
        }

        public override void OnGet()
        {
            base.OnGet();

            Reset();
        }

        private void Reset()
        {
            LifeTime = m_LifeTime;
            Speed = m_InitSpeed;
        }
    }
}