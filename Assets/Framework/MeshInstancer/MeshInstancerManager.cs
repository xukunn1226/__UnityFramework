using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Framework.Core
{
    /// <summary>
    /// 1. 静态物体
    /// 2. 仅含一个submesh
    /// </summary>
    [ExecuteAlways]
    public class MeshInstancerManager : MonoBehaviour
    {
        private Camera          m_Camera;
        public MeshInstancer    instancer;

        private void OnEnable()
        {
            m_Camera = Camera.main;
            instancer.Start();
        }

        private void OnDisable()
        {
            instancer.Dispose();
        }

        void Update()
        {
#if UNITY_EDITOR
            Camera finalCam = null;
#else
            Camera finalCam = m_Camera;
#endif

            instancer.Render(finalCam);
        }

        [ContextMenu("Fake AddInstance")]
        private void FakeAddInstance()
        {
            instancer.AddInstance(UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(0.1f, 5), UnityEngine.Random.rotation, Vector3.one);
        }

        [ContextMenu("Fake ClearInstances")]
        private void FakeClearInstances()
        {
            instancer.ClearInstances();
        }
    }

    [Serializable]
    public class MeshInstancer : IDisposable
    {
        public Mesh                     mesh;
        public Material                 material;
        public ShadowCastingMode        shadowCastingMode   = ShadowCastingMode.On;
        public bool                     receiveShadows      = true;
        public ComputeShader            cullingShader;

        private int                     m_CachedInstanceCount;
        private ComputeBuffer           m_ArgsBuffer;
        private uint[]                  m_Args              = new uint[5] { 0, 0, 0, 0, 0 };
        private ComputeBuffer           m_MeshPropertiesBuffer;
        private List<MeshProperties>    m_CachedProperties  = new List<MeshProperties>();
        private ComputeBuffer           m_VisibleInstances;
        private int                     m_cullingKernel     = -1;
        private Bounds                  m_Bounds;
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
            m_cullingKernel = cullingShader != null ? cullingShader.FindKernel("CSMain") : -1;
            m_ArgsBuffer = new ComputeBuffer(1, m_Args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            UpdateBuffer();
        }

        public void Dispose()
        {
            if (m_ArgsBuffer != null)
                m_ArgsBuffer.Release();
            m_ArgsBuffer = null;

            if (m_VisibleInstances != null)
                m_VisibleInstances.Release();
            m_VisibleInstances = null;

            if (m_MeshPropertiesBuffer != null)
                m_MeshPropertiesBuffer.Release();
            m_MeshPropertiesBuffer = null;
        }

        public void Render(Camera camera)
        {
            if (!m_bStarted)
                return;

            UpdateBuffer();

            ExecCullingShader();

            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, m_Bounds, m_ArgsBuffer, 0, null, shadowCastingMode, receiveShadows, 0, camera);
        }

        public void AddInstance(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            m_CachedProperties.Add(new MeshProperties() { matrix = Matrix4x4.TRS(pos, rot, scale) });

            m_Bounds.Encapsulate(pos);
        }

        public void ClearInstances()
        {
            m_CachedProperties.Clear();
        }

        private void UpdateBuffer()
        {
            if(m_CachedInstanceCount != m_CachedProperties.Count)
            {
                m_CachedInstanceCount = m_CachedProperties.Count;

                // update Args buffer                
                if (mesh != null)
                {
                    m_Args[0] = (uint)mesh.GetIndexCount(0);
                    m_Args[1] = (uint)m_CachedProperties.Count;
                    m_Args[2] = (uint)mesh.GetIndexStart(0);
                    m_Args[3] = (uint)mesh.GetBaseVertex(0);
                }
                else
                {
                    m_Args[0] = m_Args[1] = m_Args[2] = m_Args[3] = 0;
                }
                m_ArgsBuffer.SetData(m_Args);

                // update MeshProperties buffer
                if (m_MeshPropertiesBuffer != null)
                {
                    m_MeshPropertiesBuffer.Release();
                }
                m_MeshPropertiesBuffer = new ComputeBuffer(Mathf.Max(1, m_CachedProperties.Count), MeshProperties.size);
                m_MeshPropertiesBuffer.SetData(m_CachedProperties);

                // update VisibleInstances buffer
                if (cullingShader != null)
                {
                    if(m_VisibleInstances != null)
                        m_VisibleInstances.Release();
                    m_VisibleInstances = new ComputeBuffer(Mathf.Max(1, m_CachedProperties.Count), sizeof(uint), ComputeBufferType.Append);
                }

                // bind buffer to compute shader and material
                if (cullingShader != null)
                {
                    cullingShader.SetBuffer(m_cullingKernel, "_Properties", m_MeshPropertiesBuffer);
                    cullingShader.SetBuffer(m_cullingKernel, "_VisibleInstances", m_VisibleInstances);
                }

                material.SetBuffer("_Properties", m_MeshPropertiesBuffer);
            }
        }

        private void ExecCullingShader()
        {
            if (cullingShader == null || m_VisibleInstances == null)
                return;

            m_VisibleInstances.SetCounterValue(0);
            cullingShader.Dispatch(m_cullingKernel, Mathf.Max(1, Mathf.CeilToInt((m_CachedInstanceCount * 1.0f)/64)), 1, 1);
            ComputeBuffer.CopyCount(m_VisibleInstances, m_ArgsBuffer, 4);
        }
    }
}