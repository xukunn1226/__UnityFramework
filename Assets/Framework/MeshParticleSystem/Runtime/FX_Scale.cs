using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
{
    [ExecuteInEditMode]
    public class FX_Scale : FX_Component
    {
        public float            Delay;
#if UNITY_2019_1_OR_NEWER
        [Min(0)]
#endif
        public float            Duration;

        public Vector3          StartSize;

        public Vector3          EndSize1;
        public Vector3          EndSize2;

        public AnimationCurve   Curve = new AnimationCurve(FX_Const.defaultKeyFrames);

        private float           m_Delay;

        private Vector3         m_EndSize;

        protected override void Init()
        {
            base.Init();

            transform.localScale = StartSize;         // 恢复初始localScale

            m_EndSize = EndSize1 + (EndSize2 - EndSize1) * Random.Range(0.001f, 1.0f);

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

            transform.localScale = Vector3.Lerp(StartSize, m_EndSize, value);
        }
    }
}