using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Application.Runtime
{    
    public class CameraEffectBase
    {
        [SerializeField]
        public bool                         m_Active;
        protected float                     m_Duration;
        protected CinemachineVirtualCamera  m_Camera;

        public virtual void OnBegin(CinemachineVirtualCamera camera, float duration) { m_Camera = camera; m_Duration = duration; }

        public virtual void OnSample(float elapsedTime) { }

        public virtual void OnEnd() { }
    }
}