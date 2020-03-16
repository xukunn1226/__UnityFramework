using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class LivingPooledObject : MonoPooledObjectBase, ILifeTime
    {
        public float        LifeTime        { get; set; }                   // <= 0, 表示生命周期无限，不会自动回收
        private float       m_LifeTime;

        public float        Speed           { get { return m_Speed; } }
        private float       m_Speed;

        private float       m_ElapsedTime;

        public override IPool Pool
        {
            get
            {
                if (m_Pool == null)
                {
                    m_Pool = PoolManager.GetOrCreatePool<LivingPrefabObjectPool>(this);
                }
                return m_Pool;
            }
            set
            {
                m_Pool = value;
            }
        }

        protected virtual void Update()
        {
            if (m_LifeTime > 0)
            {
                m_ElapsedTime += Time.deltaTime * m_Speed;
                if (m_ElapsedTime > m_LifeTime)
                {
                    ReturnToPool();
                }
            }
        }

        public override void OnGet()
        {
            base.OnGet();

            // reset variables
            m_LifeTime = LifeTime;
            m_ElapsedTime = 0;
            SetSpeed(1);
        }

        public virtual void SetSpeed(float speed)
        {
            m_Speed = speed;
        }
    }
}