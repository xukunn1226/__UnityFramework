using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Gesture.Runtime
{
    public abstract class ContinuousGestureRecognizer<T, K> : GestureRecognizer<K> where K : GestureEventData, new() where T : IContinuousGestureHandler<K>
    {
        public bool ContinuousRecognizeWhenFailed = true;          // whether or not to recognize gesture when failed

        internal override void InternalUpdate()
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
                    OnProgress();
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
        
        protected override void RaiseEvent()
        {
            switch(State)
            {
                case RecognitionState.Ready:
                    ExecuteGestureReady();
                    break;
                case RecognitionState.Started:
                    ExecuteGestureStarted();
                    break;
                case RecognitionState.InProgress:
                    ExecuteGestureInProgress();
                    break;
                case RecognitionState.Ended:
                    ExecuteGestureEnded();
                    break;
                case RecognitionState.Failed:
                    ExecuteGestureFailed();
                    break;
            }
        }
        
        protected abstract void ExecuteGestureReady();
        protected abstract void ExecuteGestureStarted();
        protected abstract void ExecuteGestureInProgress();
        protected abstract void ExecuteGestureEnded();
        protected abstract void ExecuteGestureFailed();
    }
}