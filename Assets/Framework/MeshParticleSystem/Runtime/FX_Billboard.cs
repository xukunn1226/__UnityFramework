using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.MeshParticleSystem
{
    [ExecuteInEditMode]
    public class FX_Billboard : MonoBehaviour
    {
        public enum BillboardMode
        {
            Billboard,
            StretchedBillboard,
            HorizontalBillboard,
            VerticalBillboard,
        }

        public BillboardMode Mode;

        private Transform m_Cam;

        private Transform cam
        {
            get
            {
#if UNITY_EDITOR
                if(UnityEditor.EditorApplication.isPlaying)
                {
                    if (m_Cam == null && Camera.main != null)
                    {
                        m_Cam = Camera.main.transform;
                    }
                }
                else
                {
                    if(m_Cam == null || UnityEditor.SceneView.lastActiveSceneView.camera.transform != m_Cam)
                    {
                        m_Cam = UnityEditor.SceneView.lastActiveSceneView.camera.transform;
                    }
                }
#else
                if(m_Cam == null && Camera.main != null)
                {
                    m_Cam = Camera.main.transform;
                }
#endif
                return m_Cam;
            }
        }

        private void Update()
        {
            if (cam == null)
                return;

            switch(Mode)
            {
                case BillboardMode.Billboard:
                    {
                        Vector3 forward = (cam.position - transform.position).normalized;
                        Vector3 right = Vector3.Cross(Vector3.up, forward);
                        Vector3 up = Vector3.Cross(forward, right);
                        Quaternion rot = Quaternion.LookRotation(forward, up);
                        transform.rotation = rot;
                    }
                    break;
                case BillboardMode.StretchedBillboard:
                    {
                        Vector3 right = Vector3.forward;
                        Vector3 forward = (cam.position - transform.position).normalized;
                        float cosValue = Vector3.Dot(right, forward);
                        Vector3 normal = forward - right * cosValue;
                        transform.rotation = Quaternion.LookRotation(normal, Vector3.Cross(normal, Vector3.forward));
                    }
                    break;
                case BillboardMode.HorizontalBillboard:
                    {
                        transform.rotation = Quaternion.identity * Quaternion.AngleAxis(-90, Vector3.right);
                    }
                    break;
                case BillboardMode.VerticalBillboard:
                    {
                        Vector3 forward = cam.position - transform.position;
                        forward.y = 0;
                        forward.Normalize();
                        Vector3 right = Vector3.Cross(Vector3.up, forward);
                        Vector3 up = Vector3.Cross(forward, right);
                        Quaternion rot = Quaternion.LookRotation(forward, up);
                        transform.rotation = rot;
                    }
                    break;
            }
        }
    }
}