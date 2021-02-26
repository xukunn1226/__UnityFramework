using System.Collections;
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

            // PlayerInput.HitEventData data = eventData as PlayerInput.HitEventData;
            // if(data != null)
            // {
            //     Debug.Log($"HitInfo: {data.hitInfo.point}");
            // }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            transform.localScale *= 0.5f;
        }

        public void OnGesture(ObjectDragEventData eventData)
        {
            switch (eventData.State)
            {
                case RecognitionState.Started:
                    m_Delta = transform.position - GamePlayerCamera.Instance.GetGroundHitPoint(eventData.Position);
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
            Vector3 newPos = GamePlayerCamera.Instance.GetGroundHitPoint(screenPosition);
            transform.position = newPos + m_Delta;
        }
    }
}