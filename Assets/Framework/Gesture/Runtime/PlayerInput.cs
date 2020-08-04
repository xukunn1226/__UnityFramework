using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Gesture.Runtime
{
    public class PlayerInput :  MonoBehaviour, 
                                IScreenDragHandler,
                                IScreenPinchHandler,
                                ILongPressHandler,
                                IScreenPointerDownHandler,
                                IScreenPointerUpHandler
    {
        static public PlayerInput Instance { get; private set; }
        public Camera eventCamera;
        
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
        private int m_TerrainLayer;
        private int m_BaseLayer;

        void Awake()
        {
            if(eventCamera == null)
                throw new System.Exception("Missing event camera");

            // 已有PlayerInput，则自毁
            if (FindObjectsOfType<PlayerInput>().Length > 1)
            {
                DestroyImmediate(this);
                throw new System.Exception("PlayerInput has already exist...");
            }

            Instance = this;

            m_TerrainLayer = LayerMask.NameToLayer("Terrain");
            m_BaseLayer = LayerMask.NameToLayer("Base");
        }

        void OnDestroy()
        {
            Instance = null;
        }

        public void SetSelectedGameObject(GameObject selected, HitEventData eventData)
        {
            if (m_SelectionGuard)
            {
                Debug.LogError("Attempting to select " + selected +  "while already selecting an object.");
                return;
            }

            m_SelectionGuard = true;
            if (selected == m_CurrentSelected)
            {
                m_SelectionGuard = false;
                return;
            }

            ExecuteEvents.Execute(m_CurrentSelected, eventData, ExecuteEvents.deselectHandler);
            m_CurrentSelected = selected;
            ExecuteEvents.Execute(m_CurrentSelected, eventData, ExecuteEvents.selectHandler);
            m_SelectionGuard = false;
        }

        public void SetSelectedGameObject(GameObject selected)
        {
            SetSelectedGameObject(selected, hitEventData);
        }

        public void OnGesture(ScreenDragEventData eventData)
        {
            // Debug.Log($"Drag.........{eventData.State}   {eventData.Position}    {eventData.DeltaMove}   {Time.frameCount}");

            // screenDragData = eventData;
            switch (eventData.State)
            {
                case RecognitionState.Started:
                    // isScreenDragging = true;
                    break;
                case RecognitionState.Failed:
                case RecognitionState.Ended:
                    // isScreenDragging = false;
                    break;
            }
        }

        public void OnGesture(ScreenPinchEventData eventData)
        {
            Debug.Log($"Pinch..........{eventData.State}   {eventData.Position}    {eventData.DeltaMove}    {Time.frameCount}");

            // screenPinchData = eventData;
            switch(eventData.State)
            {
                case RecognitionState.Started:
                    // isScreenPinching = true;
                    break;
                case RecognitionState.Failed:
                case RecognitionState.Ended:
                    // isScreenPinching = false;
                    break;
            }
        }

        public void OnGesture(ScreenLongPressEventData eventData)
        {
            Debug.Log($"LongPress..........{eventData.State}   {eventData.screenPosition}    {Time.frameCount}");
            PickGameObject(eventData.screenPosition);
        }

        public void OnGesture(ScreenPointerDownEventData eventData)
        {
            Debug.Log($"ScreenPointerDownEventData:       {Time.frameCount}");
        }
        public void OnGesture(ScreenPointerUpEventData eventData)
        {
            Debug.Log($"ScreenPointerUpEventData:       {Time.frameCount}");
            PickGameObject(eventData.screenPosition);
        }

        private void PickGameObject(Vector2 screenPosition)
        {
            Ray ray = eventCamera.ScreenPointToRay(screenPosition);

            RaycastHit hitInfo = new RaycastHit();
            Physics.Raycast(ray, out hitInfo, Mathf.Infinity, 1 << m_TerrainLayer | 1 << m_BaseLayer);

            if(hitInfo.transform != null)
            {
                hitEventData.hitInfo = hitInfo;
                SetSelectedGameObject(hitInfo.transform.gameObject, hitEventData);
            }
        }
    }
}