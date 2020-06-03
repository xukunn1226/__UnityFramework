using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public class LongPressRecognizer : GestureRecognizer<LongPressEventData>
    {
        public float Duration = 1.0f;

        public float MoveTolerance = 5.0f;

        private void Update()
        {
        }
    }

    public class LongPressEventData : GestureEventData
    {
        // public LongPressEventData(PointerEventData pointerEventData)
        // : base(pointerEventData)
        // {}

        public LongPressEventData()
        {}
    }
}