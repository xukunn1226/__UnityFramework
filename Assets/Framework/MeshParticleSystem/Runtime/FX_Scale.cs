using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.MeshParticleSystem
{
    [ExecuteInEditMode]
    public class FX_Scale : FX_Component
    {
        public float            Delay;
#if UNITY_2019_1_OR_NEWER
        [Min(0)]
#endif
        public float            Duration;

        public bool             Addition;
        public Vector3          Target = Vector3.one;
        public AnimationCurve   Curve = new AnimationCurve(FX_Const.defaultKeyFrames);

        private float           m_Delay;
        private Vector3         m_OriginalLocalScale;

        private void Awake()
        {
            RecordInit();
        }

        protected override void Init()
        {
            base.Init();

            transform.localScale = m_OriginalLocalScale;         // 恢复初始localScale

            m_Delay = Delay;
        }

        void Update()
        {
            if (isStoped) return;

            m_Delay -= deltaTime;
            if (m_Delay > 0)
                return;

            float percent = 1;
            if (Duration > 0)
            {
                elapsedTime += deltaTime;
                percent = elapsedTime / Duration;
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

        public override void RecordInit()
        {
            // 记录初始localScale
            m_OriginalLocalScale = transform.localScale;
        }
    }
}
