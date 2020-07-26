using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Gesture.Runtime;

public class PlayerInput : MonoBehaviour, IScreenDragHandler, IPinchHandler
{
    static public PlayerInput Instance { get; private set; }

    public bool isScreenDragging { get; private set; }
    public ScreenDragEventData screenDragData { get; private set; }

    void Awake()
    {
        // 已有PlayerInput，则自毁
        if (FindObjectsOfType<PlayerInput>().Length > 1)
        {
            DestroyImmediate(this);
            throw new System.Exception("PlayerInput has already exist...");
        }

        Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }

    public void OnGesture(ScreenDragEventData eventData)
    {
        // Debug.Log($"{eventData.State}   {eventData.Position}    {eventData.DeltaMove}   {eventData.Speed}");

        screenDragData = eventData;
        switch(eventData.State)
        {
            case RecognitionState.Started:
                isScreenDragging = true;
                break;
            case RecognitionState.Failed:
            case RecognitionState.Ended:
                isScreenDragging = false;
                break;
        } 
    }

    public void OnGesture(PinchEventData eventData)
    {
        Debug.Log($"{eventData.State}   {eventData.Position}    {eventData.DeltaMove}");
    }
}
