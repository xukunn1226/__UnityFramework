using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    [RequireComponent(typeof(PlayerInput))]
    public class ScreenDragRecognizer : ContinuousGestureRecognizer<IScreenDragHandler, ScreenDragEventData>
    {
        public float MoveTolerance = 5.0f;

        [Range(0.1f, 5f)]
        public float DeltaScale = 1;

        private PlayerInput m_PlayerInput;
        private PlayerInput PlayerInput
        {
            get
            {
                if(m_PlayerInput == null)
                {
                    m_PlayerInput = gameObject.GetComponent<PlayerInput>();
                }
                return m_PlayerInput;
            }
        }

        public override int requiredPointerCount
        {
            get { return 1; }
        }

        internal override void InternalUpdate()
        {
            if(InputModule != null)
            {
                m_EventData.ClearPointerDatas();
                foreach(var data in InputModule.screenPointerData)
                {
                    m_EventData.AddPointerData(data.Value);
                }
            }
            base.InternalUpdate();
        }

        protected override bool CanBegin()
        {
            if(InputModule == null || PlayerInput == null)
                return false;
                
            if(m_EventData.PointerEventData.Count != requiredPointerCount)
                return false;

            if(m_EventData.GetAverageDistanceFromPress(requiredPointerCount) < MoveTolerance)
                return false;

            // 指向当前选中物体时不可拖拽
            if(PlayerInput.currentSelectedGameObject != null &&
                PlayerInput.currentSelectedGameObject == m_EventData[0].pointerEventData.pointerPressRaycast.gameObject)
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
            m_EventData.SetUsedBy(UsedBy.Drag);

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