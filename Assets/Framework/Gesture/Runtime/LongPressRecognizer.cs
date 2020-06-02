using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public class LongPressRecognizer : GestureRecognizer, IPointerDownHandler, IPointerUpHandler
    {
        public float Duration = 1.0f;

        public float MoveTolerance = 5.0f;

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log($"-------OnPointerDown");
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log($"OnPointerUp------------");
        }

        private void Update()
        {

        }
    }

    public class LongPressEventData : GestureEventData
    {
        public LongPressEventData(GestureSystem gestureSystem, PointerEventData pointerEventData)
        : base(gestureSystem, pointerEventData)
        {}
    }
}