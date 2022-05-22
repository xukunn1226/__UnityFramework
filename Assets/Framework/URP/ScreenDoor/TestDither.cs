using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Framework.URP
{
    public class TestDither : MonoBehaviour
    {
        private MeshRenderer m_Renderer;
        [Range(0, 1)]
        public float Transparency = 0.8f;

        private void OnEnable()
        {
            m_Renderer = GetComponent<MeshRenderer>();
            m_Renderer.sharedMaterial.EnableKeyword("ENABLE_DITHER");
            m_Renderer.sharedMaterial.SetFloat("_Transparency", Transparency);
        }

        private void OnDisable()
        {
            m_Renderer.sharedMaterial.DisableKeyword("ENABLE_DITHER");
        }

        private void Update()
        {
            if(m_Renderer != null)
            {
                m_Renderer.sharedMaterial.SetFloat("_Transparency", Transparency);
            }
        }
    }
}