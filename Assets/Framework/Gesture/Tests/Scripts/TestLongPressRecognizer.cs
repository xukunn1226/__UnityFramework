using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Gesture.Runtime;

namespace Tests
{
    public class TestLongPressRecognizer : MonoBehaviour, ILongPressHandler
    {
        public void OnGestureReady(LongPressEventData eventData)
        {
            Debug.Log($"OnGestureReady");
        }

        public void OnGestureRecognized(LongPressEventData eventData)
        {
            Debug.Log($"OnGestureRecognized");
        }

        public void OnGestureFailed(LongPressEventData eventData)
        {
            Debug.Log($"OnGestureFailed");
        }
    }
}