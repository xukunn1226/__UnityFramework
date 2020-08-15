using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Framework.Gesture.Runtime
{
    [RequireComponent(typeof(PlayerInput))]
    public class ScreenPinchRecognizer : ContinuousGestureRecognizer<IScreenPinchHandler, ScreenPinchEventData>
    {
        // public float MinDOT = -0.7f;
        public float MinDistance = 5;
        [Range(0.1f, 5f)]
        public float DeltaScale = 1.0f;

        public override int requiredPointerCount
        {
            get { return 2; }
        }

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

        private PointerEventData m_Pointer1;
        private PointerEventData m_Pointer2;

        private float m_MouseScrollingDelta;

        internal override void InternalUpdate()
        {
            if(InputModule != null)
            {
                m_EventData.ClearPointerDatas();
                foreach(var data in InputModule.screenPointerData)
                {
                    m_EventData.AddPointerData(data.Value);
                }
                m_MouseScrollingDelta = InputModule.mouseScrollingDelta;
                m_Pointer1 = m_EventData[0]?.pointerEventData;
                m_Pointer2 = m_EventData[1]?.pointerEventData;
            }
            base.InternalUpdate();
        }

        protected override bool CanBegin()
        {
            if(m_EventData.pointerCount != requiredPointerCount)
                return false;

            if(m_Pointer1 == null || m_Pointer2 == null)
                return false;

            if(PlayerInput.currentSelectedGameObject != null &&
                PlayerInput.currentSelectedGameObject == m_Pointer1.pointerPressRaycast.gameObject)
                return false;

            if(PlayerInput.currentSelectedGameObject != null &&
                PlayerInput.currentSelectedGameObject == m_Pointer2.pointerPressRaycast.gameObject)
                return false;

            float startGap = Vector2.SqrMagnitude(m_Pointer1.pressPosition - m_Pointer2.pressPosition);
            float curGap = Vector2.SqrMagnitude(m_Pointer1.position - m_Pointer2.position);

#if UNITY_IOS || UNITY_ANDROID
            if(Mathf.Abs(startGap - curGap) < MinDistance * MinDistance)
                return false;
#endif

            return true;
        }

        protected override void OnBegin()
        {
            base.OnBegin();
            
            m_EventData.Gap = Vector2.Distance(m_Pointer1.position, m_Pointer2.position);
            m_EventData.DeltaMove = Input.mousePresent ? m_MouseScrollingDelta * DeltaScale : 0;
        }

        protected override RecognitionState OnProgress()
        {
            if(m_EventData.pointerCount != requiredPointerCount)
            {
                m_EventData.DeltaMove = 0;
                return RecognitionState.Ended;
            }

            m_EventData.Position = m_EventData.GetAveragePosition(requiredPointerCount);
            
            float curGap = Vector2.Distance(m_Pointer1.position, m_Pointer2.position);
            float newDelta = curGap - m_EventData.Gap;
            m_EventData.Gap = curGap;

            m_EventData.DeltaMove = (Input.mousePresent ? m_MouseScrollingDelta : newDelta) * DeltaScale;

            // m_EventData.DeltaMove = 0;
            // if(MovedInOppositeDirections(m_Pointer1, m_Pointer2, MinDOT))
            // {
            //     m_EventData.DeltaMove = newDelta;
            // }

            m_EventData.SetUsedBy(UsedBy.Pinch);
            GestureEvents.ExecuteContinous<IScreenPinchHandler, ScreenPinchEventData>(gameObject, m_EventData);

            return RecognitionState.InProgress;
        }

        private static bool MovedInOppositeDirections(PointerEventData pointer1, PointerEventData pointer2, float minDOT )
        {
            float dot = Vector2.Dot( pointer1.delta.normalized, pointer2.delta.normalized );
            return dot < minDOT;
        }
    }
    
    public class ScreenPinchEventData : GestureEventData
    {
        public float DeltaMove { get; internal set; }       // gap difference from last frame
        public float Gap { get; internal set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(base.ToString());
            sb.AppendLine($"<b>Delta</b>: {DeltaMove}");
            sb.AppendLine($"<b>Gap</b>: {Gap}");

            return sb.ToString();
        }
    }

    public interface IScreenPinchHandler : IContinuousGestureHandler<ScreenPinchEventData>
    {
    }
}