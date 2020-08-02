using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public class ScreenPointerDownRecognizer : DiscreteGestureRecognizer<IScreenPointerDownHandler, ScreenPointerDownEventData>
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

                if(data.Value.bPressedThisFrame)
                {
                    m_EventData.screenPosition = data.Value.pointerEventData.position;
                    GestureEvents.ExecuteDiscrete<IScreenPointerDownHandler, ScreenPointerDownEventData>(gameObject, m_EventData);
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