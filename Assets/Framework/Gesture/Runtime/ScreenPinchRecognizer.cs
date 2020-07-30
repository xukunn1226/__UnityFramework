using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Framework.Gesture.Runtime
{
    [RequireComponent(typeof(PlayerInput))]
    public class ScreenPinchRecognizer : ContinuousGestureRecognizer<IPinchHandler, PinchEventData>
    {
        // public float MinDOT = -0.7f;
        public float MinDistance = 5;
        public float DeltaScale = 1.0f;

        public override int requiredPointerCount
        {
            get { return 2; }
            set { throw new System.ArgumentException("not support!"); }
        }

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

        private Dictionary<int, PointerEventData> m_UnusedPointerData = new Dictionary<int, PointerEventData>();
        private PointerEventData m_Pointer1;
        private PointerEventData m_Pointer2;

        private float m_MouseScrollingDelta;

        internal override void InternalUpdate()
        {
            // collect gesture event data
            if(InputModule != null)
            {
                InputModule.isPinchFetch = true;      // ugly code
                InputModule.UpdateUnusedEventData(ref m_UnusedPointerData);
                m_MouseScrollingDelta = InputModule.mouseScrollingDelta;
                InputModule.isPinchFetch = false;
                m_EventData.PointerEventData = m_UnusedPointerData;
                m_Pointer1 = m_EventData[0];
                m_Pointer2 = m_EventData[1];
            }
            base.InternalUpdate();
        }

        protected override bool CanBegin()
        {
            if(m_EventData.pointerCount < requiredPointerCount)         // 允许pointerCount >= requiredPointerCount，后续pointer不生效
                return false;

            if(m_Pointer1 == null || m_Pointer2 == null)
                return false;

            float startGap = Vector2.SqrMagnitude(m_Pointer1.pressPosition - m_Pointer2.pressPosition);
            float curGap = Vector2.SqrMagnitude(m_Pointer1.position - m_Pointer2.position);

#if UNITY_IOS || UNITY_ANDROID
            if(Mathf.Abs(startGap - curGap) < MinDistance * MinDistance)
                return false;
#endif

            // Debug.Log($"CanBegin2222: {Time.frameCount}");
            return true;
        }

        protected override void OnBegin()
        {
            base.OnBegin();
            
            m_EventData.Gap = Vector2.Distance(m_Pointer1.position, m_Pointer2.position);
            m_EventData.DeltaMove = 0;

            // Debug.Log($"OnBegin: {Time.frameCount}      {m_EventData.Gap}   {m_EventData.Position}");
        }

        protected override RecognitionState OnProgress()
        {
            if(m_EventData.pointerCount < requiredPointerCount)
            {
                // Debug.Log($"OnProgress.Ended: {Time.frameCount}");
                return RecognitionState.Ended;
            }

            m_EventData.Position = m_EventData.GetAveragePosition(requiredPointerCount);
            
            float curGap = Vector2.Distance(m_Pointer1.position, m_Pointer2.position);
            float newDelta = (curGap - m_EventData.Gap) * DeltaScale;
            m_EventData.Gap = curGap;

            m_EventData.DeltaMove = Input.mousePresent ? m_MouseScrollingDelta * DeltaScale : newDelta;

            // Debug.Log($"OnProgress: {Time.frameCount}   {m_EventData.Delta}");

            // m_EventData.Delta = 0;
            // if(MovedInOppositeDirections(m_Pointer1, m_Pointer2, MinDOT))
            // {
            //     m_EventData.Delta = newDelta;
            // }

            // m_EventData.SetEventDataUsed(requiredPointerCount);
            GestureEvents.ExecuteContinous<IPinchHandler, PinchEventData>(gameObject, m_EventData);

            return RecognitionState.InProgress;
        }

        private static bool MovedInOppositeDirections(PointerEventData pointer1, PointerEventData pointer2, float minDOT )
        {
            float dot = Vector2.Dot( pointer1.delta.normalized, pointer2.delta.normalized );
            return dot < minDOT;
        }
    }
    
    public class PinchEventData : GestureEventData
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

    public interface IPinchHandler : IContinuousGestureHandler<PinchEventData>
    {
    }
}