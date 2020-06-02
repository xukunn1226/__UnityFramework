using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public class LongPressRecognizer : GestureRecognizer<LongPressEventData>, IPointerDownHandler, IPointerUpHandler
    {
        public float Duration = 1.0f;

        public float MoveTolerance = 5.0f;

        private LongPressEventData m_EventData;

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log($"-------OnPointerDown");

            if(m_EventData == null)
            {
                m_EventData = new LongPressEventData(eventData);
            }


        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log($"OnPointerUp------------");

            m_EventData = null;
        }

        private void Update()
        {
            if(m_EventData == null)
                return;

            switch(m_EventData.State)
            {

            }
        }
    }

    public class LongPressEventData : GestureEventData
    {
        public LongPressEventData(PointerEventData pointerEventData)
        : base(pointerEventData)
        {}

        public LongPressEventData()
        {}
    }
}