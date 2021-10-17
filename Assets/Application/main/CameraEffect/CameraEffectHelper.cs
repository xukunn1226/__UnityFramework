using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Application.Runtime
{
    static public class CameraEffectHelper
    {
        static private CameraEffectInfo         m_EffectInfo;       // 正在执行的效果
        static private CinemachineVirtualCamera m_Camera;

        static public void Play(CameraEffectInfo info, CinemachineVirtualCamera camera, System.Action onFinished = null)
        {
            if (info == null)
            {
                throw new System.ArgumentNullException("CameraEffectInfo");
            }

            if (m_EffectInfo != null && m_EffectInfo.IsPlaying())
                return;

            StopCameraEffect();

            m_Camera = camera;
            m_EffectInfo = info;
            m_EffectInfo.Play(camera, onFinished);
        }

        static public void StopCameraEffect()
        {
            if (m_EffectInfo != null && m_EffectInfo.IsPlaying())
            {
                m_EffectInfo.Stop();

                UpdateCameraEffect();
            }
        }

        static public void UpdateCameraEffect()
        {
            if(m_EffectInfo == null)
                return;

            if( m_EffectInfo.IsPlaying() )
            {
                m_EffectInfo.UpdateCameraEffect();

                if( m_EffectInfo.m_ShakePosition.m_Active )
                {
                    m_Camera.transform.localPosition = m_EffectInfo.m_ShakePosition.finalLocalPosition;
                }

                if( m_EffectInfo.m_ShakeRotation.m_Active )
                {
                    m_Camera.transform.localRotation = m_EffectInfo.m_ShakeRotation.finalLocalRotation;
                }

                if( m_EffectInfo.m_ShakeFOV.m_Active )
                {
                    m_Camera.m_Lens.FieldOfView = m_EffectInfo.m_ShakeFOV.finalFOV;
                }
            }
            else
            {
                m_EffectInfo = null;
            }
        }
    }
}