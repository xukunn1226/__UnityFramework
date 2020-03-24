using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
{
    public class FX_Rotation : MonoBehaviour
    {
        public float            Delay;
        [Min(0)]
        public float            Duration;

        public bool             WorldSpace;
        public bool             Addition;
        public Vector3          Target;
        public AnimationCurve   Curve = new AnimationCurve(FX_Const.defaultKeyFrames);

        private float           m_ElapsedTime;
        private float           m_Delay;
        private Vector3         m_OriginalLocalEuler;
        private Vector3         m_OriginalWorldEuler;
        private bool            m_isStarted;

        void OnEnable()
        {
            if (!m_isStarted)
                return;

            Init();
        }

        private void Start()
        {
            m_isStarted = true;

            // 记录初始位置
            // 初始位置必须在Start中记录，Awake中rotation可能尚未初始化
            m_OriginalLocalEuler = transform.localEulerAngles;
            m_OriginalWorldEuler = transform.eulerAngles;

            Init();
        }

        private void Init()
        {
            // 恢复初始朝向
            transform.eulerAngles = m_OriginalWorldEuler;
            transform.localEulerAngles = m_OriginalLocalEuler;

            m_ElapsedTime = 0;
            m_Delay = Delay;
        }

        void Update()
        {
            m_Delay -= Time.deltaTime;
            if (m_Delay > 0)
                return;

            float percent = 1;
            if(Duration > 0)
            {
                m_ElapsedTime += Time.deltaTime;
                percent = m_ElapsedTime / Duration;
            }
            float value = Curve.Evaluate(percent);

            if (WorldSpace)
            {
                if (Addition)
                    transform.rotation = Quaternion.Lerp(Quaternion.Euler(m_OriginalWorldEuler), Quaternion.Euler(m_OriginalWorldEuler + Target), value);
                else
                    transform.rotation = Quaternion.Lerp(Quaternion.Euler(m_OriginalWorldEuler), Quaternion.Euler(Target), value);
            }
            else
            {
                if (Addition)
                    transform.localRotation = Quaternion.Lerp(Quaternion.Euler(m_OriginalLocalEuler), Quaternion.Euler(m_OriginalLocalEuler + Target), value);
                else
                    transform.localRotation = Quaternion.Lerp(Quaternion.Euler(m_OriginalLocalEuler), Quaternion.Euler(Target), value);
            }
        }
    }
}
