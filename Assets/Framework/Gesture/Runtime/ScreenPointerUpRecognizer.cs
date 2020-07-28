using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public class ScreenPointerUpRecognizer : DiscreteGestureRecognizer<IScreenPointerUpHandler, ScreenPointerUpEventData>
    {
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

        internal override void InternalUpdate()
        {
            // collect gesture event data
            if(InputModule != null)
            {
                InputModule.UpdateUnusedEventData(ref m_UnusedPointerData);
                m_EventData.PointerEventData = m_UnusedPointerData;
            }
            base.InternalUpdate();
        }

        protected override RecognitionState OnProgress()
        {
            return RecognitionState.Ended;
        }
    }

    public class ScreenPointerUpEventData : GestureEventData
    {}

    public interface IScreenPointerUpHandler : IDiscreteGestureHandler<ScreenPointerUpEventData>
    {}
}