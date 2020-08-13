using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Gesture.Runtime;

public class PlayerCamera : MonoBehaviour, IScreenDragHandler
{
    public Camera           mainCamera;
    private RaycastHit      m_HitInfo = new RaycastHit();
    private ref RaycastHit  m_HitInfoRef => ref m_HitInfo;
    private Vector3         m_DragVelocity;
    public float            DragSmoothTime;
    public float            SlideSmoothTime;
    private float           m_SmoothTime;
    public LayerMask        BaseLayer;
    public LayerMask        TerrainLayer;
    private float           m_OneScreenWidth;
    public bool             isDragging      { get; private set; }
    public bool             wasDragging     { get; private set; }
    public Vector3          dragStartPoint  { get; private set; }
    public Vector3          dragEndPoint    { get; private set; }
    private Vector3         m_StartCamera;

    [Tooltip("拖动屏幕结束时速度超过此值将有滑屏效果")]
    public float            FlyScreenSpeedThreshold;

    public float            SpeedValueOneScreen;

    void Start()
    {
        if(GamePlayerInput.Instance == null)
            throw new System.Exception("GamePlayerInput.Instance == null");
        
        if(mainCamera == null)
            throw new System.Exception("mainCamera == null");
        
        m_StartCamera = mainCamera.transform.position;
    }

    void LateUpdate()
    {
        CalcOneScreenWidth();

        // if(isDragging)
        {
            Vector3 delta = dragEndPoint - dragStartPoint;
            
            // mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, mainCamera.transform.position - delta, ref m_DragVelocity, m_SmoothTime);
            // mainCamera.transform.position = m_StartCamera - delta;
            // Debug.Log($"{delta}     {dragEndPoint}   {(mainCamera.transform.position - m_StartCamera).magnitude}");
        }
        // else if(wasDragging && !isDragging)
        // {
        //     // Debug.Log($"{m_DragVelocity}");
        //     // m_DragVelocity = Vector3.zero;
        // }
        // else
        // {

        // }

        // Vector3 delta = new Vector3(-25, 0, -15);
        // if(Input.GetKeyDown(KeyCode.Alpha1))
        // {
        //     GameObject obj = GameObject.Find("Marks/Cube(LongPress) (1)");
        //     if(obj != null)
        //     {
        //         obj.transform.position += delta;
        //     }
        // }

        // if(Input.GetKeyDown(KeyCode.Alpha2))
        // {
        //     mainCamera.transform.position += delta;
        // }
    }

    public void OnGesture(ScreenDragEventData eventData)
    {
        return;
        // Debug.Log($"Drag.........{eventData.State}   {eventData.Position}    {eventData.DeltaMove}   {Time.frameCount}");

        switch (eventData.State)
        {
            case RecognitionState.Started:
                Raycast(eventData.Position, TerrainLayer);
                dragStartPoint = m_HitInfo.point;
                m_StartCamera = mainCamera.transform.position;

                Vector3 p = mainCamera.ScreenToWorldPoint(new Vector3(eventData.Position.x, eventData.Position.y, 0.5f));

                wasDragging = false;
                isDragging = m_HitInfo.transform != null;

                m_SmoothTime = DragSmoothTime;
                
                break;
            case RecognitionState.InProgress:
                if(isDragging)
                { // 起始在Terrain
                    Raycast(eventData.Position, TerrainLayer);
                    dragEndPoint = m_HitInfo.transform != null ? m_HitInfo.point : dragEndPoint;
                    
                    // Debug.LogWarning($"{dragStartPoint}   {dragEndPoint}");
                    wasDragging = true;
                    isDragging = true;
                    // Debug.DrawLine(eventCamera.transform.position, dragEndPoint, Color.red);
                    // Debug.DrawLine(dragEndPoint, dragEndPoint + Vector3.up * 100, Color.green, 5);

                    Vector3 delta = dragEndPoint - dragStartPoint;
            
            // mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, m_StartCamera - delta, ref m_DragVelocity, m_SmoothTime);
                    mainCamera.transform.position = m_StartCamera - delta;
                }
                break;
            case RecognitionState.Failed:
            case RecognitionState.Ended:
                Raycast(eventData.Position, TerrainLayer);
                dragEndPoint = m_HitInfo.transform != null ? m_HitInfo.point : dragEndPoint;
                
                // Vector3 dist = Vector3.zero;
                // if(eventData.Speed.magnitude > FlyScreenSpeedThreshold)
                // {
                //     Vector3 nor = (dragEndPoint - dragStartPoint).normalized;
                //     dist = nor * eventData.Speed.magnitude / SpeedValueOneScreen * m_OneScreenWidth;
                //     dragEndPoint += dist;
                //     // Debug.DrawLine(mainCamera.transform.position, dragEndPoint, Color.red, 3);
                //     Debug.DrawLine(dragEndPoint, dragEndPoint + Vector3.up * 100, Color.green, 5);
                //     Debug.Log($"{m_DragVelocity}    {eventData.DeltaMove}   {dist.magnitude}      {eventData.Speed}");

                //     m_SmoothTime = SlideSmoothTime;
                // }
                



                wasDragging = isDragging;
                isDragging = false;
                break;
        }
    }

    private void Raycast(Vector2 screenPosition, int layerMask)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        Physics.Raycast(ray, out m_HitInfo, Mathf.Infinity, layerMask);
    }

    private void CalcOneScreenWidth()
    {
        Raycast(new Vector2(0, Screen.height * 0.5f), TerrainLayer);
        if(m_HitInfo.transform == null)
            return;
        Vector3 p1 = m_HitInfo.point;

        Raycast(new Vector2(Screen.width, Screen.height * 0.5f), TerrainLayer);
        if(m_HitInfo.transform == null)
            return;
        Vector3 p2 = m_HitInfo.point;

        m_OneScreenWidth = Vector3.Magnitude(p1 - p2);
    }
}
