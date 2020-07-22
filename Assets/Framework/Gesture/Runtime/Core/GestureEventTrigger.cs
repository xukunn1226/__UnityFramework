using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace Framework.Gesture.Runtime
{
    public class GestureEventTrigger : MonoBehaviour, ILongPressHandler, IPinchHandler, IScreenDragHandler
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
            public GestureEventTriggerType eventID = GestureEventTriggerType.LongPressRecognized;

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

        public void OnGestureRecognized(LongPressEventData eventData)
        {
            Execute(GestureEventTriggerType.LongPressRecognized, eventData);
        }

        public void OnGestureStarted(PinchEventData eventData)
        {
            Execute(GestureEventTriggerType.PinchStarted, eventData);
        }

        public void OnGestureProgress(PinchEventData eventData)
        {
            Execute(GestureEventTriggerType.PinchInProgress, eventData);
        }

        public void OnGestureEnded(PinchEventData eventData)
        {
            Execute(GestureEventTriggerType.PinchEnded, eventData);
        }

        public void OnGestureFailed(PinchEventData eventData)
        {
            Execute(GestureEventTriggerType.PinchFailed, eventData);
        }

        public void OnGestureStarted(ScreenDragEventData eventData)
        {
            Execute(GestureEventTriggerType.ScreenDragStarted, eventData);
        }

        public void OnGestureProgress(ScreenDragEventData eventData)
        {
            Execute(GestureEventTriggerType.ScreenDragInProgress, eventData);
        }

        public void OnGestureEnded(ScreenDragEventData eventData)
        {
            Execute(GestureEventTriggerType.ScreenDragEnded, eventData);
        }

        public void OnGestureFailed(ScreenDragEventData eventData)
        {
            Execute(GestureEventTriggerType.ScreenDragFailed, eventData);
        }
    }
}