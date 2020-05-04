using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Framework.MeshParticleSystem
{
    public interface IFX_Root
    {
        float lifeTime { get; }

        void Play();

        void Pause();

        void Stop();

        /// <summary>
        /// 重置特效所有状态(FX_Component, ParticleSystem, TrailRenderer)
        /// </summary>
        void Restart();
    }
}