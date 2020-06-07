using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public class PinchRecognizer : ContinuousGestureRecognizer<IPinchHandler, PinchEventData>, IPointerDownHandler, IPointerUpHandler
    {
        public float MinDOT = -0.7f;

        public float DeltaScale = 1.0f;

        public override int requiredPointerCount
        {
            get { return 2; }
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

        protected override bool CanBegin()
        {
            if(m_EventData.pointerCount < requiredPointerCount)
                return false;

            return true;
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

            // if(m_EventData.ElapsedTime > Duration)
            //     return RecognitionState.Ended;

            // if(m_EventData.GetAverageDistanceFromPress(RequiredPointerCount) > MoveTolerance)
            //     return RecognitionState.Failed;

            return RecognitionState.InProgress;
        }

        protected override void ExecuteGestureReady()
        {
            GestureEvents.Execute<IPinchHandler, PinchEventData>(gameObject, m_EventData, GestureEvents.ExecuteReady);
        }
        protected override void ExecuteGestureStarted()
        {
            GestureEvents.Execute<IPinchHandler, PinchEventData>(gameObject, m_EventData, GestureEvents.ExecuteStarted);
        }
        protected override void ExecuteGestureInProgress()
        {
            GestureEvents.Execute<IPinchHandler, PinchEventData>(gameObject, m_EventData, GestureEvents.ExecuteProgress);
        }
        protected override void ExecuteGestureEnded()
        {
            GestureEvents.Execute<IPinchHandler, PinchEventData>(gameObject, m_EventData, GestureEvents.ExecuteEnded);
        }
        protected override void ExecuteGestureFailed()
        {
            GestureEvents.Execute<IPinchHandler, PinchEventData>(gameObject, m_EventData, GestureEvents.ExecuteFailed);
        }
    }
    
    public class PinchEventData : GestureEventData
    {
    }

    public interface IPinchHandler : IContinuousGestureHandler<PinchEventData>
    {
    }
}