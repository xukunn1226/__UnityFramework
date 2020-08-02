using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace Framework.Gesture.Runtime
{
    public class GestureEventTrigger :  MonoBehaviour, 
                                        ILongPressHandler, 
                                        IScreenPinchHandler,
                                        IScreenDragHandler,
                                        IScreenPointerDownHandler,
                                        IScreenPointerUpHandler
    {
        [Serializable]
        public class GestureTriggerEvent : UnityEvent<GestureEventData>
        {}

        [Serializable]
        public class Entry
        {
            /// <summary>
            /// What type of event is the associated callback listening for.
            /// </summary>
            public GestureEventTriggerType eventID = GestureEventTriggerType.ScreenLongPress;

            /// <summary>
            /// The desired TriggerEvent to be Invoked.
            /// </summary>
            public GestureTriggerEvent callback = new GestureTriggerEvent();
        }

        [SerializeField]
        private List<Entry> m_Delegates;

        protected GestureEventTrigger()
        {}

        public List<Entry> triggers
        {
            get
            {
                if (m_Delegates == null)
                    m_Delegates = new List<Entry>();
                return m_Delegates;
            }
            set { m_Delegates = value; }
        }

        private void Execute(GestureEventTriggerType id, GestureEventData eventData)
        {
            for (int i = 0, imax = triggers.Count; i < imax; ++i)
            {
                var ent = triggers[i];
                if (ent.eventID == id && ent.callback != null)
                    ent.callback.Invoke(eventData);
            }
        }

        public void OnGesture(ScreenLongPressEventData eventData)
        {
            Execute(GestureEventTriggerType.ScreenLongPress, eventData);
        }

        public void OnGesture(ScreenDragEventData eventData)
        {
            Execute(GestureEventTriggerType.ScreenDrag, eventData);
        }

        public void OnGesture(ScreenPinchEventData eventData)
        {
            Execute(GestureEventTriggerType.ScreenPinch, eventData);
        }

        public void OnGesture(ScreenPointerDownEventData eventData)
        {
            Execute(GestureEventTriggerType.ScreenPointerDown, eventData);
        }
        public void OnGesture(ScreenPointerUpEventData eventData)
        {
            Execute(GestureEventTriggerType.ScreenPointerUp, eventData);
        }
    }
}