using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Gesture.Runtime
{
    public abstract class ContinuousGestureRecognizer<T, K> : GestureRecognizer<K> where K : GestureEventData, new() where T : IDiscreteGestureHandler<K>
    {
        public bool ContinuousRecognizeWhenFailed = true;          // whether or not to recognize gesture when failed

        private void Update()
        {
            switch(State)
            {
                case RecognitionState.Ready:
                    if(CanBegin())
                    {
                        OnBegin();
                        State = RecognitionState.Started;
                    }
                    break;
                case RecognitionState.Started:
                    State = RecognitionState.InProgress;
                    break;
                case RecognitionState.InProgress:
                    State = OnProgress();
                    break;
                case RecognitionState.Failed:
                case RecognitionState.Ended:
                    if(ContinuousRecognizeWhenFailed || m_EventData.pointerCount == 0)
                        State = RecognitionState.Ready;                    
                    break;
            }
        }
    }
}