using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Gesture.Runtime;
using UnityEngine.EventSystems;

namespace Tests
{
    public class TestLongPressResponser : MonoBehaviour, ILongPressHandler, ISelectHandler, IDeselectHandler, IPointerUpHandler
    {
        public void OnGesture(LongPressEventData eventData)
        {
            if(eventData.State == RecognitionState.Ended)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }
        
        public void OnSelect(BaseEventData eventData)
        {
            transform.localScale = transform.localScale * 2;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            transform.localScale *= 0.5f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log("OnPointerUp");
        }
    }
}