using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Framework.Gesture.Runtime;

public class TestGesture : MonoBehaviour, ISelectHandler, IDeselectHandler, IObjectDragHandler
{
    public void OnSelect(BaseEventData eventData)
    {
        transform.localScale *= 2;

        // PlayerInput.HitEventData data = eventData as PlayerInput.HitEventData;
        // if(data != null)
        // {
        //     Debug.Log($"HitInfo: {data.hitInfo.point}");
        // }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        transform.localScale *= 0.5f;
        Debug.Log($"OnDeselect: {Time.frameCount}");
    }

    // public void OnBeginDrag(PointerEventData eventData)
    // {
    //     Debug.Log("OnBeginDrag");
    //     SetDraggedPosition(eventData);
    // }

    // public void OnDrag(PointerEventData eventData)
    // {
    //     SetDraggedPosition(eventData);
    // }

    // public void OnEndDrag(PointerEventData eventData)
    // {
    //     Debug.Log($"OnEndDrag: {Time.frameCount}");
    // }

    public void OnGesture(ObjectDragEventData eventData)
    {

    }

    private void SetDraggedPosition(PointerEventData eventData)
    {
        Vector3 newPos = GamePlayerCamera.Instance.GetGroundHitPoint(eventData.position);
        transform.position = newPos;
    }
}
