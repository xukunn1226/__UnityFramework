using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    [RequireComponent(typeof(ScreenPointerDownRecognizer))]
    [RequireComponent(typeof(ScreenPointerUpRecognizer))]
    [RequireComponent(typeof(LongPressRecognizer))]
    [RequireComponent(typeof(ObjectDragRecognizer))]
    [RequireComponent(typeof(ScreenDragRecognizer))]
    [RequireComponent(typeof(ScreenPinchRecognizer))]
    public class PlayerInput :  MonoBehaviour,
                                IScreenPointerDownHandler,
                                IScreenPointerUpHandler,
                                ILongPressHandler,
                                IScreenDragHandler,
                                IScreenPinchHandler
    {
        public delegate void onScreenPointerDownHandler(ScreenPointerDownEventData eventData);
        public delegate void onScreenPointerUpHandler(ScreenPointerUpEventData eventData);
        public delegate void onLongPressHandler(ScreenLongPressEventData eventData);
        public delegate void onScreenDragHandler(ScreenDragEventData eventData);
        public delegate void onScreenPinchHandler(ScreenPinchEventData eventData);
        public event onScreenPointerDownHandler OnScreenPointerDownHandler;
        public event onScreenPointerUpHandler   OnScreenPointerUpHandler;
        public event onLongPressHandler         OnLongPressHandler;
        public event onScreenDragHandler        OnScreenDragHandler;
        public event onScreenPinchHandler       OnScreenPinchHandler;

        public class HitEventData : BaseEventData
        {
            public RaycastHit hitInfo;
            public HitEventData(EventSystem eventSystem) : base(eventSystem)
            {}

            public override void Reset()
            {
                base.Reset();
                hitInfo = default(RaycastHit);
            }
        }
        private HitEventData m_HitEventData;
        public HitEventData hitEventData
        {
            get
            {
                if(m_HitEventData == null)
                    m_HitEventData = new HitEventData(EventSystem.current);
                m_HitEventData.Reset();
                return m_HitEventData;
            }
        }

        private GameObject m_CurrentSelected;           // 自定义的当前选中对象，不同于EventSystem.m_CurrentSelected
        public GameObject currentSelectedGameObject
        {
            get { return m_CurrentSelected; } 
        }

        private bool m_SelectionGuard;
        
        public void SetSelectedGameObject(GameObject selected, HitEventData eventData)
        {
            if (m_SelectionGuard)
            {
                Debug.LogError("Attempting to select " + selected +  "while already selecting an object.");
                return;
            }

            if (selected == m_CurrentSelected)
            {
                ExecuteEvents.Execute(m_CurrentSelected, eventData, ExecuteEvents.deselectHandler);
                m_CurrentSelected = null;
                return;
            }

            m_SelectionGuard = true;
            ExecuteEvents.Execute(m_CurrentSelected, eventData, ExecuteEvents.deselectHandler);
            m_CurrentSelected = selected;
            ExecuteEvents.Execute(m_CurrentSelected, eventData, ExecuteEvents.selectHandler);
            m_SelectionGuard = false;
        }

        public void SetSelectedGameObject(GameObject selected)
        {
            SetSelectedGameObject(selected, hitEventData);
        }

        public virtual void OnGesture(ScreenPointerDownEventData eventData)
        {
            // Debug.Log($"PlayerInput.ScreenPointerDown:       {Time.frameCount}");
            OnScreenPointerDownHandler?.Invoke(eventData);
        }

        public virtual void OnGesture(ScreenPointerUpEventData eventData)
        {
            // Debug.Log($"PlayerInput.ScreenPointerUp:       {Time.frameCount}");
            OnScreenPointerUpHandler?.Invoke(eventData);
        }

        public virtual void OnGesture(ScreenLongPressEventData eventData)
        {
            // Debug.Log($"PlayerInput.LongPress  {eventData.State}   {eventData.screenPosition}    {Time.frameCount}");
            OnLongPressHandler?.Invoke(eventData);
        }

        public virtual void OnGesture(ScreenDragEventData eventData)
        {
            // Debug.Log($"PlayerInput.Drag    {eventData.State}   {eventData.Position}    {eventData.DeltaMove}   {Time.frameCount}");
            OnScreenDragHandler?.Invoke(eventData);
        }

        public virtual void OnGesture(ScreenPinchEventData eventData)
        {
            // Debug.Log($"PlayerInput.Pinch   {eventData.State}   {eventData.Position}    {eventData.DeltaMove}    {Time.frameCount}");
            OnScreenPinchHandler?.Invoke(eventData);
        }
    }
}