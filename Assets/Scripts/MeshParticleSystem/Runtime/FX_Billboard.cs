using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
{
    public class FX_Billboard : MonoBehaviour
    {
        void OnWillRenderObject()
        {
            if (Camera.current == null)
                return;

            Vector3 vecUp;
            vecUp = Camera.current.transform.rotation * Vector3.up;

            transform.LookAt(transform.position + Camera.current.transform.rotation * Vector3.back, vecUp);
            transform.Rotate(transform.up, 180, Space.World);
        }
    }
}