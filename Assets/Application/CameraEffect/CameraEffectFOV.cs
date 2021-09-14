using UnityEngine;

namespace Application.Runtime
{
    [System.Serializable]
    public class CameraEffectFOV : CameraEffectBase
    {
        [Tooltip("最小FOV缩放系数")]
        public float            m_MinScaleOfFOV         = 0.5f;

        [Tooltip("最大FOV缩放系数")]
        public float            m_MaxScaleOfFOV         = 2.0f;

        public AnimationCurve   m_DampCurve             = AnimationCurve.Linear(0, 1, 1, 0);

        public float            finalScaleOfFOV         { get; private set; }

        private float           m_CurveLength;        

        public override void OnBegin(float duration)
        {
            base.OnBegin(duration);

            finalScaleOfFOV = 1;
            m_CurveLength = m_DampCurve.keys.Length > 0 ? m_DampCurve.keys[m_DampCurve.keys.Length - 1].time : 0;
        }

        public override void OnSample(float elapsedTime)
        {
            finalScaleOfFOV = Mathf.Clamp(m_DampCurve.Evaluate(elapsedTime * m_CurveLength / m_Duration), m_MinScaleOfFOV, m_MaxScaleOfFOV);
        }

        public override void OnEnd()
        {
            base.OnEnd();

            finalScaleOfFOV = 1;
        }
    }
}