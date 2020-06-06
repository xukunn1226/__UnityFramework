using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Gesture.Runtime
{
    public class PinchRecognizer : ContinuousGestureRecognizer<IPinchHandler, PinchEventData>//, IPointerDownHandler, IPointerUpHandler
    {        
        protected override void OnBegin()
        {
            m_EventData.StartTime = Time.time;
            m_EventData.PressPosition = m_EventData.GetAveragePressPosition(RequiredPointerCount);
        }

        protected override RecognitionState OnProgress()
        {
            if(m_EventData.pointerCount != RequiredPointerCount)
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
            GestureEvents.Execute<IPinchHandler, PinchEventData>(gameObject, m_EventData, GestureEvents.ExecuteRecognized);
        }
        protected override void ExecuteGestureInProgress()
        {
            GestureEvents.Execute<IPinchHandler, PinchEventData>(gameObject, m_EventData, GestureEvents.ExecuteRecognized);
        }
        protected override void ExecuteGestureEnded()
        {
            GestureEvents.Execute<IPinchHandler, PinchEventData>(gameObject, m_EventData, GestureEvents.ExecuteRecognized);
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