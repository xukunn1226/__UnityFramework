using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Cache;

namespace Application.Runtime.Tests
{
    public class Decal : LivingPooledObject
    {
        private ParticleSystem m_PS;

        private float m_Speed;
        public override float Speed
        {
            get
            {
                return m_Speed;
            }
            set
            {
                m_Speed = value;

                if (m_PS != null)
                {
                    ParticleSystem.MainModule main = m_PS.main;
                    main.simulationSpeed = m_Speed;
                }
            }
        }

        protected override void Awake()
        {
            m_PS = GetComponent<ParticleSystem>();

            base.Awake();
        }

        public override void OnGet()
        {
            base.OnGet();

            if (m_PS != null)
            {
                m_PS.Play();
            }
        }

        public override void OnRelease()
        {
            if (m_PS != null)
            {
                m_PS.Stop();
            }

            base.OnRelease();
        }
    }
}