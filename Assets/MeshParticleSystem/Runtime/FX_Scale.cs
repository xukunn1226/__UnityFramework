using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
{
    public class FX_Scale : MonoBehaviour
    {
        public float            Delay;
        [Min(0)]
        public float            Duration;

        public bool             Addition;
        public Vector3          Target = Vector3.one;
        public AnimationCurve   Curve = new AnimationCurve(FX_Const.defaultKeyFrames);

        private float           m_ElapsedTime;
        private float           m_Delay;
        private Vector3         m_OriginalLocalScale;
        private bool            m_isStarted;

        private void OnEnable()
        {
            if (!m_isStarted)
                return;

            Init();
        }

        private void Start()
        {
            m_isStarted = true;

            // 记录初始localScale
            m_OriginalLocalScale = transform.localScale;

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
                transform.localScale = Vector3.Lerp(m_OriginalLocalScale, m_OriginalLocalScale + Target, value);
            else
                transform.localScale = Vector3.Lerp(m_OriginalLocalScale, Target, value);
        }
    }
}
