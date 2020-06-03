using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public class GestureEventData
    {
        protected Dictionary<int, PointerEventData> m_PointerData = new Dictionary<int, PointerEventData>();
      
        public enum RecognitionState
        {
            Ready,
            Started,
            InProgress,
            Failed,
            Ended,
        }

        public Vector2  StartPosition;
        public Vector2  Position;
        public float    StartTime;
        public float    ElapsedTime     { get { return Time.time - StartTime; } }

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

        public GestureEventData()
        {}

        public bool AddPointerData(PointerEventData data)
        {
            if(m_PointerData.ContainsKey(data.pointerId))
                return false;

            m_PointerData.Add(data.pointerId, data);
            return true;
        }

        public void RemovePointerData(PointerEventData data)
        {
            m_PointerData.Remove(data.pointerId);
        }

        public int pointerCount { get { return m_PointerData.Count; } }


    }
}