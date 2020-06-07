using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Gesture.Runtime;

namespace Tests
{
    public class TestDragResponser : MonoBehaviour, IDragHandler
    {
        public void OnGestureReady(DragEventData eventData)
        {
            Debug.Log($"IDragHandler: Ready");
        }

        public void OnGestureStarted(DragEventData eventData)
        {
            Debug.Log($"IDragHandler: Started");
        }

        public void OnGestureEnded(DragEventData eventData)
        {
            Debug.Log($"IDragHandler: Ended");
        }

        public void OnGestureProgress(DragEventData eventData)
        {
            Debug.Log($"IDragHandler: Progress      {eventData.DeltaMove}");
        }

        public void OnGestureFailed(DragEventData eventData)
        {
            Debug.Log($"IDragHandler: Failed");
        }
    }
}