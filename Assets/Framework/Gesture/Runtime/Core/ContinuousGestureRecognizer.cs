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
                    State = OnProgress();
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
        

        protected void ExecuteGestureReady()
        {
            // Debug.Log($"ContinuousGestureRecognizer:     ---- Ready   {Time.frameCount}    {typeof(T).Name} -- {typeof(K).Name}");
            GestureEvents.ExecuteReady_Continous<T, K>(gameObject, m_EventData);
        }
        protected void ExecuteGestureStarted()
        {
            // Debug.Log($"ContinuousGestureRecognizer:     ---- Started    {Time.frameCount}   {typeof(T).Name} -- {typeof(K).Name}");
            GestureEvents.ExecuteStarted_Continous<T, K>(gameObject, m_EventData);
        }
        protected void ExecuteGestureInProgress()
        {
            // Debug.Log($"ContinuousGestureRecognizer:     ---- InProgress   {Time.frameCount}    {typeof(T).Name} -- {typeof(K).Name}");
            GestureEvents.ExecuteProgress_Continous<T, K>(gameObject, m_EventData);
        }
        protected void ExecuteGestureEnded()
        {
            // Debug.Log($"ContinuousGestureRecognizer:     ---- Ended   {Time.frameCount}    {typeof(T).Name} -- {typeof(K).Name}");
            GestureEvents.ExecuteEnded_Continous<T, K>(gameObject, m_EventData);
        }
        protected void ExecuteGestureFailed()
        {
            // Debug.Log($"ContinuousGestureRecognizer:     ---- Failed    {Time.frameCount}   {typeof(T).Name} -- {typeof(K).Name}");
            GestureEvents.ExecuteFailed_Continous<T, K>(gameObject, m_EventData);
        }
    }
}