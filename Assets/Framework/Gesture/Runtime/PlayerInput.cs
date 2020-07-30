using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Gesture.Runtime
{
    public class PlayerInput : MonoBehaviour, IScreenDragHandler, IPinchHandler, IScreenPointerDownHandler, IScreenPointerUpHandler
    {
        static public PlayerInput Instance { get; private set; }
        
        private GameObject m_CurrentSelected;           // 自定义的当前选中对象，不同于EventSystem.m_CurrentSelected
        public GameObject currentSelectedGameObject
        {
            get { return m_CurrentSelected; } 
        }

        public bool isScreenDragging { get; private set; }
        public ScreenDragEventData screenDragData { get; private set; }

        public bool isScreenPinching { get; private set; }
        public PinchEventData screenPinchData { get; private set; }

        void Awake()
        {
            // 已有PlayerInput，则自毁
            if (FindObjectsOfType<PlayerInput>().Length > 1)
            {
                DestroyImmediate(this);
                throw new System.Exception("PlayerInput has already exist...");
            }

            Instance = this;
        }

        void OnDestroy()
        {
            Instance = null;
        }

        public void SetSelectedGameObject(GameObject selected)
        {
            m_CurrentSelected = selected;
        }

        public void OnGesture(ScreenDragEventData eventData)
        {
            Debug.Log($"Drag.........{eventData.State}   {eventData.Position}    {eventData.DeltaMove}   {Time.frameCount}");

            screenDragData = eventData;
            switch (eventData.State)
            {
                case RecognitionState.Started:
                    isScreenDragging = true;
                    break;
                case RecognitionState.Failed:
                case RecognitionState.Ended:
                    isScreenDragging = false;
                    break;
            }
        }

        public void OnGesture(PinchEventData eventData)
        {
            Debug.Log($"Pinch..........{eventData.State}   {eventData.Position}    {eventData.DeltaMove}    {Time.frameCount}");

            screenPinchData = eventData;
            switch(eventData.State)
            {
                case RecognitionState.Started:
                    isScreenPinching = true;
                    break;
                case RecognitionState.Failed:
                case RecognitionState.Ended:
                    isScreenPinching = false;
                    break;
            }
        }

        public void OnGesture(ScreenPointerEventData eventData)
        {
            if(isScreenDragging || isScreenPinching)
                return;

            Debug.Log($"Screen Pointer EventData: {eventData.bPressed}      {Time.frameCount}");
        }
    }
}