using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public class MyStandaloneInputModule : StandaloneInputModule
    {
        // collection of PointerEventData accord with:
        // 1. raycast nothing OR
        // 2. No Over UI & Not consumed by interactive object
        private Dictionary<int, PointerEventData> m_UnusedPointerData = new Dictionary<int, PointerEventData>();

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
                bool pressed = Input.touches[i].phase == TouchPhase.Began;
                bool released = (Input.touches[i].phase == TouchPhase.Canceled) || (Input.touches[i].phase == TouchPhase.Ended);

                if(pressed)
                {
                    // m_EventData.AddPointerData()
                }

                if(released)
                {
                    m_UnusedPointerData.Remove(Input.touches[i].fingerId);
                }
            }
            return Input.touchCount > 0;
        }

        protected void ProcessUnusedMouseEventData()
        {

        }

        public PointerEventData GetTouchPointerEventData(Touch input)
        {
            return GetPointerEventData(input.fingerId);
        }

        public PointerEventData GetPointerEventData(int id)
        {
            PointerEventData data;
            GetPointerData(id, out data, false);
            return data;
        }
    }
}