using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Gesture.Runtime;

public class GamePlayerCamera : MonoBehaviour, IScreenDragHandler, IScreenPointerDownHandler
{
    static public GamePlayerCamera  Instance                { get; private set; }

    static public Camera        cam                         { get { return Instance?.mainCamera; } }

    public Camera               mainCamera;
    public LayerMask            TerrainLayer;
    private Plane               m_Ground;
    public float                GroundZ;    
    public float                DragSmoothTime;
    public float                SlideSmoothTime;
    private float               m_SmoothTime;
    private bool                m_IsDragging;
    private bool                m_WasDragging;
    private Vector3             m_StartPoint;
    private Vector3             m_EndPoint;
    private Vector3             m_Velocity;

    [Min(1000)][Tooltip("每一屏距离匹配的速度值")]
    public float                SpeedValueMatchOneScreen    = 1000;
    [Min(0.2f)][Tooltip("最大划屏数")]
    public float                MaxCountScreen;
    private Vector2             m_PendingScreenPos          = Vector2.zero;
    private bool                m_IsMoving;
    private float               m_LengthOfOneScreen;        // 一屏(横屏)对应的世界距离
    public bool                 ApplyBound;
    public Rect                 Bound;

    void Awake()
    {
        if(mainCamera == null)
            throw new System.Exception("mainCamera == null");

        if(FindObjectsOfType<GamePlayerCamera>().Length > 1)
        {
            DestroyImmediate(this);
            throw new System.Exception("PlayerCamera has already exist.");
        }

        Instance = this;
        m_Ground = new Plane(Vector3.up, new Vector3(0, GroundZ, 0));
    }

    void OnDestroy()
    {
        Instance = null;
    }

    void LateUpdate()
    {
        if(m_IsMoving)
        {
            // 确认目标点
            m_EndPoint = GetGroundHitPoint(m_PendingScreenPos);

            // 移动相机逼近目标多
            mainCamera.transform.position = Vector3.SmoothDamp( mainCamera.transform.position, 
                                                                mainCamera.transform.position - (m_EndPoint - m_StartPoint), 
                                                                ref m_Velocity, 
                                                                m_SmoothTime);

            // 移动结束判断
            if(m_WasDragging && !m_IsDragging)
            {
                if((m_EndPoint - m_StartPoint).sqrMagnitude < 1 || m_Velocity.sqrMagnitude < 1)
                {
                    m_IsMoving = false;
                }
            }
        }

        if(ApplyBound)
        {
            Vector3 pos = mainCamera.transform.position;
            pos.x = Mathf.Clamp(pos.x, Bound.xMin, Bound.xMax);
            pos.z = Mathf.Clamp(pos.z, Bound.yMin, Bound.yMax);
            mainCamera.transform.position = pos;
        }
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
        m_IsMoving = false;
        m_Velocity = Vector3.zero;
    }

    private void SetViewTarget(Vector2 screenPosition, float smoothTime)
    {
        m_IsMoving = true;
        m_PendingScreenPos = screenPosition;
        m_SmoothTime = smoothTime;
    }

    public void OnGesture(ScreenDragEventData eventData)
    {
        // Debug.Log($"Drag.........{eventData.State}   {eventData.Position}    {eventData.DeltaMove}   {Time.frameCount}");

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
        float dist = Mathf.Min(MaxCountScreen, eventData.Speed.magnitude / SpeedValueMatchOneScreen) * CalcLengthOfOneScreen();
        Vector3 endPos = GetGroundHitPoint(eventData.Position);
        Vector3 dir = (endPos - m_StartPoint).normalized;
        Vector3 pendingScreenPos = mainCamera.WorldToScreenPoint(endPos + dir * dist);
        SetViewTarget(pendingScreenPos, SlideSmoothTime);
        // Debug.Log($"{Mathf.Min(MaxCountScreen, eventData.Speed.magnitude / SpeedValueMatchOneScreen)}   {eventData.Speed.magnitude}     width: {GetScreenLandscape()}");
        // Debug.DrawLine(pos, pos + dir * dist, Color.red, 10);
        // Debug.DrawLine(m_StartPoint, pos, Color.green, 10);
    }
    
    private Vector3 GetGroundHitPoint(Vector2 screenPosition)
    {
        ///// method 1
        Ray mousePos = cam.ScreenPointToRay(screenPosition);
        float distance;
        m_Ground.Raycast(mousePos, out distance);
        return mousePos.GetPoint(distance);

        ///// method 2
        // Raycast(screenPosition, TerrainLayer, ref m_HitInfo);
        // return m_HitInfo.point;
    }
    
    private float CalcLengthOfOneScreen()
    {
        Vector3 l = GetGroundHitPoint(new Vector2(0,                    GetScreenPortrait() * 0.5f));
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
