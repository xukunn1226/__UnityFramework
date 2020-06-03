using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public abstract class GestureRecognizer<T> : MonoBehaviour, IPointerDownHandler, IPointerUpHandler where T : GestureEventData, new()
    {
        public delegate void GestureEventHandler(T eventData);
        public event GestureEventHandler OnGesture;

        public int RequiredPointerCount = 1;

        protected T m_EventData;

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log($"-------OnPointerDown");

            T data = GetGestureEventData();
            data.AddPointerData(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log($"OnPointerUp------------");

            // m_EventData = null;
        }

        protected T GetGestureEventData()
        {
            if(m_EventData == null)
            {
                m_EventData = new T();
            }
            return m_EventData;
        }
    }
}