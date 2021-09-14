using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{    
    public class CameraEffectBase
    {
        [SerializeField]
        public bool         m_Active;

        protected float     m_Duration;

        public virtual void OnBegin(float duration) { m_Duration = duration; }

        public virtual void OnSample(float elapsedTime) { }

        public virtual void OnEnd() { }
    }
}