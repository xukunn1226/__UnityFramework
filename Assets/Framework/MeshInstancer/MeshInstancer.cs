using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    /// <summary>
    /// 1. 静态物体
    /// 2. 仅含一个submesh
    /// </summary>
    public class MeshInstancer : MonoBehaviour
    {
        public class RenderInfo
        {
            public readonly Mesh mesh;
            public readonly Material material;

            public RenderInfo(Mesh mesh, Material material)
            {
                this.mesh = mesh;
                this.material = material;
            }
        }

        public RenderInfo       renderInfo;
        public int              instanceCount;
        private ComputeBuffer   m_ArgsBuffer;

        private void OnEnable()
        {
            if (renderInfo == null)
                throw new System.ArgumentNullException("renderInfo == null");

            instanceCount = Mathf.Max(1, instanceCount);

            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = (uint)renderInfo.mesh.GetIndexCount(0);
            args[1] = (uint)instanceCount;
            args[2] = (uint)renderInfo.mesh.GetIndexStart(0);
            args[3] = (uint)renderInfo.mesh.GetBaseVertex(0);
            m_ArgsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            m_ArgsBuffer.SetData(args);
        }

        private void OnDisable()
        {
            if (m_ArgsBuffer != null)
                m_ArgsBuffer.Release();
        }

        public void Render(Camera camera)
        {
            Graphics.DrawMeshInstancedIndirect(renderInfo.mesh, 0, renderInfo.material, new Bounds(), m_ArgsBuffer);
        }
    }
}