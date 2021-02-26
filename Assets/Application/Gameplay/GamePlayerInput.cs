using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Gesture.Runtime;

namespace Application.Runtime
{
    [RequireComponent(typeof(PlayerInput))]
    public class GamePlayerInput :  MonoBehaviour,
                                    ILongPressHandler,
                                    IScreenPointerDownHandler,
                                    IScreenPointerUpHandler
    {
        static public GamePlayerInput   Instance { get; private set; }

        private PlayerInput             m_PlayerInput;
        private PlayerInput             playerInput { get { if (m_PlayerInput == null) m_PlayerInput = GetComponent<PlayerInput>(); return m_PlayerInput; } }

        // private RaycastHit              m_HitInfo       = new RaycastHit();
        // private ref RaycastHit          m_HitInfoRef    => ref m_HitInfo;

        public LayerMask                BaseLayer;
        public LayerMask                TerrainLayer;

        void Awake()
        {
            if (FindObjectsOfType<GamePlayerInput>().Length > 1)
            {
                DestroyImmediate(this);
                throw new System.Exception("GamePlayerInput has already exist...");
            }

            if (playerInput == null)
                throw new System.ArgumentNullException("playerInput");

            Instance = this;
        }

        void OnDestroy()
        {
            Instance = null;
        }

        private void PickGameObject(Vector2 screenPosition)
        {
            ref readonly RaycastHit hitInfo = ref GamePlayerCamera.Raycast(screenPosition, TerrainLayer | BaseLayer);
            if (hitInfo.transform != null)
            {
                playerInput.hitEventData.hitInfo = hitInfo;
                playerInput.SetSelectedGameObject(hitInfo.transform.gameObject, playerInput.hitEventData);
            }
        }

        public void OnGesture(ScreenLongPressEventData eventData)
        {
            // Debug.Log($"LongPress..........{eventData.State}   {eventData.screenPosition}    {Time.frameCount}");
            PickGameObject(eventData.screenPosition);
        }

        public void OnGesture(ScreenPointerDownEventData eventData)
        {
            // Debug.Log($"ScreenPointerDownEventData:       {Time.frameCount}");
        }
        public void OnGesture(ScreenPointerUpEventData eventData)
        {
            // Debug.Log($"ScreenPointerUpEventData:       {Screen.width}  {Screen.height}");
            PickGameObject(eventData.screenPosition);
        }

        public GameObject currentSelectedGameObject { get { return playerInput?.currentSelectedGameObject; } }
    }
}