using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Gesture.Runtime;
using UnityEngine.EventSystems;

namespace Tests
{
    public class TestLongPressResponser : MonoBehaviour, ILongPressHandler, ISelectHandler, IDeselectHandler
    {
        public void OnGesture(LongPressEventData eventData)
        {}
        
        public void OnSelect(BaseEventData eventData)
        {
            transform.localScale = transform.localScale * 2;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            transform.localScale *= 0.5f;
        }
    }
}