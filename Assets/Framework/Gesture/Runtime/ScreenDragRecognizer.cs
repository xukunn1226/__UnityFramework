using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    [RequireComponent(typeof(EventSystem))]
    public class ScreenDragRecognizer : ContinuousGestureRecognizer<IScreenDragHandler, ScreenDragEventData>
    {
        public float MoveTolerance = 1.0f;

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
            if(InputModule != null)
            {
                InputModule.UpdateUnusedEventData(ref m_UnusedPointerData);
                m_EventData.PointerEventData = m_UnusedPointerData;
                // Debug.Log($"======== {m_UnusedPointerData.Count}");
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
            
            return true;
        }

        protected override void OnBegin()
        {
            base.OnBegin();

            m_EventData.DeltaMove = m_EventData.Position - m_EventData.PressPosition;
            m_EventData.LastPos = m_EventData.Position;
        }

        protected override RecognitionState OnProgress()
        {
            if(m_EventData.pointerCount != requiredPointerCount)
                return RecognitionState.Ended;

            m_EventData.Position = m_EventData.GetAveragePosition(requiredPointerCount);
            m_EventData.DeltaMove = m_EventData.Position - m_EventData.LastPos;
            m_EventData.LastPos = m_EventData.Position;

            ExecuteGestureInProgress();
            return RecognitionState.InProgress;
        }
        
        protected override void ExecuteGestureReady()
        {
            Debug.Log($"ScreenDrag:      --- Ready      {m_EventData.DeltaMove}");
            GestureEvents.Execute<IScreenDragHandler, ScreenDragEventData>(gameObject, m_EventData, GestureEvents.ExecuteReady);
        }
        protected override void ExecuteGestureStarted()
        {
            Debug.Log($"ScreenDrag:      --- Started      {m_EventData.DeltaMove}");
            GestureEvents.Execute<IScreenDragHandler, ScreenDragEventData>(gameObject, m_EventData, GestureEvents.ExecuteStarted);
        }
        protected override void ExecuteGestureInProgress()
        {
            Debug.Log($"ScreenDrag:      --- InProgress      {m_EventData.DeltaMove}");
            GestureEvents.Execute<IScreenDragHandler, ScreenDragEventData>(gameObject, m_EventData, GestureEvents.ExecuteProgress);
        }
        protected override void ExecuteGestureEnded()
        {
            Debug.Log($"ScreenDrag:      --- Ended      {m_EventData.DeltaMove}");
            GestureEvents.Execute<IScreenDragHandler, ScreenDragEventData>(gameObject, m_EventData, GestureEvents.ExecuteEnded);
        }
        protected override void ExecuteGestureFailed()
        {
            Debug.Log($"ScreenDrag:      --- Failed      {m_EventData.DeltaMove}");
            GestureEvents.Execute<IScreenDragHandler, ScreenDragEventData>(gameObject, m_EventData, GestureEvents.ExecuteFailed);
        }
    }
    
    public class ScreenDragEventData : GestureEventData
    {
        private Vector2 m_DeltaMove;
        public Vector2 DeltaMove
        {
            get { return m_DeltaMove; }
            internal set
            {
                m_DeltaMove = value;
            }
        }

        internal Vector2 LastPos;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(base.ToString());
            sb.AppendLine($"<b>DeltaMove</b>: {DeltaMove}");

            return sb.ToString();
        }
    }

    public interface IScreenDragHandler : IContinuousGestureHandler<ScreenDragEventData>
    {
    }
}