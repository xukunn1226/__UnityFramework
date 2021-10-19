﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Framework.Gesture.Runtime;

namespace Application.Runtime.Tests
{
    public class TestGesture : MonoBehaviour, ISelectHandler, IDeselectHandler, IObjectDragHandler
    {
        private Vector3 m_Delta;

        public void OnSelect(BaseEventData eventData)
        {
            transform.localScale *= 2;

            PlayerInput.HitEventData hitEventData = (PlayerInput.HitEventData)eventData;
            ((WorldPlayerController)PlayerController.Instance).PanCamera(hitEventData.hitInfo.point, OnPanEnd);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            transform.localScale *= 0.5f;
        }

        private void OnPanEnd()
        {
            ((WorldPlayerController)PlayerController.Instance).DiveCameraToBase();
        }

        public void OnGesture(ObjectDragEventData eventData)
        {
            switch (eventData.State)
            {
                case RecognitionState.Started:
                    m_Delta = transform.position - ((WorldPlayerController)PlayerController.Instance).GetGroundHitPoint(eventData.Position);
                    break;
                case RecognitionState.InProgress:
                    SetDraggedPosition(eventData.Position);
                    break;
                case RecognitionState.Ended:
                case RecognitionState.Failed:
                    SetDraggedPosition(eventData.Position);
                    break;
            }
        }

        private void SetDraggedPosition(Vector2 screenPosition)
        {
            Vector3 newPos = ((WorldPlayerController)PlayerController.Instance).GetGroundHitPoint(screenPosition);
            transform.position = newPos + m_Delta;
        }
    }
}