using UnityEngine;
using Cinemachine;

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
        private float           m_CurveLength;
        private float           m_CachedFOV;
        public float            finalFOV                { get; private set; }

        public override void OnBegin(CinemachineVirtualCamera camera, float duration)
        {
            base.OnBegin(camera, duration);

            m_CachedFOV = camera.m_Lens.FieldOfView;
            finalFOV = m_CachedFOV;
            m_CurveLength = m_DampCurve.keys.Length > 0 ? m_DampCurve.keys[m_DampCurve.keys.Length - 1].time : 0;
        }

        public override void OnSample(float elapsedTime)
        {
            finalFOV = Mathf.Clamp(m_DampCurve.Evaluate(elapsedTime * m_CurveLength / m_Duration), m_MinScaleOfFOV, m_MaxScaleOfFOV) * m_CachedFOV;
        }

        public override void OnEnd()
        {
            base.OnEnd();

            finalFOV = m_CachedFOV;
        }
    }
}