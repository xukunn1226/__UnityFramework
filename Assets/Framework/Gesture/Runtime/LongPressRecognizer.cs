using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Gesture.Runtime
{
    [RequireComponent(typeof(PlayerInput))]
    public class LongPressRecognizer : DiscreteGestureRecognizer<ILongPressHandler, ScreenLongPressEventData>
    {
        public float Duration = 1.0f;

        public float MoveTolerance = 5.0f;

        protected override bool CanBegin()
        {
            return InputModule.screenPointerData.Count > 0;
        }

        protected override RecognitionState OnProgress()
        {
            if(InputModule.screenPointerData.Count == 0)
                return RecognitionState.Failed;

            foreach(var data in InputModule.screenPointerData)
            {
                if(data.Value.usedBy != UsedBy.None)
                    return RecognitionState.Failed;         // 触发了其他手势，则返回失败

                if(Vector2.Distance(data.Value.pointerEventData.pressPosition, data.Value.pointerEventData.position) > MoveTolerance)
                    continue;

                if(Time.time - data.Value.startTime > Duration)
                {
                    data.Value.usedBy = UsedBy.LongPress;
                    m_EventData.screenPosition = data.Value.pointerEventData.position;
                    // GestureEvents.ExecuteDiscrete<ILongPressHandler, ScreenLongPressEventData>(gameObject, m_EventData);
                    return RecognitionState.Ended;
                }
            }

            return RecognitionState.InProgress;
        }
    }

    public class ScreenLongPressEventData : GestureEventData
    {
        public Vector2 screenPosition;
    }

    public interface ILongPressHandler : IDiscreteGestureHandler<ScreenLongPressEventData>
    {
    }    
}