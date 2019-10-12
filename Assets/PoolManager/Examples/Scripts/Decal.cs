using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class Decal : MonoPooledObjectBase
{
    public float LifeTime;

    public float Speed = 1;

    private ParticleSystem m_PS;

    private void Start()
    {
        m_PS = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if(m_PS != null)
        {
            ParticleSystem.MainModule main = m_PS.main;
            main.simulationSpeed = Speed;
        }
    }
}
