using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Gesture.Runtime;

namespace Tests
{
    public class TestGestureReceiver : MonoBehaviour
    {
        public void OnGestureReady(GestureEventData eventData)
        {
            Debug.Log($"-------OnGestureReady");
        }

        public void OnGestureRecognized(GestureEventData eventData)
        {
            Debug.Log($"---------OnGestureRecognized");
        }

        public void OnGestureFailed(GestureEventData eventData)
        {
            Debug.Log($"-----------OnGestureFailed");
        }
    }
}