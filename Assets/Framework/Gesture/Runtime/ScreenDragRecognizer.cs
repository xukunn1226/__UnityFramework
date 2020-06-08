using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    [RequireComponent(typeof(EventSystem))]
    public class ScreenDragRecognizer : ContinuousGestureRecognizer<IScreenDragHandler, ScreenDragEventData>//, IPointerDownHandler, IPointerUpHandler
    {
        public float MoveTolerance = 1.0f;

        public override int requiredPointerCount
        {
            get { return 1; }
            set { throw new System.ArgumentException("not support!"); }
        }
        
        // public void OnPointerDown(PointerEventData eventData)
        // {
        //     AddPointer(eventData);
        // }

        // public void OnPointerUp(PointerEventData eventData)
        // {
        //     RemovePointer(eventData);
        // }

        protected override void Update()
        {
            base.Update();


        }       

        protected override bool CanBegin()
        {
            if(!base.CanBegin())
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
                return RecognitionState.Failed;

            m_EventData.Position = m_EventData.GetAveragePosition(requiredPointerCount);
            m_EventData.DeltaMove = m_EventData.Position - m_EventData.LastPos;
            m_EventData.LastPos = m_EventData.Position;

            ExecuteGestureInProgress();
            return RecognitionState.InProgress;
        }
        
        protected override void ExecuteGestureReady()
        {
            GestureEvents.Execute<IScreenDragHandler, ScreenDragEventData>(gameObject, m_EventData, GestureEvents.ExecuteReady);
        }
        protected override void ExecuteGestureStarted()
        {
            GestureEvents.Execute<IScreenDragHandler, ScreenDragEventData>(gameObject, m_EventData, GestureEvents.ExecuteStarted);
        }
        protected override void ExecuteGestureInProgress()
        {
            GestureEvents.Execute<IScreenDragHandler, ScreenDragEventData>(gameObject, m_EventData, GestureEvents.ExecuteProgress);
        }
        protected override void ExecuteGestureEnded()
        {
            GestureEvents.Execute<IScreenDragHandler, ScreenDragEventData>(gameObject, m_EventData, GestureEvents.ExecuteEnded);
        }
        protected override void ExecuteGestureFailed()
        {
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