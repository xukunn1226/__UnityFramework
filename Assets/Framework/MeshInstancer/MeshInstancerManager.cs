using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEditor.Rendering.CameraUI;

namespace Framework.Core
{
    /// <summary>
    /// 1. 静态物体
    /// 2. 仅含一个submesh
    /// </summary>
    public class MeshInstancerManager : MonoBehaviour
    {
        public MeshInstancer    instancer;

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }
        
        void Update()
        {
            instancer?.Render(Camera.main);
        }
    }

    [Serializable]
    public class MeshInstancer : IDisposable
    {
        public Mesh                     mesh;
        public Material                 material;
        public ShadowCastingMode        shadowCastingMode   = ShadowCastingMode.On;
        public bool                     receiveShadows      = true;
        public LayerMask                layer;
        public ComputeShader            cullingShader;

        private int                     m_CachedInstanceCount;
        private ComputeBuffer           m_ArgsBuffer;
        private uint[]                  m_Args              = new uint[5] { 0, 0, 0, 0, 0 };
        private ComputeBuffer           m_MeshPropertiesBuffer;
        private List<MeshProperties>    m_CachedProperties  = new List<MeshProperties>();
        private int                     m_cullingKernel     = -1;
        private bool                    m_bStarted;

        private struct MeshProperties
        {
            public Matrix4x4 matrix;
            static public int size { get { return sizeof(float) * 16; } }
        }

        private MeshInstancer() { }

        public MeshInstancer(Mesh mesh, Material material, ComputeShader cullingShader = null)
        {
            this.mesh = mesh;
            this.material = material;
            this.cullingShader = cullingShader;

            Start();
        }

        public void Start()
        {
            if (m_bStarted)
                return;

            m_bStarted = true;
            m_ArgsBuffer = new ComputeBuffer(1, m_Args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            m_cullingKernel = cullingShader?.FindKernel("CSMain") ?? -1;
            UpdateBuffer();
        }

        public void Dispose()
        {
            m_ArgsBuffer?.Release();
            m_ArgsBuffer = null;

            m_MeshPropertiesBuffer?.Release();
            m_MeshPropertiesBuffer = null;
        }

        public void Render(Camera camera)
        {
            if (!m_bStarted)
                return;

            UpdateBuffer();

            ExecCullingShader();

            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, new Bounds(), m_ArgsBuffer, 0, null, shadowCastingMode, receiveShadows, layer.value, camera);
        }

        public void AddInstance(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            m_CachedProperties.Add(new MeshProperties() { matrix = Matrix4x4.TRS(pos, rot, scale) });
        }

        // todo
        public void RemoveInstance()
        { }

        private void UpdateBuffer()
        {
            if(m_CachedInstanceCount != m_CachedProperties.Count && m_MeshPropertiesBuffer.count > 0)
            {
                m_CachedInstanceCount = m_CachedProperties.Count;

                if (mesh != null)
                {
                    m_Args[0] = (uint)mesh.GetIndexCount(0);
                    m_Args[1] = (uint)m_MeshPropertiesBuffer.count;
                    m_Args[2] = (uint)mesh.GetIndexStart(0);
                    m_Args[3] = (uint)mesh.GetBaseVertex(0);
                }
                else
                {
                    m_Args[0] = m_Args[1] = m_Args[2] = m_Args[3] = 0;
                }
                m_ArgsBuffer.SetData(m_Args);

                m_MeshPropertiesBuffer?.Release();
                m_MeshPropertiesBuffer = new ComputeBuffer(m_MeshPropertiesBuffer.count, MeshProperties.size);
                m_MeshPropertiesBuffer.SetData(m_CachedProperties);

                // bind buffer to compute shader and material
                cullingShader?.SetBuffer(m_cullingKernel, "_Properties", m_MeshPropertiesBuffer);
                material.SetBuffer("_Properties", m_MeshPropertiesBuffer);
            }
        }

        private void ExecCullingShader()
        {
            cullingShader?.Dispatch(m_cullingKernel, Mathf.CeilToInt(m_CachedInstanceCount/64), 1, 1);
        }
    }
}