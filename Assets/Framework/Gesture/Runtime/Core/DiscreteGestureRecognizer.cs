using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public abstract class DiscreteGestureRecognizer<T, K> : GestureRecognizer<K> where K : GestureEventData, new() where T : IDiscreteGestureHandler<K>
    {
        protected override void RaiseEvent(GestureEventData eventData)
        {
            switch(m_EventData.State)
            {
                case RecognitionState.Ready:
                    // Debug.Log($"DiscreteGestureRecognizer:     ---- Ready       {typeof(T).Name} -- {typeof(K).Name}");
                    break;
                case RecognitionState.Ended:                
                    // Debug.Log($"DiscreteGestureRecognizer:     ---- Recognized       {typeof(T).Name} -- {typeof(K).Name}");
                    GestureEvents.ExecuteDiscrete<T, K>(eventTarget, m_EventData);
                    break;
                case RecognitionState.Failed:
                    // Debug.Log($"DiscreteGestureRecognizer:     ---- Failed       {typeof(T).Name} -- {typeof(K).Name}");
                    break;
            }
        }
    }
}