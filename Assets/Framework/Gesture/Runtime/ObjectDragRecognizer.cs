using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Gesture.Runtime
{
    [RequireComponent(typeof(PlayerInput))]
    public class ObjectDragRecognizer : ContinuousGestureRecognizer<IObjectDragHandler, ObjectDragEventData>
    {
        private PlayerInput m_PlayerInput;
        private PlayerInput PlayerInput
        {
            get
            {
                if(m_PlayerInput == null)
                {
                    m_PlayerInput = gameObject.GetComponent<PlayerInput>();
                }
                return m_PlayerInput;
            }
        }
        
        internal override void InternalUpdate()
        {
            if(InputModule != null)
            {
                m_EventData.ClearPointerDatas();
                foreach(var data in InputModule.screenPointerData)
                {
                    m_EventData.AddPointerData(data.Value);
                }
            }
            base.InternalUpdate();
        }
        
        protected override bool CanBegin()
        {
            if(InputModule == null || PlayerInput == null)
                return false;
                
            if(m_EventData.PointerEventData.Count != requiredPointerCount)
                return false;

            if(PlayerInput.currentSelectedGameObject != null &&
                PlayerInput.currentSelectedGameObject == m_EventData[0].pointerEventData.pointerPressRaycast.gameObject)
                return true;

            return false;
        }

        protected override RecognitionState OnProgress()
        {
            if(m_EventData.pointerCount != requiredPointerCount)
                return RecognitionState.Ended;

            if(PlayerInput.currentSelectedGameObject == null)
                return RecognitionState.Ended;

            m_EventData.Position = m_EventData.GetAveragePosition(requiredPointerCount);
            m_EventData.SetUsedBy(UsedBy.ObjectDrag);

            GestureEvents.ExecuteContinous<IObjectDragHandler, ObjectDragEventData>(PlayerInput.currentSelectedGameObject, m_EventData);
            return RecognitionState.InProgress;
        }

        protected override GameObject eventTarget { get { return PlayerInput?.currentSelectedGameObject; } }
    }
    
    public class ObjectDragEventData : GestureEventData
    {
    }

    public interface IObjectDragHandler : IContinuousGestureHandler<ObjectDragEventData>
    {
    }
}