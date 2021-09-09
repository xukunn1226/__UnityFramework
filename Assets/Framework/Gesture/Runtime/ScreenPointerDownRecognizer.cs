using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    [RequireComponent(typeof(PlayerInput))]
    public class ScreenPointerDownRecognizer : DiscreteGestureRecognizer<IScreenPointerDownHandler, ScreenPointerDownEventData>
    {
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
                // if(data.Value.usedBy != UsedBy.None)
                //     return RecognitionState.Failed;             // 触发了其他手势，则返回失败

                if(data.Value.bPressedThisFrame)
                {
                    m_EventData.screenPosition = data.Value.pointerEventData.position;
                    // GestureEvents.ExecuteDiscrete<IScreenPointerDownHandler, ScreenPointerDownEventData>(gameObject, m_EventData);
                    return RecognitionState.Ended;
                }
            }

            return RecognitionState.InProgress;
        }
    }
    
    public class ScreenPointerDownEventData : GestureEventData
    {
        public Vector2 screenPosition;
    }
    public interface IScreenPointerDownHandler : IDiscreteGestureHandler<ScreenPointerDownEventData>
    {
    }
}