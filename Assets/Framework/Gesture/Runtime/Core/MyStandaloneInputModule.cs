using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.Gesture.Runtime
{
    public class MyStandaloneInputModule : StandaloneInputModule
    {
        public override void Process()
        {
            base.Process();

            // 为了整合IEventSystemHandler和Gesture，消息优先IEventSystemHandler处理，然后是IGestureHandler
            GestureRecognizerManager.Update();
        }

        // collection of PointerEventData accord with:
        // 1. raycast nothing OR
        // 2. No Over UI & Not consumed by interactive object
        public void UpdateUnusedEventData(ref Dictionary<int, PointerEventData> unusedPointerData)
        {
            if(!ProcessUnusedTouchEventData(ref unusedPointerData) && input.mousePresent)
                ProcessUnusedMouseEventData(ref unusedPointerData);
        }

        private bool ProcessUnusedTouchEventData(ref Dictionary<int, PointerEventData> unusedPointerData)
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
                        AddUnusedPointerEventData(ref unusedPointerData, eventData);
                    }
                }
                else if(released || eventData.used)
                {
                    RemoveUnusedPointerEventData(ref unusedPointerData, touch.fingerId);
                }
            }
            return Input.touchCount > 0;
        }

        private void ProcessUnusedMouseEventData(ref Dictionary<int, PointerEventData> unusedPointerData)
        {
            PointerEventData eventData = GetLastPointerEventData(kMouseLeftId);
            MouseState mouseState = GetMousePointerEventData();
            ButtonState buttonState = mouseState.GetButtonState(PointerEventData.InputButton.Left);
            if(buttonState.eventData.PressedThisFrame())
            {
                if(!IsPointerOverUI(eventData) && !eventData.used)
                {
                    AddUnusedPointerEventData(ref unusedPointerData, eventData);
                }
            }
            else if(buttonState.eventData.ReleasedThisFrame() || eventData.used)
            {
                RemoveUnusedPointerEventData(ref unusedPointerData, eventData.pointerId);
            }
        }

        private void AddUnusedPointerEventData(ref Dictionary<int, PointerEventData> unusedPointerData, PointerEventData eventData)
        {
            if(unusedPointerData.ContainsKey(eventData.pointerId))
                return;
            unusedPointerData.Add(eventData.pointerId, eventData);
        }

        private void RemoveUnusedPointerEventData(ref Dictionary<int, PointerEventData> unusedPointerData, int id)
        {
            if(!unusedPointerData.ContainsKey(id))
                return;
            unusedPointerData.Remove(id);
        }

        static private bool IsPointerOverUI(PointerEventData eventData)
        {
            if(eventData.pointerCurrentRaycast.gameObject == null)
                return false;

            return eventData.pointerCurrentRaycast.module is GraphicRaycaster;
        }
    }
}