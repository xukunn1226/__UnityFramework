using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class Decal : LivingMonoPooledObject
{
    private ParticleSystem m_PS;

    void Awake()
    {
        LifeTime = 5;
    }

    private void Start()
    {
        m_PS = GetComponent<ParticleSystem>();
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
        if(m_PS != null)
        {
            m_PS.Stop();
        }

        base.OnRelease();
    }

    public override void SetSpeed(float speed)
    {
        base.SetSpeed(speed);

        if(m_PS != null)
        {
            ParticleSystem.MainModule main = m_PS.main;
            main.simulationSpeed = Speed;
        }
    }
}
