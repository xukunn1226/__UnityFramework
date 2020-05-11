using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.MeshParticleSystem
{
    [ExecuteInEditMode]
    public class FX_Rotation : FX_Component
    {
        public float            Delay;
#if UNITY_2019_1_OR_NEWER
        [Min(0)]
#endif
        public float            Duration;

        public bool             Addition;
        public Vector3          Target;
        public AnimationCurve   Curve = new AnimationCurve(FX_Const.defaultKeyFrames);

        public bool             RandomStartRotation;
        public Vector2          ZStartRotation;

        public bool             RandomEndRotation;
        public Vector2          ZEndRotation;

        private float           m_Delay;
        private Vector3         m_OriginalLocalEuler;
        private Vector3         m_RandomZEndRotation;

        private void Awake()
        {
            RecordInit();
        }

        protected override void Init()
        {
            base.Init();

            // 恢复初始朝向
            m_OriginalLocalEuler.z = RandomStartRotation ? Random.Range(ZStartRotation.x, ZStartRotation.y) : m_OriginalLocalEuler.z;
            transform.localEulerAngles = m_OriginalLocalEuler;

            m_RandomZEndRotation.z = RandomEndRotation ? Random.Range(ZEndRotation.x, ZEndRotation.y) : 0;

            m_Delay = Delay;
        }

        void Update()
        {
            if (isStoped) return;

            m_Delay -= deltaTime;
            if (m_Delay > 0)
                return;

            float percent = 1;
            if(Duration > 0)
            {
                elapsedTime += deltaTime;
                percent = elapsedTime / Duration;
            }
            float value = Curve.Evaluate(percent);

            if (Addition)
            {
                transform.localEulerAngles = Vector3.Lerp(m_OriginalLocalEuler, m_OriginalLocalEuler + Target + m_RandomZEndRotation, value);
            }
            else
            {
                transform.localEulerAngles = Vector3.Lerp(m_OriginalLocalEuler, Target + m_RandomZEndRotation, value);
            }
        }

        public override void RecordInit()
        {
            // 记录初始朝向
            m_OriginalLocalEuler = transform.localEulerAngles;
            m_OriginalLocalEuler.z = RandomStartRotation ? Random.Range(ZStartRotation.x, ZStartRotation.y) : m_OriginalLocalEuler.z;
        }
    }
}
