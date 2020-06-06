﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public class LongPressRecognizer : DiscreteGestureRecognizer<ILongPressHandler, LongPressEventData>, IPointerDownHandler, IPointerUpHandler
    {
        public float Duration = 1.0f;

        public float MoveTolerance = 5.0f;

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
            m_EventData.PressPosition = m_EventData.GetAveragePressPosition(RequiredPointerCount);
        }

        protected override RecognitionState OnProgress()
        {
            if(m_EventData.pointerCount != RequiredPointerCount)
                return RecognitionState.Failed;

            if(m_EventData.ElapsedTime > Duration)
                return RecognitionState.Ended;

            if(m_EventData.GetAverageDistanceFromPress(RequiredPointerCount) > MoveTolerance)
                return RecognitionState.Failed;

            return RecognitionState.InProgress;
        }

        public static readonly GestureEvents.DiscreteEventFunction<LongPressEventData> s_GestureHandler_Ready = GestureEvents.ExecuteReady;
        public static readonly GestureEvents.DiscreteEventFunction<LongPressEventData> s_GestureHandler_Recognized = GestureEvents.ExecuteRecognized;
        public static readonly GestureEvents.DiscreteEventFunction<LongPressEventData> s_GestureHandler_Failed = GestureEvents.ExecuteFailed;
        
        protected override void ExecuteGestureReady()
        {
            GestureEvents.Execute<ILongPressHandler, LongPressEventData>(gameObject, m_EventData, s_GestureHandler_Ready);
        }
        protected override void ExecuteGestureRecognized()
        {
            GestureEvents.Execute<ILongPressHandler, LongPressEventData>(gameObject, m_EventData, s_GestureHandler_Recognized);
        }
        protected override void ExecuteGestureFailed()
        {
            GestureEvents.Execute<ILongPressHandler, LongPressEventData>(gameObject, m_EventData, s_GestureHandler_Failed);
        }
    }    
    
    public class LongPressEventData : GestureEventData
    {
    }

    public interface ILongPressHandler : IDiscreteGestureHandler<LongPressEventData>
    {
    }
}