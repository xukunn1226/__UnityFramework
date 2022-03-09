using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Framework.Gesture.Runtime;

namespace Application.Runtime
{
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class CastleCamera : MonoBehaviour
    {
        public Transform                    LookAt;
        private CinemachineVirtualCamera    virtualCamera;
        private bool                        m_IsDragging;
        private bool                        m_WasDragging;
        private LensSettings                m_InitLensSettings;
        private Vector3                     m_InitPosition;
        private Quaternion                  m_InitRotation;

        public float initDepth { get { return LookAt.transform.position.z - m_InitPosition.z; } }
        public float depth { get { return LookAt.transform.position.z - transform.position.z; } }

        void Awake()
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            m_InitLensSettings = virtualCamera.m_Lens;
            m_InitPosition = virtualCamera.transform.position;
            m_InitRotation = virtualCamera.transform.rotation;
        }
        
        void OnEnable()
        {
            PlayerInput.Instance.OnScreenDragHandler += OnGesture;
            PlayerInput.Instance.OnScreenPinchHandler += OnGesture;
            PlayerInput.Instance.OnScreenPointerDownHandler += OnGesture;
        }

        void OnDisable()
        {
            PlayerInput.Instance.OnScreenDragHandler -= OnGesture;
            PlayerInput.Instance.OnScreenPinchHandler -= OnGesture;
            PlayerInput.Instance.OnScreenPointerDownHandler -= OnGesture;
        }

        private void OnGesture(ScreenPointerDownEventData eventData)
        {
            // Debug.Log($"ScreenPointerDownEventData:       {Time.frameCount}");

            // stop the dragging if possible
            // m_IsDraggingCommand = false;
            // m_Velocity = Vector3.zero;
        }
        
        private void OnGesture(ScreenPinchEventData eventData)
        {
            // Debug.Log($"Pinch..........{eventData.State}   {eventData.Position}    {eventData.DeltaMove}    {Time.frameCount}");

            // m_PinchEventData = eventData;
            // switch (eventData.State)
            // {
            //     case RecognitionState.Started:
            //         m_WasPinching = false;
            //         m_IsPinching = true;
            //         break;
            //     case RecognitionState.InProgress:
            //         m_WasPinching = m_IsPinching;
            //         m_IsPinching = true;
            //         break;
            //     case RecognitionState.Failed:
            //     case RecognitionState.Ended:
            //         m_WasPinching = m_IsPinching;
            //         m_IsPinching = false;
            //         break;
            // }
        }

        private void OnGesture(ScreenDragEventData eventData)
        {
            switch (eventData.State)
            {
                case RecognitionState.Started:
                    m_WasDragging = false;
                    m_IsDragging = true;
                    BeginScreenDrag(eventData);
                    break;
                case RecognitionState.InProgress:
                    m_WasDragging = m_IsDragging;
                    m_IsDragging = true;
                    UpdateScreenDrag(eventData);
                    break;
                case RecognitionState.Failed:
                case RecognitionState.Ended:
                    m_WasDragging = m_IsDragging;
                    m_IsDragging = false;
                    EndScreenDrag(eventData);
                    break;
            }
        }

        private Vector2 m_DeltaMove          = Vector2.zero;
        private bool    m_IsDraggingCommand;
        private float   m_SmoothTime;

        private void BeginScreenDrag(ScreenDragEventData eventData)
        {
            SetViewTarget(eventData.DeltaMove, 0.1f);
        }

        private void UpdateScreenDrag(ScreenDragEventData eventData)
        {
            SetViewTarget(eventData.DeltaMove, 0.1f);
        }

        private void EndScreenDrag(ScreenDragEventData eventData)
        {
            SetViewTarget(eventData.DeltaMove, 0.2f);
        }

        private void SetViewTarget(Vector2 screenPosition, float smoothTime)
        {
            m_IsDraggingCommand = true;
            m_DeltaMove = screenPosition;
            m_SmoothTime = smoothTime;
        }

        public Vector2 dragScale;
        private void Update()
        {
            if(m_IsDraggingCommand)
            {
                virtualCamera.transform.position -= new Vector3(m_DeltaMove.x * dragScale.x, m_DeltaMove.y * dragScale.y, 0);
                m_IsDraggingCommand = false;
            }
        }
    }
}