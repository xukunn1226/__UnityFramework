using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cinemachine.Examples
{
    public class BrainListener : MonoBehaviour
    {
        private CinemachineBrain m_Brain;

        private void Awake()
        {
            m_Brain = GetComponent<CinemachineBrain>();
            m_Brain.m_CameraCutEvent.AddListener(OnCameraCut);
            m_Brain.m_CameraActivatedEvent.AddListener(OnCameraActivatedEvent);
        }

        private void OnDestroy()
        {
            m_Brain.m_CameraCutEvent.RemoveListener(OnCameraCut);
            m_Brain.m_CameraActivatedEvent.RemoveListener(OnCameraActivatedEvent);
        }

        void OnCameraCut(CinemachineBrain brain)
        {
            Debug.Log($"------ cut");
        }

        void OnCameraActivatedEvent(ICinemachineCamera incomingVcam, ICinemachineCamera outgoingVcam)
        {
            Debug.Log($"OnCameraActivatedEvent: incomingVcam  [{incomingVcam?.Name}]     outgoingVcam  [{outgoingVcam?.Name}]       time: {Time.time}");
        }
    }
}