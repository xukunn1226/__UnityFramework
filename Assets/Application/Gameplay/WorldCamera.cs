﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Gesture.Runtime;
using Framework.Core;
using Cinemachine;

namespace Application.Runtime
{
    /// <summary>
    /// 负责大世界的镜头表现
    /// <summary>
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class WorldCamera : MonoBehaviour
    {
        public WorldPlayerController        playerController;
        private CinemachineVirtualCamera    virtualCamera;
        [Min(0.01f)]
        public float                        DragSmoothTime;             // 拖拽时屏幕的跟随时间
        [Min(0.01f)]
        public float                        SlideSmoothTime;            // 拖拽结束时屏幕的跟随时间
        private float                       m_SmoothTime;
        private bool                        m_IsDragging;
        private bool                        m_WasDragging;
        private Vector3                     m_StartPoint;
        private Vector3                     m_EndPoint;
        private Vector3                     m_Velocity;

        [Min(1000)]
        public float                        DampingWhenDraggingFinished = 1000;
        [Min(0.2f)]
        [Tooltip("最大划屏数")]
        public float                        MaxLengthOfScreen;
        [Range(0.01f, 2)]
        public float                        PinchSensitivity            = 0.2f;
        private Vector2                     m_PendingScreenPos          = Vector2.zero;
        private bool                        m_IsDraggingCommand;
        private bool                        m_ApplyBound;
        private Rect                        m_Bound;
        private bool                        m_IsPinching;
        private bool                        m_WasPinching;
        private ScreenPinchEventData        m_PinchEventData;
        public float                        HeightOfRiseCamera;         // 此高度之下镜头略微抬起
        public float                        TargetCameraEulerX;         // 
        private Vector3                     m_OriginalEulerAngles;      // 相机初始角度

        private LinkedList<PositionEasingEvent> m_EasingEvent = new LinkedList<PositionEasingEvent>();

        void Awake()
        {
            if (playerController == null)
                throw new System.ArgumentNullException("playerController");

            virtualCamera = GetComponent<CinemachineVirtualCamera>();

            m_OriginalEulerAngles = virtualCamera.transform.eulerAngles;
#if UNITY_EDITOR || UNITY_STANDALONE
            PinchSensitivity *= 10;     // PC与真机模式灵敏度不一致
#endif            
        }

        void OnEnable()
        {
            if(PlayerInput.Instance == null)
                throw new System.Exception("PlayerInput == null");

            PlayerInput.Instance.OnScreenDragHandler += OnGesture;
            PlayerInput.Instance.OnScreenPinchHandler += OnGesture;
            PlayerInput.Instance.OnScreenPointerDownHandler += OnGesture;
        }

        void OnDisable()
        {
            if(PlayerInput.Instance == null)
                return;

            PlayerInput.Instance.OnScreenDragHandler -= OnGesture;
            PlayerInput.Instance.OnScreenPinchHandler -= OnGesture;
            PlayerInput.Instance.OnScreenPointerDownHandler -= OnGesture;
        }

        void LateUpdate()
        {
            if(!ProcessEasingEvents())
            { // 缓动事件处理完毕，才处理input
                ProcessInput();
            }

            UpdateCameraEffect();

            ApplyLimitedBound();
        }

        /// <summary>
        /// 处理镜头的缓动
        /// true: 仍有需求在处理中；false：没有或处理完毕
        /// <summary>
        private bool ProcessEasingEvents()
        {
            while(m_EasingEvent.Count > 0)
            {
                StopInput();
                
                PositionEasingEvent evt = m_EasingEvent.First.Value;
                Vector3 pos;
                bool isDone = evt.Poll(Time.deltaTime, out pos);
                virtualCamera.transform.position = pos;
                if(isDone)
                {
                    m_EasingEvent.RemoveFirst();
                }
                else
                {
                    break;
                }
            }
            return m_EasingEvent.Count > 0;
        }

        private void ProcessInput()
        {
            // process screen dragging
            if (m_IsDraggingCommand)
            {
                // 确认目标点
                m_EndPoint = playerController.GetGroundHitPoint(m_PendingScreenPos);

                // 移动相机逼近目标多
                virtualCamera.transform.position = Vector3.SmoothDamp(virtualCamera.transform.position,
                                                                      virtualCamera.transform.position - (m_EndPoint - m_StartPoint),
                                                                      ref m_Velocity,
                                                                      m_SmoothTime);

                // 移动结束判断
                if (m_WasDragging && !m_IsDragging)
                {
                    if (/*(m_EndPoint - m_StartPoint).sqrMagnitude < 1 ||*/ m_Velocity.sqrMagnitude < 1)
                    {
                        m_IsDraggingCommand = false;
                    }
                }
            }

            // process screen pinching
            if (m_IsPinching)
            {
                // XZ plane movement
                Vector3 curPos = playerController.GetGroundHitPoint(m_PinchEventData.Position);
                Vector3 delta = curPos - playerController.GetGroundHitPoint(m_PinchEventData.PrevPosition);
                
                // camera to focus point movement
                Vector3 camPos = virtualCamera.transform.position - delta;
                Vector3 dir = (curPos - camPos).normalized;
                float deltaMove = m_PinchEventData.DeltaMove * PinchSensitivity;
                Vector2 absoluteHeightRange = playerController.GetAbsoluteHeightRange();
                if (deltaMove > 0)      // 向前推进
                {
                    if (camPos.y > absoluteHeightRange.x + 0.001f)      // 0.001f确保相机始终处于最低值之上
                    {
                        camPos += dir * deltaMove;
                        camPos.y = Mathf.Clamp(camPos.y, absoluteHeightRange.x + 0.0001f, absoluteHeightRange.y);        // 0.0001f确保相机高度略低于0.001f，而在最低处停止更新
                    }
                }
                else if (deltaMove < 0)
                {
                    if (camPos.y < absoluteHeightRange.y - 0.001f)
                    {
                        camPos += dir * deltaMove;
                        camPos.y = Mathf.Clamp(camPos.y, absoluteHeightRange.x, absoluteHeightRange.y - 0.0001f);
                    }
                }
                virtualCamera.transform.position = camPos;
            }

            if(virtualCamera.transform.position.y < HeightOfRiseCamera)
            {
                float alpha = (HeightOfRiseCamera - virtualCamera.transform.position.y) / (HeightOfRiseCamera - playerController.lowestView);
                virtualCamera.transform.eulerAngles = new Vector3(m_OriginalEulerAngles.x * (1 - alpha) + TargetCameraEulerX * alpha, m_OriginalEulerAngles.y, m_OriginalEulerAngles.z);
            }
            else
            {
                virtualCamera.transform.eulerAngles = m_OriginalEulerAngles;
            }
        }

        private void StopInput()
        {
            m_IsDraggingCommand = false;
            m_IsPinching = false;
        }

        private void ApplyLimitedBound()
        {
            if (m_ApplyBound)
            {
                Vector3 pos = virtualCamera.transform.position;
                pos.x = Mathf.Clamp(pos.x, m_Bound.xMin, m_Bound.xMax);
                pos.z = Mathf.Clamp(pos.z, m_Bound.yMin, m_Bound.yMax);
                virtualCamera.transform.position = pos;
            }
        }

        public void SetLimitedBound(Rect bound)
        {
            m_ApplyBound = true;
            m_Bound = bound;
        }

        public void SetUnlimitedBound()
        {
            m_ApplyBound = false;
        }

        private void OnGesture(ScreenPointerDownEventData eventData)
        {
            // Debug.Log($"ScreenPointerDownEventData:       {Time.frameCount}");

            // stop the dragging if possible
            m_IsDraggingCommand = false;
            m_Velocity = Vector3.zero;
        }
        
        private void OnGesture(ScreenPinchEventData eventData)
        {
            // Debug.Log($"Pinch..........{eventData.State}   {eventData.Position}    {eventData.DeltaMove}    {Time.frameCount}");

            m_PinchEventData = eventData;
            switch (eventData.State)
            {
                case RecognitionState.Started:
                    m_WasPinching = false;
                    m_IsPinching = true;
                    break;
                case RecognitionState.InProgress:
                    m_WasPinching = m_IsPinching;
                    m_IsPinching = true;
                    break;
                case RecognitionState.Failed:
                case RecognitionState.Ended:
                    m_WasPinching = m_IsPinching;
                    m_IsPinching = false;
                    break;
            }
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
        
        private void SetViewTarget(Vector2 screenPosition, float smoothTime)
        {
            m_IsDraggingCommand = true;
            m_PendingScreenPos = screenPosition;
            m_SmoothTime = smoothTime;
        }

        private void BeginScreenDrag(ScreenDragEventData eventData)
        {
            m_StartPoint = playerController.GetGroundHitPoint(eventData.Position);
            SetViewTarget(eventData.Position, DragSmoothTime);
        }

        private void UpdateScreenDrag(ScreenDragEventData eventData)
        {
            SetViewTarget(eventData.Position, DragSmoothTime);
        }

        private void EndScreenDrag(ScreenDragEventData eventData)
        {
            float dist = Mathf.Min(MaxLengthOfScreen, eventData.Speed.magnitude / DampingWhenDraggingFinished) * playerController.CalcLengthOfOneScreen();
            Vector3 endPos = playerController.GetGroundHitPoint(eventData.Position);
            Vector3 dir = (endPos - m_StartPoint).normalized;
            Vector3 pendingScreenPos = playerController.WorldToScreenPoint(endPos + dir * dist);
            SetViewTarget(pendingScreenPos, SlideSmoothTime);
        }

        public void AddEasingEvent(PositionEasingEvent evt)
        {
            m_EasingEvent.AddLast(evt);
        }

        public void PlayCameraEffect(CameraEffectInfo info, System.Action onFinished = null)
        {
            CameraEffectHelper.Play(info, virtualCamera, onFinished);
        }

        public void StopCameraEffect()
        {
            CameraEffectHelper.StopCameraEffect();
        }

        private void UpdateCameraEffect()
        {
            CameraEffectHelper.UpdateCameraEffect();
        }
    }
}