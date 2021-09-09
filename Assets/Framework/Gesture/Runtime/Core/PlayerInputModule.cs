using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.Gesture.Runtime
{
    public class PlayerInputModule : StandaloneInputModule
    {
        private Dictionary<int, ScreenPointerData> m_ScreenPointerData = new Dictionary<int, ScreenPointerData>();

        public Dictionary<int, ScreenPointerData> screenPointerData { get { return m_ScreenPointerData; } }

        private List<int> m_pendingRemoveItem = new List<int>();
        
        private bool m_wasMouseScrolling;
        private bool m_isMouseScrolling;
        public float mouseScrollingDelta { get; private set; }

        public override void Process()
        {
            base.Process();

            if (!eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus())
            {
                // release gesture event
                GestureRecognizerManager.Release();
                ClearScreenPointerData();
                return;
            }

            // 收集、更新ScreenPointerData
            if(!ProcessScreenTouchEventData() && Input.mousePresent)
                ProcessScreenMouseEventData();

            GestureRecognizerManager.Update();

            // 删除ScreenPointerData
            ReleaseScreenPointer();
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
                    return true;
            }
        }

        private bool ProcessScreenTouchEventData()
        {
            for(int i = 0; i < Input.touchCount; ++i)
            {
                Touch touch = Input.touches[i];
                bool pressed = touch.phase == TouchPhase.Began;
                bool released = (touch.phase == TouchPhase.Canceled) || (touch.phase == TouchPhase.Ended);

                if(pressed)
                {
                    PointerEventData eventData = GetLastPointerEventData(touch.fingerId);
                    if(!IsPointerOverUI(eventData))
                    {
                        ScreenPointerData data = AddScreenPointerData(eventData);
                        data.bPressedThisFrame = true;
                        data.bReleasedThisFrame = false;
                    }
                }
                else
                {
                    ScreenPointerData data = GetData(touch.fingerId);
                    if(data != null)
                    {
                        data.bPressedThisFrame = false;
                        data.bReleasedThisFrame = released;

                        if(released)
                            m_pendingRemoveItem.Add(touch.fingerId);
                    }
                }
            }
            return Input.touchCount > 0;
        }

        private void ProcessScreenMouseEventData()
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
                PointerEventData eventData = GetLastPointerEventData(kMouseLeftId);
                if(!IsPointerOverUI(eventData))
                {
                    ScreenPointerData mouseLeftData = AddScreenPointerData(GetLastPointerEventData(kMouseLeftId));
                    mouseLeftData.bPressedThisFrame = false;
                    mouseLeftData.bReleasedThisFrame = false;
                    
                    ScreenPointerData mouseRightData = AddScreenPointerData(GetLastPointerEventData(kMouseRightId));
                    mouseRightData.bPressedThisFrame = false;
                    mouseRightData.bReleasedThisFrame = false;

                    // Debug.Log($"Pinch:  {Time.frameCount}   ");
                }
            }
            else if(m_wasMouseScrolling && !m_isMouseScrolling)
            {
                ScreenPointerData mouseLeftData = GetData(kMouseLeftId);
                if(mouseLeftData != null)
                {
                    mouseLeftData.bPressedThisFrame = false;
                    mouseLeftData.bReleasedThisFrame = true;

                    m_pendingRemoveItem.Add(kMouseLeftId);
                }

                ScreenPointerData mouseRightData = GetData(kMouseRightId);
                if(mouseRightData != null)
                {
                    mouseRightData.bPressedThisFrame = false;
                    mouseRightData.bReleasedThisFrame = true;

                    m_pendingRemoveItem.Add(kMouseRightId);

                    // Debug.Log($"------------Pinch: {Time.frameCount}");
                }
            }
            else
            {
                bool pressed = input.GetMouseButtonDown(0);
                bool released = input.GetMouseButtonUp(0);
                if(pressed)
                {
                    PointerEventData eventData = GetLastPointerEventData(kMouseLeftId);
                    if(!IsPointerOverUI(eventData))
                    {
                        ScreenPointerData data = AddScreenPointerData(eventData);
                        data.bPressedThisFrame = true;
                        data.bReleasedThisFrame = false;
                        // Debug.Log($"-----------{Time.frameCount}");
                    }
                }
                else
                {
                    ScreenPointerData data = GetData(kMouseLeftId);
                    if(data != null)
                    {
                        data.bPressedThisFrame = false;
                        data.bReleasedThisFrame = released;

                        if(released)
                        {
                            m_pendingRemoveItem.Add(kMouseLeftId);
                            // Debug.Log($"{Time.frameCount}================");
                        }
                    }
                }
            }
        }

        private void ReleaseScreenPointer()
        {
            foreach(var id in m_pendingRemoveItem)
            {
                RemoveScreenPointerData(id);
            }
            m_pendingRemoveItem.Clear();
        }

        private ScreenPointerData AddScreenPointerData(PointerEventData eventData)
        {
            ScreenPointerData data;
            if(m_ScreenPointerData.TryGetValue(eventData.pointerId, out data))
                return data;

            data = new ScreenPointerData() { pointerEventData = eventData, usedBy = UsedBy.None, startTime = Time.time };
            m_ScreenPointerData.Add(eventData.pointerId, data);
            return data;
        }

        private void RemoveScreenPointerData(int pointerId)
        {
            if(!m_ScreenPointerData.ContainsKey(pointerId))
                return;
            m_ScreenPointerData.Remove(pointerId);
        }

        private void ClearScreenPointerData()
        {
            m_ScreenPointerData.Clear();
        }
        
        private ScreenPointerData GetData(int pointerId)
        {
            ScreenPointerData data;
            m_ScreenPointerData.TryGetValue(pointerId, out data);
            return data;
        }

        static internal bool IsPointerOverUI(PointerEventData eventData)
        {
            if(eventData.pointerCurrentRaycast.gameObject == null)
                return false;

            return eventData.pointerCurrentRaycast.module is GraphicRaycaster;
        }
    }
}