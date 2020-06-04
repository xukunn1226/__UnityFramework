using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public abstract class GestureRecognizer<T> : MonoBehaviour where T : GestureEventData, new()
    {
        public delegate void GestureEventHandler(T eventData);
        public event GestureEventHandler OnGesture;

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
                }
            }
        }

        public RecognitionState PrevState
        {
            get { return m_PrevState; }
        }

        public bool ContinuousRecognizeWhenFailed;          // whether or not to recognize gesture when failed

        public int RequiredPointerCount = 1;

        protected T m_EventData = new T();
 
        protected void AddPointer(PointerEventData eventData)
        {
            m_EventData.AddPointerData(eventData);
        }

        protected void RemovePointer(PointerEventData eventData)
        {
            m_EventData.RemovePointerData(eventData);

            if(m_EventData.pointerCount == 0)
            {
                State = RecognitionState.Ready;
            }
        }

        protected virtual bool CanBegin()
        {
            return m_EventData.pointerCount == RequiredPointerCount;
        }

        private void Begin()
        {
            OnBegin();
            State = RecognitionState.Started;
        }

        protected abstract void OnBegin();

        protected abstract RecognitionState OnProgress();

        private void Update()
        {
            switch(State)
            {
                case RecognitionState.Ready:
                    if(CanBegin())
                        OnBegin();
                    break;
                case RecognitionState.Started:
                    State = RecognitionState.InProgress;
                    break;
                case RecognitionState.InProgress:
                    OnProgress();
                    break;
                case RecognitionState.Failed:
                case RecognitionState.Ended:
                    
                    break;
            }
        }
    }
}