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
    public Camera eventCamera;
    private PlayerInput m_PlayerInput;
    private PlayerInput playerInput
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

    void Awake()
    {
        if(eventCamera == null)
            throw new System.Exception("Missing event camera");
        if(playerInput == null)
            throw new System.ArgumentNullException("playerInput");

        m_TerrainLayer = LayerMask.NameToLayer("Terrain");
        m_BaseLayer = LayerMask.NameToLayer("Base");
    }

    private void PickGameObject(Vector2 screenPosition)
    {
        Ray ray = eventCamera.ScreenPointToRay(screenPosition);

        RaycastHit hitInfo = new RaycastHit();
        Physics.Raycast(ray, out hitInfo, Mathf.Infinity, 1 << m_TerrainLayer | 1 << m_BaseLayer);

        if (hitInfo.transform != null)
        {
            playerInput.hitEventData.hitInfo = hitInfo;
            playerInput.SetSelectedGameObject(hitInfo.transform.gameObject, playerInput.hitEventData);
        }
    }

    public void OnGesture(ScreenDragEventData eventData)
    {
        // Debug.Log($"Drag.........{eventData.State}   {eventData.Position}    {eventData.DeltaMove}   {Time.frameCount}");

        // screenDragData = eventData;
        switch (eventData.State)
        {
            case RecognitionState.Started:
                // isScreenDragging = true;
                break;
            case RecognitionState.Failed:
            case RecognitionState.Ended:
                // isScreenDragging = false;
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
