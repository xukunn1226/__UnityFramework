using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
{
    public class FX_Rotation : FX_Component, IReplay
    {
        public float            Delay;
#if UNITY_2019_1_OR_NEWER
        [Min(0)]
#endif
        public float            Duration;

        public bool             Addition;
        public Vector3          Target;
        public AnimationCurve   Curve = new AnimationCurve(FX_Const.defaultKeyFrames);

        private float           m_ElapsedTime;
        private float           m_Delay;
        private Vector3         m_OriginalLocalEuler;

        private void Awake()
        {
            // 记录初始朝向
            m_OriginalLocalEuler = transform.localEulerAngles;
        }

        void OnEnable()
        {
            Init();
        }

        private void Init()
        {
            // 恢复初始朝向
            transform.localEulerAngles = m_OriginalLocalEuler;

            m_ElapsedTime = 0;
            m_Delay = Delay;
        }

        public void Replay()
        {
            enabled = !enabled;
        }

        void Update()
        {
            m_Delay -= Time.deltaTime;
            if (m_Delay > 0)
                return;

            float percent = 1;
            if(Duration > 0)
            {
                m_ElapsedTime += Time.deltaTime;
                percent = m_ElapsedTime / Duration;
            }
            float value = Curve.Evaluate(percent);

            if (Addition)
            {
                transform.localRotation = Quaternion.Lerp(Quaternion.Euler(m_OriginalLocalEuler), Quaternion.Euler(m_OriginalLocalEuler + Target), value);
            }
            else
            {
                transform.localRotation = Quaternion.Lerp(Quaternion.Euler(m_OriginalLocalEuler), Quaternion.Euler(Target), value);
            }
        }
    }
}
