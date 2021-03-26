using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public enum RecognitionState
    {
        Ready,
        Started,
        InProgress,
        Failed,
        Ended,
    }

    internal enum UsedBy
    {
        None,
        LongPress,
        ScreenDrag,
        Pinch,
        ObjectDrag,
    }

    public class ScreenPointerData
    {
        public PointerEventData pointerEventData;
        internal UsedBy         usedBy;
        public float            startTime;
        public bool             bPressedThisFrame;
        public bool             bReleasedThisFrame;
    }

    public class GestureEventData
    {
        public delegate void EventHandler(GestureEventData gesture);
        public event EventHandler OnStateChanged;

        protected Dictionary<int, ScreenPointerData> m_PointerData = new Dictionary<int, ScreenPointerData>();

        public Dictionary<int, ScreenPointerData> PointerEventData
        {
            get { return m_PointerData; }
            internal set { m_PointerData = value; }
        }

        private Vector2 m_Position;
        public Vector2  Position        { get { return m_Position; } internal set { PrevPosition = m_Position; m_Position = value; } }
        public Vector2  PrevPosition    { get; internal set; }
        public Vector2  PressPosition;
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

                    OnStateChanged?.Invoke(this);
                }
            }
        }

        public RecognitionState PrevState
        {
            get { return m_PrevState; }
        }

        public GestureEventData()
        {}

        public ScreenPointerData this[int index]
        {
            get
            {
                if(index < 0 || index >= m_PointerData.Count)
                    return null;

                Dictionary<int, ScreenPointerData>.Enumerator e = m_PointerData.GetEnumerator();
                ScreenPointerData data = null;
                int c = 0;
                while(e.MoveNext())
                {
                    if(index == c)
                    {
                        data = e.Current.Value;
                        break;
                    }
                    ++c;
                }
                e.Dispose();
                return data;
            }
        }

        internal void SetUsedBy(UsedBy by)
        {
            foreach(var data in m_PointerData)
            {
                data.Value.usedBy = by;
            }
        }

        public bool AddPointerData(ScreenPointerData data)
        {
            if(m_PointerData.ContainsKey(data.pointerEventData.pointerId))
                return false;

            m_PointerData.Add(data.pointerEventData.pointerId, data);
            return true;
        }

        public void RemovePointerData(ScreenPointerData data)
        {
            if(!m_PointerData.ContainsKey(data.pointerEventData.pointerId))
                return;

            m_PointerData.Remove(data.pointerEventData.pointerId);
        }

        public void ClearPointerDatas()
        {
            m_PointerData.Clear();
        }

        public int pointerCount { get { return m_PointerData.Count; } }

        public Vector2 GetAveragePressPosition(int count)
        {            
            return AverageVector(GetPressPosition, count);
        }

        public Vector2 GetAveragePosition(int count)
        {
            return AverageVector(GetPosition, count);
        }

        public Vector2 GetAverageDelta(int count)
        {
            return AverageVector(GetDelta, count);
        }

        public float GetAverageDistanceFromPress(int count)
        {
            return AverageFloat(GetDistanceFromPress, count);
        }

        public void SetEventDataUsed(int count)
        {
            SetPropertyBoolean(SetPointerUsed, true, count);
        }

        public delegate T PropertyGetterDelegate<T>(PointerEventData pointer);

        private Vector2 AverageVector(PropertyGetterDelegate<Vector2> getProperty, int count)
        {
            Vector2 avg = Vector2.zero;

            if(m_PointerData.Count > 0 && count > 0)
            {
                Dictionary<int, ScreenPointerData>.Enumerator e = m_PointerData.GetEnumerator();
                int c = 0;
                while(e.MoveNext())
                {
                    if(c >= count)
                        break;

                    avg += getProperty(e.Current.Value.pointerEventData);
                    ++c;
                }
                avg /= c;
                e.Dispose();
            }

            return avg;
        }

        private float AverageFloat(PropertyGetterDelegate<float> getProperty, int count)
        {
            float avg = 0;

            if(m_PointerData.Count > 0 && count > 0)
            {
                Dictionary<int, ScreenPointerData>.Enumerator e = m_PointerData.GetEnumerator();
                int c = 0;
                while(e.MoveNext())
                {
                    if(c >= count)
                        break;

                    avg += getProperty(e.Current.Value.pointerEventData);
                    ++c;
                }
                avg /= c;
                e.Dispose();
            }

            return avg;
        }

        public delegate void PropertySetterDelegate<T>(PointerEventData pointer, T value);

        private void SetPropertyBoolean(PropertySetterDelegate<bool> setProperty, bool value, int count)
        {
            if(m_PointerData.Count > 0 && count > 0)
            {
                Dictionary<int, ScreenPointerData>.Enumerator e = m_PointerData.GetEnumerator();
                int c = 0;
                while(e.MoveNext())
                {
                    if(c >= count)
                        break;

                    setProperty(e.Current.Value.pointerEventData, value);
                    ++c;
                }
                e.Dispose();
            }
        }

        static private Vector2 GetPressPosition(PointerEventData eventData)
        {
            return eventData.pressPosition;
        }

        static private Vector2 GetPosition(PointerEventData eventData)
        {
            return eventData.position;
        }

        static private Vector2 GetDelta(PointerEventData eventData)
        {
            return eventData.delta;
        }

        static private float GetDistanceFromPress(PointerEventData eventData)
        {
            return Vector2.Distance(eventData.pressPosition, eventData.position);
        }

        static private void SetPointerUsed(PointerEventData eventData, bool value)
        {
            eventData.Use();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<b>PressPosition:</b> {PressPosition}");
            sb.AppendLine($"<b>Position</b>: {Position}");
            sb.AppendLine($"<b>StartTime:</b> {StartTime}");
            sb.AppendLine($"<b>ElapsedTime:</b> {ElapsedTime}");

            return sb.ToString();
        }
    }
}