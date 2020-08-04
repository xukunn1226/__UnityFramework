using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Gesture.Runtime
{
    [RequireComponent(typeof(PlayerInput))]
    public class ScreenPointerUpRecognizer : DiscreteGestureRecognizer<IScreenPointerUpHandler, ScreenPointerUpEventData>
    {
        protected override bool CanBegin()
        {
            return true;
        }

        protected override RecognitionState OnProgress()
        {
            if(InputModule == null)
                return RecognitionState.InProgress;

            foreach(var data in InputModule.screenPointerData)
            {
                if(data.Value.usedBy != UsedBy.None)
                    continue;

                if(data.Value.bReleasedThisFrame)
                {
                    m_EventData.screenPosition = data.Value.pointerEventData.position;
                    GestureEvents.ExecuteDiscrete<IScreenPointerUpHandler, ScreenPointerUpEventData>(gameObject, m_EventData);
                }
            }

            return RecognitionState.InProgress;
        }
    }
    
    public class ScreenPointerUpEventData : GestureEventData
    {
        public Vector2 screenPosition;
    }

    public interface IScreenPointerUpHandler : IDiscreteGestureHandler<ScreenPointerUpEventData>
    {
    }
}