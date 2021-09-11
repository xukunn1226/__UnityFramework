using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Gesture.Runtime;
using Framework.Core;

namespace Application.Runtime
{    
    public class WorldPlayerController : PlayerController
    {
        public WorldCamera              virtualCamera;
        private Plane                   m_Ground;               // 虚拟水平面
        [Tooltip("水平面高度")]
        public float                    GroundZ;                // 水平面高度
        public Vector2                  HeightRange;            // 相对水平面的高度区间
        public bool                     ApplyBound;
        public Rect                     Bound;
        public LayerMask                BaseLayer;
        public LayerMask                TerrainLayer;

        void Awake()
        {
            if(virtualCamera == null)
                throw new System.ArgumentNullException("worldCamera");

            m_Ground = new Plane(Vector3.up, new Vector3(0, GroundZ, 0));

            if(ApplyBound)
                ApplyLimitedBound(Bound);
            else
                UnapplyLimitedBound();
        }

        void OnEnable()
        {
            if(PlayerInput.Instance == null)
                throw new System.Exception("PlayerInput == null");

            PlayerInput.Instance.OnLongPressHandler += OnGesture;
            PlayerInput.Instance.OnScreenPointerUpHandler += OnGesture;
            
            virtualCamera.enabled = true;
        }

        void OnDisable()
        {
            if(PlayerInput.Instance != null)
            {
                PlayerInput.Instance.OnLongPressHandler -= OnGesture;
                PlayerInput.Instance.OnScreenPointerUpHandler -= OnGesture;
            }

            if (virtualCamera != null)
            {
                virtualCamera.enabled = false;
            }
        }

        private void PickGameObject(Vector2 screenPosition)
        {
            ref readonly RaycastHit hitInfo = ref Raycast(screenPosition, TerrainLayer | BaseLayer);
            if (hitInfo.transform != null)
            {
                PlayerInput.Instance.hitEventData.hitInfo = hitInfo;
                PlayerInput.Instance.SetSelectedGameObject(hitInfo.transform.gameObject, PlayerInput.Instance.hitEventData);
            }
        }

        private void OnGesture(ScreenLongPressEventData eventData)
        {
            PickGameObject(eventData.screenPosition);
        }

        private void OnGesture(ScreenPointerUpEventData eventData)
        {
            PickGameObject(eventData.screenPosition);
        }

        public void ApplyLimitedBound(Rect bound)
        {
            ApplyBound = true;
            Bound = bound;
            virtualCamera.ApplyLimitedBound(bound);
        }

        public void UnapplyLimitedBound()
        {
            ApplyBound = false;
            virtualCamera.UnapplyLimitedBound();
        }

        public Vector2 GetAbsoluteHeightRange()
        {
            return new Vector2(GroundZ + HeightRange.x, GroundZ + HeightRange.y);
        }

        // 水平面交点坐标
        public Vector3 GetGroundHitPoint(Vector2 screenPosition)
        {
            ///// method 1
            Ray mousePos = ScreenPointToRay(screenPosition);
            float distance;
            m_Ground.Raycast(mousePos, out distance);
            return mousePos.GetPoint(distance);

            ///// method 2
            // Raycast(screenPosition, TerrainLayer, ref m_HitInfo);
            // return m_HitInfo.point;
        }

        // 计算一屏的水平距离
        public float CalcLengthOfOneScreen()
        {
            Vector3 l = GetGroundHitPoint(new Vector2(0, Screen.height * 0.5f));
            Vector3 r = GetGroundHitPoint(new Vector2(Screen.width, Screen.height * 0.5f));
            return (r - l).magnitude;
        }
    }
}