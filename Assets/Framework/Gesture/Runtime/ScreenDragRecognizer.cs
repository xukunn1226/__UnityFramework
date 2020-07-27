using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public class ScreenDragRecognizer : ContinuousGestureRecognizer<IScreenDragHandler, ScreenDragEventData>
    {
        public float MoveTolerance = 5.0f;

        [Range(0.1f, 5f)]
        public float DeltaScale = 1;

        private MyStandaloneInputModule m_InputModule;
        private MyStandaloneInputModule InputModule
        {
            get
            {
                if(m_InputModule == null)
                {
                    m_InputModule = EventSystem.current.currentInputModule as MyStandaloneInputModule;
                }
                return m_InputModule;
            }
        }

        public override int requiredPointerCount
        {
            get { return 1; }
            set { throw new System.ArgumentException("not support!"); }
        }
        
        private Dictionary<int, PointerEventData> m_UnusedPointerData = new Dictionary<int, PointerEventData>();


        internal override void InternalUpdate()
        {
            // collect gesture event data
            if(InputModule != null)
            {
                InputModule.UpdateUnusedEventData(ref m_UnusedPointerData);
                m_EventData.PointerEventData = m_UnusedPointerData;
            }
            base.InternalUpdate();
        }

        protected override bool CanBegin()
        {
            if(InputModule == null)
                return false;
                
            if(m_EventData.PointerEventData.Count != requiredPointerCount)
                return false;

            if(m_EventData.GetAverageDistanceFromPress(requiredPointerCount) < MoveTolerance)
                return false;

            // todo: EventSystem.currentSelectedGameObject逻辑不符合需求，适合UI内部使用，MyStandaloneInputModule扩展currentSelectedGameObject使用
            if(EventSystem.current.currentSelectedGameObject != null && 
               EventSystem.current.currentSelectedGameObject == m_EventData[0].pointerPressRaycast.gameObject)
                return false;
            
            return true;
        }

        protected override void OnBegin()
        {
            base.OnBegin();

            m_EventData.DeltaMove = Vector2.zero;
            m_EventData.LastPos = m_EventData.Position;
            m_EventData.PressPosition = m_EventData.Position;           // MoveTolerance原因，重新定位PressPosition
        }

        protected override RecognitionState OnProgress()
        {
            if(m_EventData.pointerCount != requiredPointerCount)
                return RecognitionState.Ended;

            m_EventData.Position = m_EventData.GetAveragePosition(requiredPointerCount);
            m_EventData.DeltaMove = (m_EventData.Position - m_EventData.LastPos) * DeltaScale;
            m_EventData.LastPos = m_EventData.Position;
            m_EventData.SetEventDataUsed(requiredPointerCount);
            GestureEvents.ExecuteContinous<IScreenDragHandler, ScreenDragEventData>(gameObject, m_EventData);
            return RecognitionState.InProgress;
        }
    }
    
    public class ScreenDragEventData : GestureEventData
    {
        public Vector2 DeltaMove { get; internal set; }
        public Vector2 Speed { get { return DeltaMove / Time.deltaTime; } }
        internal Vector2 LastPos;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(base.ToString());
            sb.AppendLine($"<b>DeltaMove</b>: {DeltaMove}");
            sb.AppendLine($"<b>Speed</b>: {Speed}");

            return sb.ToString();
        }
    }

    public interface IScreenDragHandler : IContinuousGestureHandler<ScreenDragEventData>
    {
    }
}