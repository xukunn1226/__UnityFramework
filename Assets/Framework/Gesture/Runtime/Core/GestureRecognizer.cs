using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public abstract class GestureRecognizer<T> : MonoBehaviour where T : GestureEventData, new()
    {
        public enum RecognitionState
        {
            Ready,
            Started,
            InProgress,
            Failed,
            Ended,
        }

        private RecognitionState m_CurState = RecognitionState.Ready;
        private RecognitionState m_PrevState = RecognitionState.Ready;

        public RecognitionState State
        {
            get { return m_CurState; }
            set
            {
                if(m_CurState != value)
                {
                    m_PrevState = m_CurState;
                    m_CurState = value;

                    OnStateChanged();
                }
            }
        }

        public RecognitionState PrevState
        {
            get { return m_PrevState; }
        }

        [Min(1)][SerializeField]
        private int m_RequiredPointerCount = 1;

        public virtual int requiredPointerCount
        {
            get { return m_RequiredPointerCount; }
            set { m_RequiredPointerCount = value; }
        }

        protected T m_EventData = new T();
 
        protected void AddPointer(PointerEventData eventData)
        {
            m_EventData.AddPointerData(eventData);
        }

        protected void RemovePointer(PointerEventData eventData)
        {
            m_EventData.RemovePointerData(eventData);
        }

        protected virtual void OnStateChanged()
        {
            RaiseEvent();
        }

        protected virtual bool CanBegin()
        {
            return m_EventData.pointerCount == requiredPointerCount;
        }

        protected abstract void OnBegin();

        protected abstract RecognitionState OnProgress();

        protected abstract void RaiseEvent();
    }
}