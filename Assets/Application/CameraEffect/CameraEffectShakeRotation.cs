using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Application.Runtime
{
    [System.Serializable]
    public class CameraEffectShakeRotation : CameraEffectBase
    {
        public Vector3          m_Strength;
        public AnimationCurve   m_XCurve                = AnimationCurve.Linear(0, 1, 1, 0);
        public AnimationCurve   m_YCurve                = AnimationCurve.Linear(0, 1, 1, 0);
        public AnimationCurve   m_ZCurve                = AnimationCurve.Linear(0, 1, 1, 0);
        private float           m_XCurveLength;
        private float           m_YCurveLength;
        private float           m_ZCurveLength;
        private Quaternion      m_CachedLocalRotation;
        public Quaternion       finalLocalRotation      { get; private set; }

        public override void OnBegin(CinemachineVirtualCamera camera, float duration)
        {
            base.OnBegin(camera, duration);

            m_CachedLocalRotation = camera.transform.localRotation;
            finalLocalRotation = m_CachedLocalRotation;
            m_XCurveLength = m_XCurve.keys.Length > 0 ? m_XCurve.keys[m_XCurve.keys.Length - 1].time : 0;
            m_YCurveLength = m_YCurve.keys.Length > 0 ? m_YCurve.keys[m_YCurve.keys.Length - 1].time : 0;
            m_ZCurveLength = m_ZCurve.keys.Length > 0 ? m_ZCurve.keys[m_ZCurve.keys.Length - 1].time : 0;
        }

        public override void OnSample(float elapsedTime)
        {
            Vector3 randomRotation = Vector3.zero;
                        
            randomRotation.x = m_Strength.x * m_XCurve.Evaluate(elapsedTime * m_XCurveLength / m_Duration);
            randomRotation.y = m_Strength.y * m_YCurve.Evaluate(elapsedTime * m_YCurveLength / m_Duration);
            randomRotation.z = m_Strength.z * m_ZCurve.Evaluate(elapsedTime * m_ZCurveLength / m_Duration);
            finalLocalRotation = Quaternion.Euler(randomRotation);
        }

        public override void OnEnd()
        {
            base.OnEnd();
            finalLocalRotation = m_CachedLocalRotation;
        }
    }
}