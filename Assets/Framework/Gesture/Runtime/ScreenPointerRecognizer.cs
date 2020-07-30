using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    [RequireComponent(typeof(PlayerInput))]
    public class ScreenPointerRecognizer : DiscreteGestureRecognizer<IScreenPointerUpHandler, ScreenPointerEventData>
    {
        public bool sendDownMessage = true;
        public bool sendUpMessage = true;

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
                    if(sendDownMessage && !MyStandaloneInputModule.IsPointerOverUI(eventData))
                    {
                        m_EventData.singlePointerData = eventData;
                        m_EventData.bPressed = true;
                        GestureEvents.ExecuteDiscrete<IScreenPointerDownHandler, ScreenPointerEventData>(gameObject, m_EventData);
                    }
                }
                else if(released)
                {
                    if(sendUpMessage)
                    {
                        m_EventData.singlePointerData = eventData;
                        m_EventData.bPressed = false;
                        GestureEvents.ExecuteDiscrete<IScreenPointerUpHandler, ScreenPointerEventData>(gameObject, m_EventData);
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
                if(sendDownMessage && !MyStandaloneInputModule.IsPointerOverUI(eventData))
                {
                    m_EventData.singlePointerData = eventData;
                    m_EventData.bPressed = true;
                    GestureEvents.ExecuteDiscrete<IScreenPointerDownHandler, ScreenPointerEventData>(gameObject, m_EventData);
                }
            }
            else if(released)
            {
                if(sendUpMessage)
                {
                    m_EventData.singlePointerData = eventData;
                    m_EventData.bPressed = false;
                    GestureEvents.ExecuteDiscrete<IScreenPointerUpHandler, ScreenPointerEventData>(gameObject, m_EventData);
                }
            }
        }

        protected override bool CanBegin()
        {
            return false;
        }

        protected override RecognitionState OnProgress()
        {
            return RecognitionState.Ended;
        }
    }

    public class ScreenPointerEventData : GestureEventData
    {
        public PointerEventData singlePointerData;
        public bool bPressed;
    }

    public interface IScreenPointerUpHandler : IDiscreteGestureHandler<ScreenPointerEventData>
    {}

    public interface IScreenPointerDownHandler : IDiscreteGestureHandler<ScreenPointerEventData>
    {}
}