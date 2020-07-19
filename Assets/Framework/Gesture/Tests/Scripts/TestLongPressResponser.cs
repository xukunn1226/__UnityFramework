using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Gesture.Runtime;
using UnityEngine.EventSystems;

namespace Tests
{
    public class TestLongPressResponser : MonoBehaviour, ILongPressHandler, ISelectHandler, IDeselectHandler
    {
        public void OnGestureReady(LongPressEventData eventData)
        {
            Debug.Log($"OnGestureReady: {gameObject.name}");
        }

        public void OnGestureRecognized(LongPressEventData eventData)
        {
            Debug.Log($"OnGestureRecognized: {gameObject.name}");
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnGestureFailed(LongPressEventData eventData)
        {
            Debug.Log($"OnGestureFailed: {gameObject.name}");
        }

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