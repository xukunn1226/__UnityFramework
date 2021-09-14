using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    [System.Serializable]
    public class CameraEffectShakePosition : CameraEffectBase
    {
        public Vector3          m_Strength;

        public AnimationCurve   m_XCurve            = AnimationCurve.Linear(0, 1, 1, 0);
        public AnimationCurve   m_YCurve            = AnimationCurve.Linear(0, 1, 1, 0);
        public AnimationCurve   m_ZCurve            = AnimationCurve.Linear(0, 1, 1, 0);

        public Vector3          finalPosition       { get; private set; }

        private float           m_XCurveLength;
        private float           m_YCurveLength;
        private float           m_ZCurveLength;

        public override void OnBegin(float duration)
        {
            base.OnBegin(duration);

            finalPosition = Vector3.zero;
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

            finalPosition = randomPosition;
        }

        public override void OnEnd()
        {
            base.OnEnd();

            finalPosition = Vector3.zero;
        }
    }
}