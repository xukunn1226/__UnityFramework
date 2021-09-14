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
        public float                    HeightOfDive;           // 俯冲到的高度（relative to GroundZ）
        public float                    timeOfPan;
        public EasingFunction           easingFunctionOfPan;
        public float                    timeOfDive;
        public EasingFunction           easingFunctionOfDive;
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
                SetLimitedBound(Bound);
            else
                SetUnlimitedBound();
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
                PlayerInput.HitEventData eventData = PlayerInput.Instance.hitEventData;
                eventData.hitInfo = hitInfo;
                PlayerInput.Instance.SetSelectedGameObject(hitInfo.transform.gameObject, eventData);
            }
        }

        private void OnGesture(ScreenLongPressEventData eventData)
        {
            PickGameObject(eventData.screenPosition);
        }

        private void OnGesture(ScreenPointerUpEventData eventData)
        {
            PickGameObject(eventData.screenPosition);

            // ref readonly RaycastHit hitInfo = ref Raycast(eventData.screenPosition, TerrainLayer | BaseLayer);
            // if (hitInfo.transform != null)
            // {
            //     ((WorldPlayerController)GameInfoManager.playerController).Pan(hitInfo.point);
            // }
        }

        public void SetLimitedBound(Rect bound)
        {
            ApplyBound = true;
            Bound = bound;
            virtualCamera.SetLimitedBound(bound);
        }

        public void SetUnlimitedBound()
        {
            ApplyBound = false;
            virtualCamera.SetUnlimitedBound();
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

        // 屏幕中心点的世界坐标
        public Vector3 GetCenterHitPoint()
        {
            return GetGroundHitPoint(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        }

        // 从起始点沿镜头方向到水平面的距离
        public float GetDistanceToGround(Vector3 start)
        {
            Ray ray = new Ray(start, cameraForward);
            float distance;
            m_Ground.Raycast(ray, out distance);
            return distance;
        }

        // 计算一屏的水平距离
        public float CalcLengthOfOneScreen()
        {
            Vector3 l = GetGroundHitPoint(new Vector2(0, Screen.height * 0.5f));
            Vector3 r = GetGroundHitPoint(new Vector2(Screen.width, Screen.height * 0.5f));
            return (r - l).magnitude;
        }
        
        public void PanCamera(Vector3 targetPos, System.Action callback = null)
        {
            Vector3 center = GetGroundHitPoint(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
            Vector3 delta;
            if(Mathf.Approximately(center.y, targetPos.y))
            {
                delta = targetPos - center;
            }
            else
            {
                Ray ray = new Ray(targetPos, cameraForward * -1);
                Plane cam = new Plane(Vector3.up, cameraPos);
                float distance;
                cam.Raycast(ray, out distance);
                delta = ray.GetPoint(distance) - cameraPos;
            }
            delta.y = 0;
            
            DoCameraRush(cameraPos, cameraPos + delta, timeOfPan, easingFunctionOfPan, callback);
        }

        public void DiveCameraToBase(System.Action callback = null)
        {
            DiveCamera(GetDistanceToGround(cameraPos) - GetDistanceToGround(new Vector3(0, GroundZ + HeightOfDive, 0)), callback);
        }

        public void DiveCamera(float delta, System.Action callback = null)
        {
            DoCameraRush(cameraPos, cameraPos + cameraForward * delta, timeOfDive, easingFunctionOfDive, callback);
        }

        private void DoCameraRush(Vector3 start, Vector3 end, float time, EasingFunction easingFunction, System.Action callback = null)
        {
            virtualCamera.AddEasingEvent(new PositionEasingEvent(start, end, time, easingFunction, callback));
        }
    }
}

