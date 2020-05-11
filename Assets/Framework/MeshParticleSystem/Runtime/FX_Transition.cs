using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.MeshParticleSystem
{
    [ExecuteInEditMode]
    public class FX_Transition : FX_Component
    {
        public float            Delay;
#if UNITY_2019_1_OR_NEWER
        [Min(0)]
#endif
        public float            Duration;

        public bool             Addition;
        public Vector3          Target;
        public AnimationCurve   Curve = new AnimationCurve(FX_Const.defaultKeyFrames);
        
        private float           m_Delay;
        private Vector3         m_OriginalLocalPos;

        private void Awake()
        {
            RecordInit();
        }

        protected override void Init()
        {
            base.Init();

            transform.localPosition = m_OriginalLocalPos;

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
                transform.localPosition = Vector3.Lerp(m_OriginalLocalPos, m_OriginalLocalPos + Target, value);
            }
            else
            {
                transform.localPosition = Vector3.Lerp(m_OriginalLocalPos, Target, value);
            }
        }

        public override void RecordInit()
        {
            // 记录初始位置
            m_OriginalLocalPos = transform.localPosition;
        }
    }
}
