using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.Gesture.Runtime
{
    public class MyStandaloneInputModule : StandaloneInputModule
    {
        // collection of PointerEventData accord with:
        // 1. raycast nothing OR
        // 2. No Over UI & Not consumed by interactive object
        private Dictionary<int, PointerEventData> m_UnusedPointerData = new Dictionary<int, PointerEventData>();

        public Dictionary<int, PointerEventData> UnusedPointerData => m_UnusedPointerData;

        public override void Process()
        {
            base.Process();

            if(!ProcessUnusedTouchEventData() && input.mousePresent)
                ProcessUnusedMouseEventData();
        }

        protected bool ProcessUnusedTouchEventData()
        {
            for(int i = 0; i < Input.touchCount; ++i)
            {
                Touch touch = Input.touches[i];
                bool pressed = touch.phase == TouchPhase.Began;
                bool released = (touch.phase == TouchPhase.Canceled) || (touch.phase == TouchPhase.Ended);

                PointerEventData eventData = GetLastPointerEventData(touch.fingerId);
                if(pressed)
                {
                    if(!IsPointerOverUI(eventData) && !eventData.used)
                    {
                        AddUnusedPointerEventData(eventData);
                    }
                }
                else if(released || eventData.used)
                {
                    RemoveUnusedPointerEventData(touch.fingerId);
                }
            }
            return Input.touchCount > 0;
        }

        protected void ProcessUnusedMouseEventData()
        {
            PointerEventData eventData = GetLastPointerEventData(kMouseLeftId);
            MouseState mouseState = GetMousePointerEventData();
            ButtonState buttonState = mouseState.GetButtonState(PointerEventData.InputButton.Left);
            if(buttonState.eventData.PressedThisFrame())
            {
                if(!IsPointerOverUI(eventData) && !eventData.used)
                {
                    AddUnusedPointerEventData(eventData);
                }
            }
            else if(buttonState.eventData.ReleasedThisFrame() || eventData.used)
            {
                RemoveUnusedPointerEventData(eventData.pointerId);
            }
        }

        private void AddUnusedPointerEventData(PointerEventData eventData)
        {
            if(m_UnusedPointerData.ContainsKey(eventData.pointerId))
                return;
            m_UnusedPointerData.Add(eventData.pointerId, eventData);
        }

        private void RemoveUnusedPointerEventData(int id)
        {
            if(!m_UnusedPointerData.ContainsKey(id))
                return;
            m_UnusedPointerData.Remove(id);
        }

        static private bool IsPointerOverUI(PointerEventData eventData)
        {
            if(eventData.pointerCurrentRaycast.gameObject == null)
                return false;

            return eventData.pointerCurrentRaycast.module is GraphicRaycaster;
        }
    }
}