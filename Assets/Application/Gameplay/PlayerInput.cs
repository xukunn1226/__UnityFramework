using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Gesture.Runtime;

public class PlayerInput : MonoBehaviour, IScreenDragHandler, IPinchHandler
{
    public void OnGesture(ScreenDragEventData eventData)
    {
        Debug.Log($"{eventData.State}   {eventData.Position}    {eventData.DeltaMove}   {eventData.Speed}");
    }

    public void OnGesture(PinchEventData eventData)
    {
        Debug.Log($"{eventData.State}   {eventData.Position}    {eventData.Delta}");
    }
}
