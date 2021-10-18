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
        public float                    HeightOfDive;           // 俯冲到的高度（relative to GroundZ）
        public float                    timeOfPan;              // 镜头平移时的缓动时间
        public EasingFunction           easingFunctionOfPan;    // 镜头平移的缓动模式
        public float                    timeOfDive;             // 镜头俯冲时的缓动时间
        public EasingFunction           easingFunctionOfDive;   // 镜头俯冲时的缓动模式
        public bool                     ApplyBound;
        public Rect                     Bound;
        public LayerMask                BaseLayer;
        public LayerMask                TerrainLayer;
        public float[]                  m_ViewPoints            = new float[(int)ViewLayer.ViewLayer_Max + 1];              // 每个高度点的高度值
        public Vector2[]                ViewHeights             { get; private set; }                                       // 每个区间的高度范围
        public float                    highestView             { get { return m_ViewPoints[m_ViewPoints.Length - 1]; } }
        public float                    lowestView              { get { return m_ViewPoints[0]; } }
        public ViewLayer                cameraViewLayer         { get; private set; } = ViewLayer.ViewLayer_Invalid;        // 相机所处的层级
        public float                    cameraViewLayerAlpha    { get; private set; }                                       // 相机所处层级区间的alpha值，0表示所处区间的最低处，1表示最高处

        void Awake()
        {
            if(virtualCamera == null)
                throw new System.ArgumentNullException("worldCamera");

            m_Ground = new Plane(Vector3.up, new Vector3(0, GroundZ, 0));

            if(ApplyBound)
                SetLimitedBound(Bound);
            else
                SetUnlimitedBound();

            ViewHeights = new Vector2[m_ViewPoints.Length - 1];
            for(int i = 0; i < ViewHeights.Length; ++i)
            {
                ViewHeights[i] = new Vector2(m_ViewPoints[i], m_ViewPoints[i+1]);
            }

            Instance = this;
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
            if(cameraViewLayer > ViewLayer.ViewLayer_1)
            { // 一层以上时，迅速推进镜头
                PanCamera(GetGroundHitPoint(eventData.screenPosition), () => { DiveCameraToBase(); });
            }
            else
            {
                PickGameObject(eventData.screenPosition);
            }
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
            return new Vector2(GroundZ + lowestView, GroundZ + highestView);
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

        // 平移
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
            
            InternalCameraRush(cameraPos, cameraPos + delta, timeOfPan, easingFunctionOfPan, callback);
        }

        // 俯冲至预设高度
        public void DiveCameraToBase(System.Action callback = null)
        {
            DiveCamera(GetDistanceToGround(cameraPos) - GetDistanceToGround(new Vector3(0, GroundZ + HeightOfDive, 0)), callback);
        }

        // 俯冲
        public void DiveCamera(float delta, System.Action callback = null)
        {
            InternalCameraRush(cameraPos, cameraPos + cameraForward * delta, timeOfDive, easingFunctionOfDive, callback);
        }

        // 镜头缓动
        private void InternalCameraRush(Vector3 start, Vector3 end, float time, EasingFunction easingFunction, System.Action callback = null)
        {
            virtualCamera.AddEasingEvent(new PositionEasingEvent(start, end, time, easingFunction, callback));
        }

        private void UpdateViewLayer()
        {
            float height = virtualCamera.transform.position.y;
            int layer = -1;
            for(int i = 0; i < ViewHeights.Length; ++i)
            {
                if(height > ViewHeights[i].x)
                {
                    layer = i;
                }
                else
                    break;
            }
            Debug.Assert(layer != -1);
            cameraViewLayer = (ViewLayer)layer;
            cameraViewLayerAlpha = (height - (GroundZ + ViewHeights[layer].x)) / (ViewHeights[layer].y - ViewHeights[layer].x);            
        }

        private void Update()
        {
            UpdateViewLayer();

            // ViewLayerManager.Update(Time.deltaTime);
            // LocomotionManager.Update(Time.deltaTime);
            // AISimpleManager.Update(Time.deltaTime);

            InputForDebug();
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void InputForDebug()
        {
            if(Input.GetKeyDown(KeyCode.F1))
            {
                PanCamera(Vector3.zero);
            }

            if(Input.GetKeyDown(KeyCode.Space))
            {
                UnityEngine.Debug.Log($"print center hit point: {GetCenterHitPoint()}");
            }            
        }

        private void OnGUI()
        {
            if(GUI.Button(new Rect(Screen.width-200, Screen.height/2, 200, 100), "Create Actors"))
            {
                for(int i = 0; i < 20; ++i)
                    TestActorManager.CreateActor();
            }

            if(GUI.Button(new Rect(Screen.width-200, Screen.height/2 - 120, 200, 100), "Destroy Actors"))
            {
                TestActorManager.DestroyAll();                
            }
        }
    }
}

