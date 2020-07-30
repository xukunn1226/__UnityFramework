using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    [RequireComponent(typeof(PlayerInput))]
    public class LongPressRecognizer : DiscreteGestureRecognizer<ILongPressHandler, LongPressEventData>
    {
        public float Duration = 1.0f;

        public float MoveTolerance = 5.0f;

        public override int requiredPointerCount
        {
            get { return 1; }
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

        internal override void InternalUpdate()
        {
            if(InputModule == null)
                return;

            ProcessInput();
            base.InternalUpdate();
        }

        private void ProcessInput()
        {
            if(!ProcessTouchInput() && Input.mousePresent)
                ProcessMouseInput();
        }

        private bool ProcessTouchInput()
        {
            for(int i = 0; i < Input.touchCount; ++i)
            {
                Touch touch = Input.touches[i];
                bool pressed = touch.phase == TouchPhase.Began;
                bool released = (touch.phase == TouchPhase.Canceled) || (touch.phase == TouchPhase.Ended);

                PointerEventData eventData = InputModule.GetPointerEventData(touch.fingerId);
                if(pressed)
                {
                    if(!MyStandaloneInputModule.IsPointerOverUI(eventData))
                    {
                        m_EventData.singlePointerData = eventData;
                        m_EventData.bPressed = true;
                        GestureEvents.ExecuteDiscrete<ILongPressHandler, LongPressEventData>(gameObject, m_EventData);
                    }
                }
                else if(released)
                {
                    {
                        m_EventData.singlePointerData = eventData;
                        m_EventData.bPressed = false;
                        GestureEvents.ExecuteDiscrete<ILongPressHandler, LongPressEventData>(gameObject, m_EventData);
                    }
                }
            }
            return Input.touchCount > 0;
        }

        private void ProcessMouseInput()
        {
            PointerEventData eventData = InputModule.GetPointerEventData(-1);

            bool pressed = Input.GetMouseButtonDown(0);
            bool released = Input.GetMouseButtonUp(0);
            if(pressed)
            {
                if(!MyStandaloneInputModule.IsPointerOverUI(eventData))
                {
                    m_EventData.singlePointerData = eventData;
                    m_EventData.bPressed = true;
                    GestureEvents.ExecuteDiscrete<ILongPressHandler, LongPressEventData>(gameObject, m_EventData);
                }
            }
            else if(released)
            {
                {
                    m_EventData.singlePointerData = eventData;
                    m_EventData.bPressed = false;
                    GestureEvents.ExecuteDiscrete<ILongPressHandler, LongPressEventData>(gameObject, m_EventData);
                }
            }
        }

        protected override RecognitionState OnProgress()
        {
            if(m_EventData.pointerCount != requiredPointerCount)
                return RecognitionState.Failed;

            if(m_EventData.GetAverageDistanceFromPress(requiredPointerCount) > MoveTolerance)
                return RecognitionState.Failed;
                
            if(m_EventData.ElapsedTime > Duration)
            {
                // m_EventData.SetEventDataUsed(requiredPointerCount);
                return RecognitionState.Ended;
            }                

            return RecognitionState.InProgress;
        }
    }    
    
    public class LongPressEventData : GestureEventData
    {
        public PointerEventData singlePointerData;
        public bool bPressed;
    }

    public interface ILongPressHandler : IDiscreteGestureHandler<LongPressEventData>
    {
    }
}