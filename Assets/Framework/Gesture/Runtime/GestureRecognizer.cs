using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public abstract class GestureRecognizer<T> : MonoBehaviour where T : GestureEventData, new()
    {
        public delegate void GestureEventHandler(T eventData);
        public event GestureEventHandler OnGesture;

        public bool ContinuousRecognizeWhenFailed;          // whether or not to recognize gesture when failed

        public int RequiredPointerCount = 1;

        protected T m_EventData = new T();
 
        protected void AddPointer(PointerEventData eventData)
        {
            Debug.Log($"-------OnPointerDown");

            m_EventData.AddPointerData(eventData);
        }

        protected void RemovePointer(PointerEventData eventData)
        {
            Debug.Log($"OnPointerUp------------");

            m_EventData.RemovePointerData(eventData);
        }

        protected virtual bool CanBegin()
        {
            return m_EventData.pointerCount == RequiredPointerCount;
        }

        private void Update()
        {

        }
    }
}