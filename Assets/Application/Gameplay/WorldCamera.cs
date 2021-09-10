using System.Collections;
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
    public class WorldCamera : SingletonMono<WorldCamera>
    {
        static public Camera cam { get { return Instance?.mainCamera; } }

        public Camera                   mainCamera;
        public CinemachineVirtualCamera virtualCamera;
        public LayerMask                TerrainLayer;
        private Plane                   m_Ground;               // 虚拟水平面
        [Tooltip("水平面高度")]
        public float                    GroundZ;                // 水平面高度
        [Min(0.01f)]
        public float                    DragSmoothTime;         // 拖拽时屏幕的跟随时间
        [Min(0.01f)]
        public float                    SlideSmoothTime;        // 拖拽结束时屏幕的跟随时间
        private float                   m_SmoothTime;
        private bool                    m_IsDragging;
        private bool                    m_WasDragging;
        private Vector3                 m_StartPoint;
        private Vector3                 m_EndPoint;
        private Vector3                 m_Velocity;

        [Min(1000)]
        public float                    DampingWhenDraggingFinished = 1000;
        [Min(0.2f)]
        [Tooltip("最大划屏数")]
        public float                    MaxCountScreen;
        [Range(0.01f, 2)]
        public float                    PinchSensitivity            = 0.2f;
        private Vector2                 m_PendingScreenPos          = Vector2.zero;
        private bool                    m_IsDraggingCommand;
        private float                   m_LengthOfOneScreen;        // 一屏(横屏)对应的世界距离
        public bool                     ApplyBound;
        public Rect                     Bound;
        private bool                    m_IsPinching;
        private bool                    m_WasPinching;
        private ScreenPinchEventData    m_PinchEventData;
        public Vector2                  HeightRange;
        public float                    HeightOfRiseCamera;         // 此高度之下镜头略微抬起
        public float                    TargetCameraEulerX;         // 
        private Vector3                 m_OriginalEulerAngles;      // 相机初始角度

        protected override void Awake()
        {
            base.Awake();

            if (mainCamera == null)
                throw new System.ArgumentNullException("mainCamera");
            if(virtualCamera == null)
                throw new System.ArgumentNullException("virtualCamera");

            m_OriginalEulerAngles = virtualCamera.transform.eulerAngles;
            m_Ground = new Plane(Vector3.up, new Vector3(0, GroundZ, 0));
#if UNITY_EDITOR || UNITY_STANDALONE
            PinchSensitivity *= 10;     // PC与真机模式灵敏度不一致
#endif            
        }

        void OnEnable()
        {
            if(GamePlayerInput.Instance == null)
                throw new System.Exception("GamePlayerInput == null");

            GamePlayerInput.Instance.OnScreenDragHandler += OnGesture;
            GamePlayerInput.Instance.OnScreenPinchHandler += OnGesture;
            GamePlayerInput.Instance.OnScreenPointerDownHandler += OnGesture;
        }

        void OnDisable()
        {
            if(GamePlayerInput.Instance == null)
                return;

            GamePlayerInput.Instance.OnScreenDragHandler -= OnGesture;
            GamePlayerInput.Instance.OnScreenPinchHandler -= OnGesture;
            GamePlayerInput.Instance.OnScreenPointerDownHandler -= OnGesture;
        }

        void LateUpdate()
        {
            // process screen dragging
            if (m_IsDraggingCommand)
            {
                // 确认目标点
                m_EndPoint = GetGroundHitPoint(m_PendingScreenPos);

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
                Vector3 curPos = GetGroundHitPoint(m_PinchEventData.Position);
                Vector3 delta = curPos - GetGroundHitPoint(m_PinchEventData.PrevPosition);
                virtualCamera.transform.position -= delta;

                // camera to focus point movement
                Vector3 camPos = virtualCamera.transform.position;
                Vector3 dir = (curPos - camPos).normalized;
                float deltaMove = m_PinchEventData.DeltaMove * PinchSensitivity;
                if (deltaMove > 0)      // 向前推进
                {
                    if (camPos.y > GroundZ + HeightRange.x + 1)
                    {
                        camPos += dir * deltaMove;
                        camPos.y = Mathf.Clamp(camPos.y, GroundZ + HeightRange.x, GroundZ + HeightRange.y);
                    }
                }
                else if (deltaMove < 0)
                {
                    if (camPos.y < GroundZ + HeightRange.y - 1)
                    {
                        camPos += dir * deltaMove;
                        camPos.y = Mathf.Clamp(camPos.y, GroundZ + HeightRange.x, GroundZ + HeightRange.y);
                    }
                }
                virtualCamera.transform.position = camPos;
            }

            if(virtualCamera.transform.position.y < HeightOfRiseCamera)
            {
                float alpha = (HeightOfRiseCamera - virtualCamera.transform.position.y) / (HeightOfRiseCamera - HeightRange.x);
                virtualCamera.transform.eulerAngles = new Vector3(m_OriginalEulerAngles.x * (1 - alpha) + TargetCameraEulerX * alpha, m_OriginalEulerAngles.y, m_OriginalEulerAngles.z);
            }
            else
            {
                virtualCamera.transform.eulerAngles = m_OriginalEulerAngles;
            }

            if (ApplyBound)
            {
                Vector3 pos = virtualCamera.transform.position;
                pos.x = Mathf.Clamp(pos.x, Bound.xMin, Bound.xMax);
                pos.z = Mathf.Clamp(pos.z, Bound.yMin, Bound.yMax);
                virtualCamera.transform.position = pos;
            }

            // Vector3 mousePos = Input.mousePosition;
            // mousePos.z = 0.34f;
            // Vector3 pos1 = Camera.main.ScreenToViewportPoint (mousePos);
            // Debug.Log($"------- VP: {pos1.z}      mousePosition: {Input.mousePosition.z}");

            // if(GamePlayerInput.Instance.currentSelectedGameObject != null)
            // {
            //     Vector3 pos = mainCamera.WorldToViewportPoint(GamePlayerInput.Instance.currentSelectedGameObject.transform.position);
            //     Debug.Log($"{pos}   {GamePlayerInput.Instance.currentSelectedGameObject.name}");
            // }
        }

        public void ApplyLimitedBound(Rect bound)
        {
            ApplyBound = true;
            Bound = bound;
        }

        public void UnapplyLimitedBound()
        {
            ApplyBound = false;
        }

        public void OnGesture(ScreenPointerDownEventData eventData)
        {
            // Debug.Log($"ScreenPointerDownEventData:       {Time.frameCount}");

            // stop the dragging if possible
            m_IsDraggingCommand = false;
            m_Velocity = Vector3.zero;
        }

        private void SetViewTarget(Vector2 screenPosition, float smoothTime)
        {
            m_IsDraggingCommand = true;
            m_PendingScreenPos = screenPosition;
            m_SmoothTime = smoothTime;
        }

        public void OnGesture(ScreenPinchEventData eventData)
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

        public void OnGesture(ScreenDragEventData eventData)
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

        private void BeginScreenDrag(ScreenDragEventData eventData)
        {
            m_StartPoint = GetGroundHitPoint(eventData.Position);
            SetViewTarget(eventData.Position, DragSmoothTime);
        }

        private void UpdateScreenDrag(ScreenDragEventData eventData)
        {
            SetViewTarget(eventData.Position, DragSmoothTime);
        }

        private void EndScreenDrag(ScreenDragEventData eventData)
        {
            float dist = Mathf.Min(MaxCountScreen, eventData.Speed.magnitude / DampingWhenDraggingFinished) * CalcLengthOfOneScreen();
            Vector3 endPos = GetGroundHitPoint(eventData.Position);
            Vector3 dir = (endPos - m_StartPoint).normalized;
            Vector3 pendingScreenPos = mainCamera.WorldToScreenPoint(endPos + dir * dist);
            SetViewTarget(pendingScreenPos, SlideSmoothTime);
            
#if UNITY_EDITOR
            Debug.DrawLine(endPos + Vector3.up, endPos - Vector3.up, Color.red, 1);
#endif            
            // Debug.Log($"{Mathf.Min(MaxCountScreen, eventData.Speed.magnitude / DampingWhenDraggingFinished)}   {eventData.Speed.magnitude}     width: {GetScreenLandscape()}");
            // Debug.DrawLine(pos, pos + dir * dist, Color.red, 10);
            // Debug.DrawLine(m_StartPoint, pos, Color.green, 10);
        }

        // 水平面交点坐标
        public Vector3 GetGroundHitPoint(Vector2 screenPosition)
        {
            ///// method 1
            Ray mousePos = mainCamera.ScreenPointToRay(screenPosition);
            float distance;
            m_Ground.Raycast(mousePos, out distance);
            return mousePos.GetPoint(distance);

            ///// method 2
            // Raycast(screenPosition, TerrainLayer, ref m_HitInfo);
            // return m_HitInfo.point;
        }

        // 计算一屏的水平距离
        private float CalcLengthOfOneScreen()
        {
            Vector3 l = GetGroundHitPoint(new Vector2(0, GetScreenPortrait() * 0.5f));
            Vector3 r = GetGroundHitPoint(new Vector2(GetScreenLandscape(), GetScreenPortrait() * 0.5f));
            return (r - l).magnitude;
        }

        // 适配横竖屏，预留接口
        private float GetScreenLandscape()
        {
            return Screen.width;
        }

        private float GetScreenPortrait()
        {
            return Screen.height;
        }

        static public bool Raycast(Vector2 screenPosition, int layerMask, ref RaycastHit hitInfo)
        {
            Ray ray = cam.ScreenPointToRay(screenPosition);

            return PhysUtility.Raycast(ray, 1000, layerMask, ref hitInfo);
        }

        static public ref readonly RaycastHit Raycast(Vector2 screenPosition, int layerMask)
        {
            Ray ray = cam.ScreenPointToRay(screenPosition);

            return ref PhysUtility.Raycast(ray, 1000, layerMask);
        }
    }
}