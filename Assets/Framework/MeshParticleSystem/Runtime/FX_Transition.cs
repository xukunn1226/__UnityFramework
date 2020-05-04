using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.MeshParticleSystem
{
    public class FX_Transition : FX_Component, IReplay
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
        private Vector3         m_OriginalLocalPos;

        private void Awake()
        {
            // 记录初始位置
            m_OriginalLocalPos = transform.localPosition;
        }

        void OnEnable()
        {
            Init();
        }

        private void Init()
        {
            // 恢复初始位置
            transform.localPosition = m_OriginalLocalPos;

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
                transform.localPosition = Vector3.Lerp(m_OriginalLocalPos, m_OriginalLocalPos + Target, value);
            }
            else
            {
                transform.localPosition = Vector3.Lerp(m_OriginalLocalPos, Target, value);
            }
        }

        public void Replay()
        {
            enabled = !enabled;
            enabled = !enabled;
        }
    }
}
