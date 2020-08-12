using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Gesture.Runtime;

[RequireComponent(typeof(PlayerInput))]
public class GamePlayerInput :  MonoBehaviour,
                                IScreenDragHandler,
                                IScreenPinchHandler,
                                ILongPressHandler,
                                IScreenPointerDownHandler,
                                IScreenPointerUpHandler
{
    static public GamePlayerInput Instance { get; private set; }

    public Camera               eventCamera;
    private PlayerInput         m_PlayerInput;
    private PlayerInput         playerInput
    {
        get
        {
            if(m_PlayerInput == null)
            {
                m_PlayerInput = GetComponent<PlayerInput>();
            }
            return m_PlayerInput;
        }
    }

    private RaycastHit m_HitInfo = new RaycastHit();
    private ref RaycastHit m_HitInfoRef => ref m_HitInfo;

    public LayerMask            BaseLayer;
    public LayerMask            TerrainLayer;

    public ScreenDragEventData  dragData        { get; private set; }
    public bool                 isDragging      { get; private set; }
    public bool                 wasDragging     { get; private set; }
    public Vector3              dragStartPoint  { get; private set; }
    public Vector3              dragEndPoint    { get; private set; }


    void Awake()
    {
        if (FindObjectsOfType<GamePlayerInput>().Length > 1)
        {
            DestroyImmediate(this);
            throw new System.Exception("GamePlayerInput has already exist...");
        }

        if(eventCamera == null)
            throw new System.Exception("Missing event camera");
        if(playerInput == null)
            throw new System.ArgumentNullException("playerInput");

        Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }

    private void PickGameObject(Vector2 screenPosition)
    {
        m_HitInfoRef = Raycast(screenPosition, TerrainLayer | BaseLayer);
     
        if (m_HitInfoRef.transform != null)
        {
            playerInput.hitEventData.hitInfo = m_HitInfoRef;
            playerInput.SetSelectedGameObject(m_HitInfoRef.transform.gameObject, playerInput.hitEventData);
        }
    }

    private ref RaycastHit Raycast(Vector2 screenPosition, int layerMask)
    {
        Ray ray = eventCamera.ScreenPointToRay(screenPosition);

        Physics.Raycast(ray, out m_HitInfoRef, Mathf.Infinity, layerMask);
        return ref m_HitInfoRef;
    }

    public void OnGesture(ScreenDragEventData eventData)
    {
        // Debug.Log($"Drag.........{eventData.State}   {eventData.Position}    {eventData.DeltaMove}   {Time.frameCount}");

        dragData = eventData;
        switch (eventData.State)
        {
            case RecognitionState.Started:
                Raycast(dragData.Position, TerrainLayer);
                wasDragging = false;
                isDragging = m_HitInfo.transform != null;
                dragStartPoint = m_HitInfo.point;
                break;
            case RecognitionState.InProgress:
                if(isDragging)
                { // 起始在Terrain
                    Raycast(dragData.Position, TerrainLayer);
                    if(m_HitInfo.transform != null)
                    {
                        dragEndPoint = m_HitInfo.point;
                    }
                    Debug.LogWarning($"{dragStartPoint}   {dragEndPoint}");
                    wasDragging = true;
                    isDragging = true;
                    // Debug.DrawLine(eventCamera.transform.position, dragEndPoint, Color.red);
                }
                break;
            case RecognitionState.Failed:
            case RecognitionState.Ended:
                Raycast(dragData.Position, TerrainLayer);
                if(m_HitInfo.transform != null)
                {
                    dragEndPoint = m_HitInfo.point;
                }
                wasDragging = isDragging;
                isDragging = false;
                break;
        }
    }

    public void OnGesture(ScreenPinchEventData eventData)
    {
        Debug.Log($"Pinch..........{eventData.State}   {eventData.Position}    {eventData.DeltaMove}    {Time.frameCount}");

        // screenPinchData = eventData;
        switch (eventData.State)
        {
            case RecognitionState.Started:
                // isScreenPinching = true;
                break;
            case RecognitionState.Failed:
            case RecognitionState.Ended:
                // isScreenPinching = false;
                break;
        }
    }

    public void OnGesture(ScreenLongPressEventData eventData)
    {
        Debug.Log($"LongPress..........{eventData.State}   {eventData.screenPosition}    {Time.frameCount}");
        PickGameObject(eventData.screenPosition);
    }

    public void OnGesture(ScreenPointerDownEventData eventData)
    {
        Debug.Log($"ScreenPointerDownEventData:       {Time.frameCount}");
    }
    public void OnGesture(ScreenPointerUpEventData eventData)
    {
        Debug.Log($"ScreenPointerUpEventData:       {Time.frameCount}");
        PickGameObject(eventData.screenPosition);
    }
}
