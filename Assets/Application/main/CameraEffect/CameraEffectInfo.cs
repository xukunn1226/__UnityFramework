using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Application.Runtime
{
    [CreateAssetMenu(menuName = "创建相机震屏效果", fileName = "CameraEffectInfo")]
    public class CameraEffectInfo : ScriptableObject
    {
        [Tooltip("震屏时间")]
        public float                        m_Duration          = 1;
        public CameraEffectShakePosition    m_ShakePosition     = new CameraEffectShakePosition();
        public CameraEffectShakeRotation    m_ShakeRotation     = new CameraEffectShakeRotation();
        public CameraEffectFOV              m_ShakeFOV          = new CameraEffectFOV();
        private float                       m_StartTime;
        private System.Action               onFinished;
        private CinemachineVirtualCamera    m_Camera;

        private enum CameraEffectState
        {
            None,
            Begin,
            Sample,
            End,
        }
        private CameraEffectState           m_EffectState       = CameraEffectState.None;

        private void Begin()
        {
            m_StartTime = Time.time;
            m_Duration = Mathf.Max(0.01f, m_Duration);

            if( m_ShakePosition.m_Active )
            {
                m_ShakePosition.OnBegin(m_Camera, m_Duration);
            }
            if( m_ShakeRotation.m_Active )
            {
                m_ShakeRotation.OnBegin(m_Camera, m_Duration);
            }
            if( m_ShakeFOV.m_Active )
            {
                m_ShakeFOV.OnBegin(m_Camera, m_Duration);
            }
        }

        private void Sample(float elapsedTime)
        {
            if (m_ShakePosition.m_Active)
            {
                m_ShakePosition.OnSample(elapsedTime);
            }
            if (m_ShakeRotation.m_Active)
            {
                m_ShakeRotation.OnSample(elapsedTime);
            }
            if (m_ShakeFOV.m_Active)
            {
                m_ShakeFOV.OnSample(elapsedTime);
            }
        }

        private void End()
        {
            if (m_ShakePosition.m_Active)
            {
                m_ShakePosition.OnEnd();
            }
            if (m_ShakeRotation.m_Active)
            {
                m_ShakeRotation.OnEnd();
            }
            if (m_ShakeFOV.m_Active)
            {
                m_ShakeFOV.OnEnd();
            }
        }
        
        /// <summary>
        /// 震屏更新流程
        /// </summary>
        public void UpdateCameraEffect()
        {
            switch(m_EffectState)
            {
                case CameraEffectState.Begin:
                    {
                        Begin();
                        m_EffectState = CameraEffectState.Sample;
                    }
                    break;
                case CameraEffectState.Sample:
                    {
                        float elapsedTime = Time.time - m_StartTime;
                        if( elapsedTime < m_Duration )
                        {
                            Sample(elapsedTime);
                        }
                        else
                        {
                            m_EffectState = CameraEffectState.End;
                        }
                    }
                    break;
                case CameraEffectState.End:
                    {
                        End();
                        onFinished?.Invoke();
                        onFinished = null;
                        m_EffectState = CameraEffectState.None;
                    }
                    break;
            }
        }

        /// <summary>
        /// 播放相机效果
        /// </summary>
        public void Play(CinemachineVirtualCamera camera, System.Action onFinished = null)
        {
            m_EffectState = CameraEffectState.Begin;
            m_Camera = camera;
            this.onFinished = onFinished;
        }

        /// <summary>
        /// 立即停止播放相机效果
        /// </summary>
        public void Stop()
        {
            if( IsPlaying() )
            {
                m_EffectState = CameraEffectState.End;
            }
        }

        public bool IsPlaying()
        {
            return m_EffectState != CameraEffectState.None;
        }
    }
}