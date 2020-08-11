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

    public Camera           eventCamera;
    public Collider         terrain;
    private PlayerInput     m_PlayerInput;
    private PlayerInput     playerInput
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

    private int m_TerrainLayer;
    private int m_BaseLayer;

    public ScreenDragEventData dragData { get; private set; }
    public bool isDragging { get; private set; }
    public bool wasDragging { get; private set; }

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
        
        m_TerrainLayer = LayerMask.NameToLayer("Terrain");
        m_BaseLayer = LayerMask.NameToLayer("Base");
    }

    void OnDestroy()
    {
        Instance = null;
    }

    private void PickGameObject(Vector2 screenPosition)
    {
        RaycastHit hitInfo = Raycast(screenPosition, 1 << m_TerrainLayer | 1 << m_BaseLayer);
     
        if (hitInfo.transform != null)
        {
            playerInput.hitEventData.hitInfo = hitInfo;
            playerInput.SetSelectedGameObject(hitInfo.transform.gameObject, playerInput.hitEventData);
        }
    }

    private RaycastHit Raycast(Vector2 screenPosition, int layerMask)
    {
        Ray ray = eventCamera.ScreenPointToRay(screenPosition);

        RaycastHit hitInfo = new RaycastHit();
        Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask);
        return hitInfo;
    }

    public void OnGesture(ScreenDragEventData eventData)
    {
        Debug.Log($"Drag.........{eventData.State}   {eventData.Position}    {eventData.DeltaMove}   {Time.frameCount}");

        dragData = eventData;
        switch (eventData.State)
        {
            case RecognitionState.Started:
                wasDragging = false;
                isDragging = true;
                break;
            case RecognitionState.InProgress:
                wasDragging = isDragging;
                isDragging = true;
                break;
            case RecognitionState.Failed:
            case RecognitionState.Ended:
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
