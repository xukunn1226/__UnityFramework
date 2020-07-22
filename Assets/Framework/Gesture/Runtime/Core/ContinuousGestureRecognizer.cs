using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Gesture.Runtime
{
    public abstract class ContinuousGestureRecognizer<T, K> : GestureRecognizer<K> where K : GestureEventData, new() where T : IContinuousGestureHandler<K>
    {
        protected override void RaiseEvent()
        {
            switch(State)
            {
                case RecognitionState.Ready:
                    // Debug.Log($"ContinuousGestureRecognizer:     ---- Ready   {Time.frameCount}    {typeof(T).Name} -- {typeof(K).Name}");
                    // GestureEvents.ExecuteReady_Continous<T, K>(gameObject, m_EventData);
                    break;
                case RecognitionState.Started:
                    // Debug.Log($"ContinuousGestureRecognizer:     ---- Started    {Time.frameCount}   {typeof(T).Name} -- {typeof(K).Name}");
                    GestureEvents.ExecuteStarted_Continous<T, K>(gameObject, m_EventData);
                    break;
                case RecognitionState.InProgress:
                    // Debug.Log($"ContinuousGestureRecognizer:     ---- InProgress   {Time.frameCount}    {typeof(T).Name} -- {typeof(K).Name}");
                    GestureEvents.ExecuteProgress_Continous<T, K>(gameObject, m_EventData);
                    break;
                case RecognitionState.Ended:
                    // Debug.Log($"ContinuousGestureRecognizer:     ---- Ended   {Time.frameCount}    {typeof(T).Name} -- {typeof(K).Name}");
                    GestureEvents.ExecuteEnded_Continous<T, K>(gameObject, m_EventData);
                    break;
                case RecognitionState.Failed:
                    // Debug.Log($"ContinuousGestureRecognizer:     ---- Failed    {Time.frameCount}   {typeof(T).Name} -- {typeof(K).Name}");
                    GestureEvents.ExecuteFailed_Continous<T, K>(gameObject, m_EventData);
                    break;
            }
        }
    }
}