using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public abstract class DiscreteGestureRecognizer<T, K> : GestureRecognizer<K> where K : GestureEventData, new() where T : IDiscreteGestureHandler<K>
    {
        // life cycle: Ready -> InProgress -> Failed/Ended
        internal override void InternalUpdate()
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
                case RecognitionState.Failed:               // 清空数据，重置状态Ready
                    m_EventData.ClearPointerDatas();
                    State = RecognitionState.Ready;
                    break;
                case RecognitionState.Ended:                // 持续持有状态，设置used标志
                    m_EventData.SetEventDataUsed(requiredPointerCount);                    
                    if(m_EventData.pointerCount == 0)
                    {
                        State = RecognitionState.Ready;
                    }
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
                case RecognitionState.Ended:
                    ExecuteGestureRecognized();
                    break;
                case RecognitionState.Failed:
                    ExecuteGestureFailed();
                    break;
            }
        }

        protected abstract void ExecuteGestureReady();
        protected abstract void ExecuteGestureRecognized();
        protected abstract void ExecuteGestureFailed();
    }
}