using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
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
                        Vector3 right = transform.right;
                        right.y = 0;
                        right.Normalize();
                        Vector3 forward = (cam.position - transform.position).normalized;
                        float cosValue = Vector3.Dot(right, forward);
                        right = right * cosValue * Mathf.Sign(cosValue);
                        Vector3 normal = forward - right;
                        if(cosValue > 0)
                            transform.rotation = Quaternion.LookRotation(normal, Vector3.Cross(right, normal));
                        else
                            transform.rotation = Quaternion.LookRotation(normal, Vector3.Cross(normal, right));
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