﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.Gesture.Runtime
{
    public class MyStandaloneInputModule : StandaloneInputModule
    {
        // private GameObject m_CurrentSelected;           // 自定义的当前选中对象，不同于EventSystem.m_CurrentSelected
        // public GameObject currentSelectedGameObject
        // {
        //     get { return m_CurrentSelected; } 
        // }

        private bool m_wasMouseScrolling;
        private bool m_isMouseScrolling;
        public bool m_isPinchFetch;

        public override void Process()
        {
            base.Process();

            if (!eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus())
                return;

            // 为了整合IEventSystemHandler和Gesture，消息优先IEventSystemHandler处理，然后是IGestureHandler
            GestureRecognizerManager.Update();
        }

        private bool ShouldIgnoreEventsOnNoFocus()
        {
            switch (SystemInfo.operatingSystemFamily)
            {
                case OperatingSystemFamily.Windows:
                case OperatingSystemFamily.Linux:
                case OperatingSystemFamily.MacOSX:
#if UNITY_EDITOR
                    if (UnityEditor.EditorApplication.isRemoteConnected)
                        return false;
#endif
                    return true;
                default:
                    return false;
            }
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

            if(m_isPinchFetch)         // ugly code, Pinch只需监听mouse scroll
            {
                m_wasMouseScrolling = m_isMouseScrolling;
                if(Input.mouseScrollDelta.y != 0)
                {
                    m_isMouseScrolling = true;
                }
                else
                {
                    m_isMouseScrolling = false;                
                }

                if(m_isMouseScrolling)
                {
                    Debug.Log($"begin scroll: {Time.frameCount}");
                    if(!IsPointerOverUI(eventData))
                    {
                        AddUnusedPointerEventData(ref unusedPointerData, GetLastPointerEventData(kMouseLeftId));
                        AddUnusedPointerEventData(ref unusedPointerData, GetLastPointerEventData(kMouseRightId));
                    }
                }
                if(m_wasMouseScrolling && !m_isMouseScrolling)
                {
                    Debug.LogWarning($"end scroll: {Time.frameCount}");
                    RemoveUnusedPointerEventData(ref unusedPointerData, GetLastPointerEventData(kMouseLeftId).pointerId);
                    RemoveUnusedPointerEventData(ref unusedPointerData, GetLastPointerEventData(kMouseRightId).pointerId);
                }                
            }
            else
            {                
                bool pressed = input.GetMouseButtonDown(0);
                bool released = input.GetMouseButtonUp(0);
                if(pressed)
                {
                    if(!IsPointerOverUI(eventData) && !eventData.used)
                    {
                        AddUnusedPointerEventData(ref unusedPointerData, eventData);
                    }
                }
                else if(released || eventData.used)
                {
                    RemoveUnusedPointerEventData(ref unusedPointerData, eventData.pointerId);
                }
            }
        }

        private void AddUnusedPointerEventData(ref Dictionary<int, PointerEventData> unusedPointerData, PointerEventData eventData)
        {
            if(unusedPointerData.ContainsKey(eventData.pointerId))
                return;
            
            Debug.LogWarning($"Add: {eventData.pointerId}   {Time.frameCount}");
            unusedPointerData.Add(eventData.pointerId, eventData);
        }

        private void RemoveUnusedPointerEventData(ref Dictionary<int, PointerEventData> unusedPointerData, int id)
        {
            if(!unusedPointerData.ContainsKey(id))
                return;

            Debug.LogError($"Remove: {id}   {Time.frameCount}");
            unusedPointerData.Remove(id);
        }

        static private bool IsPointerOverUI(PointerEventData eventData)
        {
            if(eventData.pointerCurrentRaycast.gameObject == null)
                return false;

            return eventData.pointerCurrentRaycast.module is GraphicRaycaster;
        }

        public void SetSelectedGameObject(GameObject selected)
        {
        }
    }
}