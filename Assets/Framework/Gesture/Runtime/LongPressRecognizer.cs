using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public class LongPressRecognizer : DiscreteGestureRecognizer<ILongPressHandler, LongPressEventData>, IPointerDownHandler, IPointerUpHandler
    {
        public float Duration = 1.0f;

        public float MoveTolerance = 5.0f;

        public override int requiredPointerCount
        {
            get { return 1; }
            set { throw new System.ArgumentException("not support!"); }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            AddPointer(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            RemovePointer(eventData);
        }

        protected override void OnBegin()
        {
            m_EventData.StartTime = Time.time;
            m_EventData.PressPosition = m_EventData.GetAveragePressPosition(requiredPointerCount);
        }

        protected override RecognitionState OnProgress()
        {
            if(m_EventData.pointerCount != requiredPointerCount)
                return RecognitionState.Failed;

            if(m_EventData.ElapsedTime > Duration)
                return RecognitionState.Ended;

            if(m_EventData.GetAverageDistanceFromPress(requiredPointerCount) > MoveTolerance)
                return RecognitionState.Failed;

            return RecognitionState.InProgress;
        }

        protected override void ExecuteGestureReady()
        {
            GestureEvents.Execute<ILongPressHandler, LongPressEventData>(gameObject, m_EventData, GestureEvents.ExecuteReady);
        }
        protected override void ExecuteGestureRecognized()
        {
            GestureEvents.Execute<ILongPressHandler, LongPressEventData>(gameObject, m_EventData, GestureEvents.ExecuteRecognized);
        }
        protected override void ExecuteGestureFailed()
        {
            GestureEvents.Execute<ILongPressHandler, LongPressEventData>(gameObject, m_EventData, GestureEvents.ExecuteFailed);
        }
    }    
    
    public class LongPressEventData : GestureEventData
    {
    }

    public interface ILongPressHandler : IDiscreteGestureHandler<LongPressEventData>
    {
    }
}