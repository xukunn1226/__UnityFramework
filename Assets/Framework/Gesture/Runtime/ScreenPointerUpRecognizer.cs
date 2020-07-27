using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Gesture.Runtime
{
    public class ScreenPointerUpRecognizer : DiscreteGestureRecognizer<IScreenPointerUpHandler, ScreenPointerUpEventData>
    {
        protected override void RaiseEvent(GestureEventData eventData)
        {}

        protected override RecognitionState OnProgress()
        {
            return RecognitionState.Ended;
        }
    }

    public class ScreenPointerUpEventData : GestureEventData
    {}

    public interface IScreenPointerUpHandler : IDiscreteGestureHandler<ScreenPointerUpEventData>
    {}
}