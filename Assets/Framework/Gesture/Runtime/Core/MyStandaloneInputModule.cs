using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.Gesture.Runtime
{
    public class MyStandaloneInputModule : StandaloneInputModule
    {
        // collection of PointerEventData for screen gesture recognizer
        public class ScreenPointerEventData
        {
            public PointerEventData pointerData;
            public bool used;
        }
        protected Dictionary<int, ScreenPointerEventData> m_ScreenPointerData = new Dictionary<int, ScreenPointerEventData>();

        public ScreenPointerEventData GetScreenPointerEventData(PointerEventData data)
        {
            ScreenPointerEventData eventData;
            if(!m_ScreenPointerData.TryGetValue(data.pointerId, out eventData))
            {
                eventData = new ScreenPointerEventData();
                eventData.pointerData = data;
                m_ScreenPointerData.Add(data.pointerId, eventData);
            }
            return eventData;
        }

        public void RemoveGesturePointerEventData(PointerEventData data)
        {
            m_ScreenPointerData.Remove(data.pointerId);
        }

        public Dictionary<int, ScreenPointerEventData> screenPointerEventData
        {
            get { return m_ScreenPointerData; }
            internal set { m_ScreenPointerData = value; }
        }



        private bool m_wasMouseScrolling;
        private bool m_isMouseScrolling;
        public float mouseScrollingDelta { get; private set; }
        public bool isPinchFetch { get; internal set; }

        public override void Process()
        {
            base.Process();

            if (!eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus())
                return;

            UpdateScreenPointerData();

            // 为了整合IEventSystemHandler和Gesture，消息优先IEventSystemHandler处理，然后是IGestureHandler
            GestureRecognizerManager.Update();
        }

        private void UpdateScreenPointerData()
        {
            if(!CollectTouchScreenPointerData() && input.mousePresent)
                CollectMouseScreenPointerData();
        }

        private bool CollectTouchScreenPointerData()
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
                        // AddUnusedPointerEventData(ref unusedPointerData, eventData);
                    }
                }
                else if(released || eventData.used)
                {
                    // RemoveUnusedPointerEventData(ref unusedPointerData, touch.fingerId);
                }
            }
            return Input.touchCount > 0;
        }

        private void CollectMouseScreenPointerData()
        {}





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

            if(isPinchFetch)         // ugly code, Pinch只需监听mouse scroll
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
                mouseScrollingDelta = Input.mouseScrollDelta.y;

                if(m_isMouseScrolling)
                {
                    if(!IsPointerOverUI(eventData))
                    {
                        AddUnusedPointerEventData(ref unusedPointerData, GetLastPointerEventData(kMouseLeftId));
                        AddUnusedPointerEventData(ref unusedPointerData, GetLastPointerEventData(kMouseRightId));
                    }
                }
                if(m_wasMouseScrolling && !m_isMouseScrolling)
                {
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
            
            unusedPointerData.Add(eventData.pointerId, eventData);
        }

        private void RemoveUnusedPointerEventData(ref Dictionary<int, PointerEventData> unusedPointerData, int id)
        {
            if(!unusedPointerData.ContainsKey(id))
                return;

            unusedPointerData.Remove(id);
        }

        static internal bool IsPointerOverUI(PointerEventData eventData)
        {
            if(eventData.pointerCurrentRaycast.gameObject == null)
                return false;

            return eventData.pointerCurrentRaycast.module is GraphicRaycaster;
        }

        // [trick] make the api public for ScreenPointerUpRecognizer
        internal PointerEventData GetPointerEventData(int id)
        {
            return GetLastPointerEventData(id);
        }
    }
}