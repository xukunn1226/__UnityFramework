using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
{
    public class FX_Rotation : MonoBehaviour
    {
        public bool             Addition;

        public float            Delay;
        public float            Duration;
        public bool             Loop;

        public bool             WorldSpace;
        public Vector3          Target;
        public AnimationCurve   Curve = new AnimationCurve(FX_Const.defaultKeyFrames);

        public bool             lateUpdate;

        private float           m_Duration;
        private float           m_Delay;
        private Vector3         m_OriginalLocalEuler;
        private Vector3         m_OriginalWorldEuler;
        private Vector3         m_StartLocalEuler;
        private Vector3         m_StartWorldEuler;
        private Vector3         m_LocalEuler;
        private Vector3         m_WorldEuler;

        private void Awake()
        {
            m_OriginalLocalEuler = transform.localEulerAngles;
            m_OriginalWorldEuler = transform.eulerAngles;
        }

        private void OnEnable()
        {
            transform.eulerAngles = m_OriginalWorldEuler;
            transform.localEulerAngles = m_OriginalLocalEuler;

            // 重复使用时需要先设置到正确位置，再active
            m_StartLocalEuler = transform.localEulerAngles;
            m_StartWorldEuler = transform.eulerAngles;

            m_Duration = Duration;
            m_Delay = Delay;
        }

        void UpdateDataInternal()
        {
            m_Delay -= Time.deltaTime;
            if (m_Delay > 0)
                return;

            m_Duration -= Time.deltaTime;
            float percent = 1;
            if (m_Duration >= 0)
            {
                percent = 1 - (m_Duration / Duration);
            }
            else
            {
                if (Loop)
                {
                    m_Duration += Duration;
                    percent = 1 - (m_Duration / Duration);
                }
            }

            float value = Curve.Evaluate(percent);
            if (WorldSpace)
            {
                if (Addition)
                    m_WorldEuler = Vector3.Lerp(m_StartWorldEuler, m_StartWorldEuler + Target, value);
                else
                    m_WorldEuler = Vector3.Lerp(m_StartWorldEuler, Target, value);
                transform.eulerAngles = m_WorldEuler;
            }
            else
            {
                if (Addition)
                    m_LocalEuler = Vector3.Lerp(m_StartLocalEuler, m_StartLocalEuler + Target, value);
                else
                    m_LocalEuler = Vector3.Lerp(m_StartLocalEuler, Target, value);
                transform.localEulerAngles = m_LocalEuler;
            }
        }

        void Update()
        {
            if (!lateUpdate)
            {
                UpdateDataInternal();
            }            
        }
        
        private void LateUpdate()
        {
            if (lateUpdate)
            {
                UpdateDataInternal();
            }
        }
    }
}
