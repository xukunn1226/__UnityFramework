using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public class GestureEventData
    {
        protected Dictionary<int, PointerEventData> m_PointerData = new Dictionary<int, PointerEventData>();

        public Vector2  PressPosition;
        public float    StartTime;
        public float    ElapsedTime     { get { return Time.time - StartTime; } }

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

        public delegate T PropertyGetterDelegate<T>(PointerEventData pointer);

        public Vector2 AverageVector(PropertyGetterDelegate<Vector2> getProperty, int count)
        {
            Vector2 avg = Vector2.zero;

            if(m_PointerData.Count > 0 && count > 0)
            {
                Dictionary<int, PointerEventData>.Enumerator e = m_PointerData.GetEnumerator();
                int c = 0;
                while(e.MoveNext())
                {
                    if(c >= count)
                        break;

                    avg += getProperty(e.Current.Value);
                    ++c;
                }
                avg /= c;
                e.Dispose();
            }

            return avg;
        }

        public float AverageFloat(PropertyGetterDelegate<float> getProperty, int count)
        {
            float avg = 0;

            if(m_PointerData.Count > 0 && count > 0)
            {
                Dictionary<int, PointerEventData>.Enumerator e = m_PointerData.GetEnumerator();
                int c = 0;
                while(e.MoveNext())
                {
                    if(c >= count)
                        break;

                    avg += getProperty(e.Current.Value);
                    ++c;
                }
                avg /= c;
                e.Dispose();
            }

            return avg;
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
    }
}