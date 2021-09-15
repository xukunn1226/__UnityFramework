using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Application.Runtime
{
    [System.Serializable]
    public class CameraEffectShakePosition : CameraEffectBase
    {
        public Vector3          m_Strength;
        public AnimationCurve   m_XCurve                = AnimationCurve.Linear(0, 1, 1, 0);
        public AnimationCurve   m_YCurve                = AnimationCurve.Linear(0, 1, 1, 0);
        public AnimationCurve   m_ZCurve                = AnimationCurve.Linear(0, 1, 1, 0);
        private float           m_XCurveLength;
        private float           m_YCurveLength;
        private float           m_ZCurveLength;
        private Vector3         m_CachedLocalPosition;
        public Vector3          finalLocalPosition      { get; private set; }

        public override void OnBegin(CinemachineVirtualCamera camera, float duration)
        {
            base.OnBegin(camera, duration);

            m_CachedLocalPosition = camera.transform.localPosition;
            finalLocalPosition = m_CachedLocalPosition;
            m_XCurveLength = m_XCurve.keys.Length > 0 ? m_XCurve.keys[m_XCurve.keys.Length - 1].time : 0;
            m_YCurveLength = m_YCurve.keys.Length > 0 ? m_YCurve.keys[m_YCurve.keys.Length - 1].time : 0;
            m_ZCurveLength = m_ZCurve.keys.Length > 0 ? m_ZCurve.keys[m_ZCurve.keys.Length - 1].time : 0;
        }

        public override void OnSample(float elapsedTime)
        {
            Vector3 randomPosition = Vector3.zero;

            randomPosition.x = m_Strength.x * m_XCurve.Evaluate(elapsedTime * m_XCurveLength / m_Duration);
            randomPosition.y = m_Strength.y * m_YCurve.Evaluate(elapsedTime * m_YCurveLength / m_Duration);
            randomPosition.z = m_Strength.z * m_ZCurve.Evaluate(elapsedTime * m_ZCurveLength / m_Duration);

            finalLocalPosition = m_CachedLocalPosition + randomPosition;        // 叠加效果
        }

        public override void OnEnd()
        {
            base.OnEnd();
            finalLocalPosition = m_CachedLocalPosition;
        }
    }
}