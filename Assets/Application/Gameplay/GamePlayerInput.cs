using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Gesture.Runtime;

namespace Application.Runtime
{
    public class GamePlayerInput :  PlayerInput
    {
        static public GamePlayerInput   Instance { get; private set; }

        public LayerMask                BaseLayer;
        public LayerMask                TerrainLayer;

        void Awake()
        {
            Instance = this;
        }

        void OnDestroy()
        {
            Instance = null;
        }
        
        private void PickGameObject(Vector2 screenPosition)
        {
            ref readonly RaycastHit hitInfo = ref WorldCamera.Raycast(screenPosition, TerrainLayer | BaseLayer);
            if (hitInfo.transform != null)
            {
                hitEventData.hitInfo = hitInfo;
                SetSelectedGameObject(hitInfo.transform.gameObject, hitEventData);
            }
        }

        public override void OnGesture(ScreenLongPressEventData eventData)
        {
            // Debug.Log($"LongPress..........{eventData.State}   {eventData.screenPosition}    {Time.frameCount}");
            base.OnGesture(eventData);

            PickGameObject(eventData.screenPosition);
        }

        public override void OnGesture(ScreenPointerUpEventData eventData)
        {
            // Debug.Log($"ScreenPointerUpEventData:       {Screen.width}  {Screen.height}");
            base.OnGesture(eventData);

            PickGameObject(eventData.screenPosition);
        }
    }
}