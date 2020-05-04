using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.MeshParticleSystem
{
    public class FX_Scale : FX_Component, IReplay
    {
        public float            Delay;
#if UNITY_2019_1_OR_NEWER
        [Min(0)]
#endif
        public float            Duration;

        public bool             Addition;
        public Vector3          Target = Vector3.one;
        public AnimationCurve   Curve = new AnimationCurve(FX_Const.defaultKeyFrames);

        private float           m_ElapsedTime;
        private float           m_Delay;
        private Vector3         m_OriginalLocalScale;

        private void Awake()
        {
            // 记录初始localScale
            m_OriginalLocalScale = transform.localScale;
        }

        private void OnEnable()
        {
            Init();
        }

        private void Init()
        {
            transform.localScale = m_OriginalLocalScale;         // 恢复初始localScale

            m_ElapsedTime = 0;
            m_Delay = Delay;
        }

        void Update()
        {
            m_Delay -= Time.deltaTime;
            if (m_Delay > 0)
                return;

            float percent = 1;
            if (Duration > 0)
            {
                m_ElapsedTime += Time.deltaTime;
                percent = m_ElapsedTime / Duration;
            }
            float value = Curve.Evaluate(percent);

            if (Addition)
            {
                transform.localScale = Vector3.Lerp(m_OriginalLocalScale, m_OriginalLocalScale + Target, value);
            }
            else
            {
                transform.localScale = Vector3.Lerp(m_OriginalLocalScale, Target, value);
            }
        }

        public void Replay()
        {
            enabled = !enabled;
            enabled = !enabled;
        }
    }
}
