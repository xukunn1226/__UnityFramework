using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Gesture.Runtime;

public class GamePlayerCamera : MonoBehaviour, IScreenDragHandler, IScreenPointerDownHandler
{
    static public GamePlayerCamera  Instance                    { get; private set; }

    static public Camera        cam                         { get { return Instance?.mainCamera; } }

    public Camera               mainCamera;
    public LayerMask            TerrainLayer;
    private RaycastHit          m_HitInfo                   = new RaycastHit();
    private Plane               m_Ground;
    public float                GroundZ;
    private Vector3             m_DragVelocity;
    public float                DragSmoothTime;
    public float                SlideSmoothTime;
    private float               m_SmoothTime;
    private bool                m_IsDragging;
    private bool                m_WasDragging;
    private Vector3             m_DragStartPoint;
    private Vector3             m_DragEndPoint;

    [Min(1000)][Tooltip("每一屏距离匹配的速度值")]
    public float                SpeedValueMatchOneScreen    = 1000;
    [Min(0.2f)][Tooltip("最大划屏数")]
    public float                MaxCountScreen;
    private Vector2             m_PendingScreenPos          = Vector2.zero;
    private bool                m_IsMoving;

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
            m_DragEndPoint = GetGroundHitPoint(m_PendingScreenPos);

            // 移动相机逼近目标多
            Vector3 delta = m_DragEndPoint - m_DragStartPoint;            
            mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, mainCamera.transform.position - delta, ref m_DragVelocity, m_SmoothTime);

            // 移动结束判断
            if(m_WasDragging && !m_IsDragging)
            {
                if(delta.sqrMagnitude < 1 || m_DragVelocity.sqrMagnitude < 1)
                {
                    m_IsMoving = false;
                }
            }
        }
    }

    public void OnGesture(ScreenPointerDownEventData eventData)
    {
        // Debug.Log($"ScreenPointerDownEventData:       {Time.frameCount}");
        m_IsMoving = false;
        m_DragVelocity = Vector3.zero;
    }

    private void SetCameraMovement(Vector2 screenPosition, float smoothTime)
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
                m_DragStartPoint = GetGroundHitPoint(eventData.Position);

                m_WasDragging = false;
                m_IsDragging = true;
                SetCameraMovement(eventData.Position, DragSmoothTime);
                
                break;
            case RecognitionState.InProgress:
                if(m_IsDragging)
                {
                    m_WasDragging = true;
                    m_IsDragging = true;
                    SetCameraMovement(eventData.Position, DragSmoothTime);
                }
                break;
            case RecognitionState.Failed:
            case RecognitionState.Ended:
                m_WasDragging = m_IsDragging;
                m_IsDragging = false;

                Vector2 dir = eventData.DeltaMove.normalized;
                float w = Mathf.Min(MaxCountScreen, eventData.Speed.magnitude / SpeedValueMatchOneScreen) * GetScreenWidth();
                SetCameraMovement(eventData.Position + dir * w, SlideSmoothTime);

#if UNITY_EDITOR
                // debug
                Debug.Log($"=========== w: {w}     ratio: {w / GetScreenWidth()}");
                Vector3 pos = GetGroundHitPoint(eventData.Position + dir * w);
                Debug.DrawLine(pos, pos + Vector3.up * 1000, Color.green, 5);
                Debug.Log($"{pos}   {eventData.Speed.magnitude}");

                // Raycast(eventData.Position + dir * w, TerrainLayer, ref m_HitInfo);
                // Debug.DrawLine(m_HitInfo.point, m_HitInfo.point + Vector3.up * 100, Color.green, 5);
                // if(m_HitInfo.transform == null)
                //     Debug.LogError($"-----  {eventData.Speed.magnitude}");
                // else
                //     Debug.Log($"{m_HitInfo.point}       {eventData.Speed.magnitude}");
#endif            
                break;
        }
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

    // 适配横竖屏，预留接口
    private float GetScreenWidth()
    {
        return Screen.width;
    }

    private float GetScreenHeight()
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





// public class CameraHandler : MonoBehaviour
// {

//     public float PanSpeed = 50f;
//     private static readonly float ZoomSpeedTouch = 0.1f;
//     private static readonly float ZoomSpeedMouse = 0.5f;

//     private static readonly float[] BoundsX = new float[] { -10f, 5f };
//     private static readonly float[] BoundsZ = new float[] { -18f, -4f };
//     private static readonly float[] ZoomBounds = new float[] { 10f, 85f };

//     private Camera cam;

//     private Vector3 lastPanPosition;
//     private int panFingerId; // Touch mode only

//     private bool wasZoomingLastFrame; // Touch mode only
//     private Vector2[] lastZoomPositions; // Touch mode only

//     void Awake()
//     {
//         cam = GetComponent<Camera>();
//     }

//     void Update()
//     {
//         if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
//         {
//             HandleTouch();
//         }
//         else
//         {
//             HandleMouse();
//         }
//     }

//     void HandleTouch()
//     {
//         switch (Input.touchCount)
//         {

//             case 1: // Panning
//                 wasZoomingLastFrame = false;

//                 // If the touch began, capture its position and its finger ID.
//                 // Otherwise, if the finger ID of the touch doesn't match, skip it.
//                 Touch touch = Input.GetTouch(0);
//                 if (touch.phase == TouchPhase.Began)
//                 {
//                     lastPanPosition = touch.position;
//                     panFingerId = touch.fingerId;
//                 }
//                 else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
//                 {
//                     PanCamera(touch.position);
//                 }
//                 break;

//             case 2: // Zooming
//                 Vector2[] newPositions = new Vector2[] { Input.GetTouch(0).position, Input.GetTouch(1).position };
//                 if (!wasZoomingLastFrame)
//                 {
//                     lastZoomPositions = newPositions;
//                     wasZoomingLastFrame = true;
//                 }
//                 else
//                 {
//                     // Zoom based on the distance between the new positions compared to the 
//                     // distance between the previous positions.
//                     float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
//                     float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
//                     float offset = newDistance - oldDistance;

//                     ZoomCamera(offset, ZoomSpeedTouch);

//                     lastZoomPositions = newPositions;
//                 }
//                 break;

//             default:
//                 wasZoomingLastFrame = false;
//                 break;
//         }
//     }

//     void HandleMouse()
//     {
//         // On mouse down, capture it's position.
//         // Otherwise, if the mouse is still down, pan the camera.
//         if (Input.GetMouseButtonDown(0))
//         {
//             lastPanPosition = Input.mousePosition;
//         }
//         else if (Input.GetMouseButton(0))
//         {
//             PanCamera(Input.mousePosition);
//         }

//         // Check for scrolling to zoom the camera
//         float scroll = Input.GetAxis("Mouse ScrollWheel");
//         ZoomCamera(scroll, ZoomSpeedMouse);
//     }

//     void PanCamera(Vector3 newPanPosition)
//     {
//         // Determine how much to move the camera
//         Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
//         Vector3 move = new Vector3(offset.x * PanSpeed, 0, offset.y * PanSpeed);

//         // Perform the movement
//         // transform.Translate(move, Space.World);
//         transform.position += move;

//         // Ensure the camera remains within bounds.
//         // Vector3 pos = transform.position;
//         // pos.x = Mathf.Clamp(transform.position.x, BoundsX[0], BoundsX[1]);
//         // pos.z = Mathf.Clamp(transform.position.z, BoundsZ[0], BoundsZ[1]);
//         // transform.position = pos;

//         // Cache the position
//         lastPanPosition = newPanPosition;
//     }

//     void ZoomCamera(float offset, float speed)
//     {
//         if (offset == 0)
//         {
//             return;
//         }

//         cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - (offset * speed), ZoomBounds[0], ZoomBounds[1]);
//     }
// }