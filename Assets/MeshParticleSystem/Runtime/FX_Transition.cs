using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
{
    public class FX_Transition : MonoBehaviour
    {
        public float            Delay;
        [Min(0)]
        public float            Duration;

        public bool             WorldSpace;                     // 更新transform的世界坐标
        public bool             Addition;
        public Vector3          Target;
        public AnimationCurve   Curve = new AnimationCurve(FX_Const.defaultKeyFrames);
        
        private float           m_ElapsedTime;
        private float           m_Delay;
        private Vector3         m_OriginalLocalPos;
        private Vector3         m_OriginalWorldPos;
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
            // 初始位置必须在Start中记录，Awake中position可能尚未初始化
            m_OriginalLocalPos = transform.localPosition;
            m_OriginalWorldPos = transform.position;

            Init();
        }

        private void Init()
        {
            // 恢复初始位置
            transform.position = m_OriginalWorldPos;
            transform.localPosition = m_OriginalLocalPos;

            m_ElapsedTime = 0;
            m_Delay = Delay;            
        }

        void Update()
        {
            m_Delay -= Time.deltaTime;
            if (m_Delay > 0)
                return;

            float percent = 1;
            if (Duration > 0)
            {
                m_ElapsedTime += Time.deltaTime;
                percent = m_ElapsedTime / Duration;
            }
            float value = Curve.Evaluate(percent);

            if (WorldSpace)
            {
                if(Addition)
                    transform.position = Vector3.Lerp(m_OriginalWorldPos, m_OriginalWorldPos + Target, value);
                else
                    transform.position = Vector3.Lerp(m_OriginalWorldPos, Target, value);
            }
            else
            {
                if(Addition)
                    transform.localPosition = Vector3.Lerp(m_OriginalLocalPos, m_OriginalLocalPos + Target, value);
                else
                    transform.localPosition = Vector3.Lerp(m_OriginalLocalPos, Target, value);
            }
        }
    }
}
