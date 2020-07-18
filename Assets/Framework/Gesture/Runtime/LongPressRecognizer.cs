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

        protected override RecognitionState OnProgress()
        {
            if(m_EventData.pointerCount != requiredPointerCount)
                return RecognitionState.Failed;

            if(m_EventData.GetAverageDistanceFromPress(requiredPointerCount) > MoveTolerance)
                return RecognitionState.Failed;
                
            if(m_EventData.ElapsedTime > Duration)
                return RecognitionState.Ended;

            return RecognitionState.InProgress;
        }

        protected override void ExecuteGestureReady()
        {
            Debug.Log("LongPressRecognizer:     ---- Ready");
            GestureEvents.ExecuteReady_Discrete<ILongPressHandler, LongPressEventData>(gameObject, m_EventData);
        }
        protected override void ExecuteGestureRecognized()
        {
            Debug.Log("LongPressRecognizer:     ---- Recognized");
            GestureEvents.ExecuteReady_Discrete<ILongPressHandler, LongPressEventData>(gameObject, m_EventData);
        }
        protected override void ExecuteGestureFailed()
        {
            Debug.Log("LongPressRecognizer:     ---- Failed");
            GestureEvents.ExecuteFailed_Discrete<ILongPressHandler, LongPressEventData>(gameObject, m_EventData);
        }
    }    
    
    public class LongPressEventData : GestureEventData
    {
    }

    public interface ILongPressHandler : IDiscreteGestureHandler<LongPressEventData>
    {
    }
}