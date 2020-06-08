using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Gesture.Runtime;

namespace Tests
{
    public class TestDragResponser : MonoBehaviour, IScreenDragHandler
    {
        public void OnGestureReady(ScreenDragEventData eventData)
        {
            Debug.Log($"IDragHandler: Ready");
        }

        public void OnGestureStarted(ScreenDragEventData eventData)
        {
            Debug.Log($"IDragHandler: Started");
        }

        public void OnGestureEnded(ScreenDragEventData eventData)
        {
            Debug.Log($"IDragHandler: Ended");
        }

        public void OnGestureProgress(ScreenDragEventData eventData)
        {
            Debug.Log($"IDragHandler: Progress      {eventData.DeltaMove}");
        }

        public void OnGestureFailed(ScreenDragEventData eventData)
        {
            Debug.Log($"IDragHandler: Failed");
        }
    }
}