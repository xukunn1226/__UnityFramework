using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Gesture.Runtime
{
    public abstract class DiscreteGestureRecognizer<T> : GestureRecognizer<T> where T : GestureEventData, new()
    {
        // life cycle: Ready -> InProgress -> Failed/Ended
        private void Update()
        {
            switch(State)
            {
                case RecognitionState.Ready:
                    if(CanBegin())
                    {
                        OnBegin();
                        State = RecognitionState.InProgress;
                    }
                    break;
                case RecognitionState.InProgress:
                    State = OnProgress();
                    break;
                case RecognitionState.Failed:
                case RecognitionState.Ended:
                    if(m_EventData.pointerCount == 0)
                    {
                        State = RecognitionState.Ready;
                    }
                    break;
            }
        }
    }
}