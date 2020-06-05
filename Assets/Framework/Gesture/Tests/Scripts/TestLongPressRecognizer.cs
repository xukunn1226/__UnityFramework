using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Gesture.Runtime;

namespace Tests
{
    public class TestLongPressRecognizer : MonoBehaviour, ILongPressHandler
    {
        public void OnGestureReady(LongPressEventData eventData)
        {}

        public void OnGestureRecognized(LongPressEventData eventData)
        {}

        public void OnGestureFailed(LongPressEventData eventData)
        {}
    }
}