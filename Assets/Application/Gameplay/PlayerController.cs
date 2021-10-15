using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public class PlayerController : MonoBehaviour
    {
        private Camera      m_MainCamera;
        public Camera       mainCamera
        {
            get
            {
                if(m_MainCamera == null)
                {
                    m_MainCamera = Camera.main;
                }
                return m_MainCamera;
            }
        }

        public Vector3      cameraPos
        {
            get
            {
                return mainCamera.transform.position;
            }
        }

        public Vector3      cameraForward
        {
            get
            {
                return mainCamera.transform.forward;
            }
        }

        public Vector3      cameraEulerAngles
        {
            get
            {
                return mainCamera.transform.eulerAngles;
            }
        }

        void Awake()
        {
            if(mainCamera == null)
                throw new System.ArgumentNullException("mainCamera");
        }

        public void SetCullingMask(int mask)
        {
            mainCamera.cullingMask = mask;
        }
        
        public Ray ScreenPointToRay(Vector3 screenPosition)
        {
            return mainCamera.ScreenPointToRay(screenPosition);
        }

        public Vector3 WorldToScreenPoint(Vector3 worldPosition)
        {
            return mainCamera.WorldToScreenPoint(worldPosition);
        }

        public bool Raycast(Vector2 screenPosition, int layerMask, ref RaycastHit hitInfo)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);

            return PhysUtility.Raycast(ray, 1000, layerMask, ref hitInfo);
        }

        public ref readonly RaycastHit Raycast(Vector2 screenPosition, int layerMask)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);

            return ref PhysUtility.Raycast(ray, 1000, layerMask);
        }
    }
}