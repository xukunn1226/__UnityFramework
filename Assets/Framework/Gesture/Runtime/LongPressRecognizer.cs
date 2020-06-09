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

            if(m_EventData.ElapsedTime > Duration)
            {
                // PointerEventData data;
                // m_EventData.PointerEventData.TryGetValue(-1, out data);
                // Debug.Log($"-------{data.used}");
                m_EventData.SetEventDataUsed(requiredPointerCount);         // 设置消息被使用的标志
                // Debug.LogWarning($"{data.used}-------");
                return RecognitionState.Ended;
            }

            if(m_EventData.GetAverageDistanceFromPress(requiredPointerCount) > MoveTolerance)
                return RecognitionState.Failed;

            return RecognitionState.InProgress;
        }

        protected override void ExecuteGestureReady()
        {
            Debug.Log("LongPressRecognizer:     ---- Ready");
            GestureEvents.Execute<ILongPressHandler, LongPressEventData>(gameObject, m_EventData, GestureEvents.ExecuteReady);
        }
        protected override void ExecuteGestureRecognized()
        {
            Debug.Log("LongPressRecognizer:     ---- Recognized");
            GestureEvents.Execute<ILongPressHandler, LongPressEventData>(gameObject, m_EventData, GestureEvents.ExecuteRecognized);
        }
        protected override void ExecuteGestureFailed()
        {
            Debug.Log("LongPressRecognizer:     ---- Failed");
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