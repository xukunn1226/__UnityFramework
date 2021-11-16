using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace AnimationInstancingModule.Runtime
{
    /// <summary>
    /// manager of all animation instancing
    /// <summary>
    public class AnimationInstancingManager : SingletonMono<AnimationInstancingManager>
    {
        static private int                              s_MaxInstanceCountPerRendering  = 512;            // 一次最多可渲染的实例化数量
        static private int                              s_MaxCullingCount               = 5000;
        private Dictionary<int, VertexCache>            m_VertexCachePool               = new Dictionary<int, VertexCache>();
        private List<AnimationInstancing>               m_AnimInstancingList            = new List<AnimationInstancing>();

        private BoundingSphere[]                        m_BoundingSphere;
        private int                                     m_UsedBoundingSphereCount;
        private CullingGroup                            m_CullingGroup;
        public bool                                     useGPUInstancing                { get; set; } = true;
        private Camera                                  m_TargetCamera;
        public Camera                                   targetCamera
        {
            get { return m_TargetCamera; }
            set
            {
                if(value != null)
                {
                    m_TargetCamera = value;
                }
            }
        }

        static public int                               maxInstanceCountPerRendering
        {
            get { return s_MaxInstanceCountPerRendering; }
            set
            {
                if(s_MaxInstanceCountPerRendering != value)
                {
                    s_MaxInstanceCountPerRendering = Mathf.ClosestPowerOfTwo(Mathf.Clamp(s_MaxInstanceCountPerRendering, 1, 1024)) - 1;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            targetCamera = Camera.main;
            
            InitCullingGroup();
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2)
            {
                useGPUInstancing = false;
            }
        }

        protected override void OnDestroy()
        {
            UninitCullingGroup();
            base.OnDestroy();
        }

        private void InitCullingGroup()
        {
            m_BoundingSphere = new BoundingSphere[s_MaxCullingCount];
            m_CullingGroup = new CullingGroup();
            m_CullingGroup.targetCamera = targetCamera;
            m_CullingGroup.onStateChanged = CullingStateChanged;
            m_CullingGroup.SetBoundingSpheres(m_BoundingSphere);
            m_UsedBoundingSphereCount = 0;
            m_CullingGroup.SetBoundingSphereCount(m_UsedBoundingSphereCount);
        }

        private void UninitCullingGroup()
        {
            m_CullingGroup.Dispose();
            m_CullingGroup = null;
        }

        private void CullingStateChanged(CullingGroupEvent evt)
        {
            Debug.Assert(evt.index < m_UsedBoundingSphereCount);
            if (evt.hasBecomeVisible)
            {
                Debug.Assert(evt.index < m_AnimInstancingList.Count);
                if (m_AnimInstancingList[evt.index].isActiveAndEnabled)
                {
                    m_AnimInstancingList[evt.index].isCulled = false;
                }
            }
            if (evt.hasBecomeInvisible)
            {
                Debug.Assert(evt.index < m_AnimInstancingList.Count);
                m_AnimInstancingList[evt.index].isCulled = true;
            }
        }

        private void AddBoundingSphere(AnimationInstancing inst)
        {
            m_BoundingSphere[m_UsedBoundingSphereCount++] = inst.boundingSphere;
            m_CullingGroup.SetBoundingSphereCount(m_UsedBoundingSphereCount);
            inst.isCulled = !m_CullingGroup.IsVisible(m_UsedBoundingSphereCount - 1);
        }

        private void RemoveBoundingSphere()
        {
            --m_UsedBoundingSphereCount;
            m_CullingGroup.SetBoundingSphereCount(m_UsedBoundingSphereCount);
            Debug.Assert(m_UsedBoundingSphereCount >= 0);
        }

        private void UpdateBoundingSphere(AnimationInstancing inst, int index)
        {
            inst.boundingSphere.position = inst.worldTransform.position;
            m_BoundingSphere[index] = inst.boundingSphere;
        }

        internal void AddInstance(AnimationInstancing inst)
        {
#if UNITY_EDITOR
            if(m_AnimInstancingList.Contains(inst))
                Debug.LogError($"{inst.gameObject.name} has already exist.");
#endif            

            m_AnimInstancingList.Add(inst);
            AddBoundingSphere(inst);
        }

        internal void RemoveInstance(AnimationInstancing inst)
        {
            if(m_AnimInstancingList.Remove(inst))
            {
                RemoveAllVertexCache(inst);
                RemoveBoundingSphere();                
            }
#if UNITY_EDITOR            
            else
            {
                Debug.LogError($"{inst.gameObject.name} remove from AnimationInstancingManager fail.");
            }
#endif            
        }

        internal void AddVertexCache(AnimationInstancing inst, LODInfo lodInfo)
        {
            UnityEngine.Profiling.Profiler.BeginSample("AnimationInstancingManager::AddVertexCache");
            foreach(var rendererCache in lodInfo.rendererCacheList)
            {
                if(rendererCache.isUsed)
                    continue;

                VertexCache vertexCache = GetOrCreateVertexCache(inst, rendererCache);
                rendererCache.materialBlock = GetOrCreateMaterialBlock(inst, vertexCache, rendererCache);
                rendererCache.isUsed = true;
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }

        // 删除注册过的所有VertexCache
        private void RemoveAllVertexCache(AnimationInstancing inst)
        {
            foreach(var lodInfo in inst.lodInfos)
            {
                foreach(var rendererCache in lodInfo.rendererCacheList)
                {
                    if(!rendererCache.isUsed)
                        continue;

                    VertexCache vertexCache;
                    int nameHash = rendererCache.mesh.name.GetHashCode();
                    m_VertexCachePool.TryGetValue(nameHash, out vertexCache);
                    Debug.Assert(vertexCache != null);
                    // remove MaterialBlock
                    {
                        int materialsHashCode = GetMaterialsHashCode(rendererCache.mesh, rendererCache.materials);
                        MaterialBlock materialBlock;
                        vertexCache.matBlockList.TryGetValue(materialsHashCode, out materialBlock);
                        Debug.Assert(materialBlock != null);
                        materialBlock.onOverridePropertyBlock -= inst.ExecutePropertyBlock;
                        --materialBlock.refCount;
                        if(materialBlock.refCount == 0)
                        {
                            foreach(var package in materialBlock.packageList)
                            {
                                ArrayPool<Matrix4x4>.Release(package.worldMatrix);
                                ArrayPool<float>.Release(package.frameIndex);
                                ArrayPool<float>.Release(package.preFrameIndex);
                                ArrayPool<float>.Release(package.transitionProgress);
                            }
                            
                            vertexCache.matBlockList.Remove(materialsHashCode);
                        }
                    }

                    // remove VertexCache
                    {
                        --vertexCache.refCount;
                        if(vertexCache.refCount == 0)
                        {
                            m_VertexCachePool.Remove(nameHash);
                        }
                    }
                }
            }
        }

        private VertexCache GetOrCreateVertexCache(AnimationInstancing inst, RendererCache rendererCache)
        {
            UnityEngine.Profiling.Profiler.BeginSample("AnimationInstancingManager::GetOrCreateVertexCache");
            int nameHash = rendererCache.mesh.name.GetHashCode();
            VertexCache vertexCache;
            if(!m_VertexCachePool.TryGetValue(nameHash, out vertexCache))
            {
                UnityEngine.Profiling.Profiler.BeginSample("AnimationInstancingManager::Internal_GetOrCreateVertexCache");
                // construct VertexCache
                vertexCache = new VertexCache();
                vertexCache.nameHash = nameHash;
                vertexCache.mesh = rendererCache.mesh;
                vertexCache.blockWidth = inst.animDataInst.textureBlockWidth;
                vertexCache.blockHeight = inst.animDataInst.textureBlockHeight;
                vertexCache.animTexture = inst.animDataInst.animTexture;
                vertexCache.shadowCastingMode = inst.shadowCastingMode;
                vertexCache.receiveShadows = inst.receiveShadows;
                vertexCache.layer = inst.layer;
                m_VertexCachePool.Add(nameHash, vertexCache);

                UnityEngine.Profiling.Profiler.EndSample();
            }
            // vertexCache.onGetAnimTexture += inst.animDataInst.GetAnimTexture;
            ++vertexCache.refCount;
            UnityEngine.Profiling.Profiler.EndSample();
            return vertexCache;
        }

        private MaterialBlock GetOrCreateMaterialBlock(AnimationInstancing inst, VertexCache vertexCache, RendererCache rendererCache)
        {
            UnityEngine.Profiling.Profiler.BeginSample("AnimationInstancingManager::GetOrCreateMaterialBlock");
            int materialsHashCode = GetMaterialsHashCode(rendererCache.mesh, rendererCache.materials);
            MaterialBlock materialBlock;
            if(!vertexCache.matBlockList.TryGetValue(materialsHashCode, out materialBlock))
            {
                materialBlock = new MaterialBlock();
                materialBlock.subMeshCount = rendererCache.mesh.subMeshCount;
                materialBlock.materials = rendererCache.materials;
                materialBlock.propertyBlocks = new MaterialPropertyBlock[rendererCache.materials.Length];
                for(int i = 0; i < rendererCache.materials.Length; ++i)
                {
                    materialBlock.propertyBlocks[i] = new MaterialPropertyBlock();
                }
                materialBlock.instancingCount       = 0;
                materialBlock.packageList           = new List<InstancingPackage>();
                vertexCache.matBlockList.Add(materialsHashCode, materialBlock);

                SetupMaterialBlockPropertyIfNeed(materialBlock, vertexCache);
            }
            materialBlock.onOverridePropertyBlock += inst.ExecutePropertyBlock;
            ++materialBlock.refCount;
            UnityEngine.Profiling.Profiler.EndSample();
            return materialBlock;
        }

        private void SetupMaterialBlockPropertyIfNeed(MaterialBlock block, VertexCache vertexCache)
        {
            for(int i = 0; i < block.subMeshCount; ++i)
            {
                block.materials[i].SetTexture("_boneTexture", vertexCache.animTexture);
                block.materials[i].SetInt("_boneTextureWidth", vertexCache.animTexture.width);
                block.materials[i].SetInt("_boneTextureHeight", vertexCache.animTexture.height);
                block.materials[i].SetInt("_boneTextureBlockWidth", vertexCache.blockWidth);
                block.materials[i].SetInt("_boneTextureBlockHeight", vertexCache.blockHeight);
                if(!useGPUInstancing)
                {
                    block.materials[i].DisableKeyword("INSTANCING_ON");
                    block.materials[i].enableInstancing = false;
                }
                // block.materials[i].EnableKeyword("USE_CONSTANT_BUFFER");
                // block.materials[i].EnableKeyword("USE_COMPUTE_BUFFER");
            }
        }

        private Vector4[] GetBoneIndices(Mesh mesh)
        {
            Vector4[] boneIndices = new Vector4[mesh.vertexCount];
            BoneWeight[] boneWeights = mesh.boneWeights;
            for(int i = 0; i < mesh.vertexCount; ++i)
            {
                boneIndices[i].x = boneWeights[i].boneIndex0;
                boneIndices[i].y = boneWeights[i].boneIndex1;
                boneIndices[i].z = boneWeights[i].boneIndex2;
                boneIndices[i].w = boneWeights[i].boneIndex3;

                Debug.Log($"index[{i}]   {boneIndices[i].x}  {boneIndices[i].y}  {boneIndices[i].z}  {boneIndices[i].w}");
            }
            return boneIndices;
        }
        
        private Vector4[] GetBoneWeights(Mesh mesh, int bonePerVertex)
        {
            Vector4[] weights = new Vector4[mesh.vertexCount];
            BoneWeight[] boneWeights = mesh.boneWeights;
            for(int i = 0; i < mesh.vertexCount; ++i)
            {
                weights[i].x = boneWeights[i].weight0;
                weights[i].y = boneWeights[i].weight1;
                weights[i].z = boneWeights[i].weight2;
                weights[i].w = boneWeights[i].weight3;

                // if((boneWeights[i].weight0 != 0 && boneWeights[i].weight0 != 1)
                // || (boneWeights[i].weight1 != 0 && boneWeights[i].weight1 != 1)
                // || (boneWeights[i].weight2 != 0 && boneWeights[i].weight2 != 1)
                // || (boneWeights[i].weight3 != 0 && boneWeights[i].weight3 != 1))
                // {
                //     ++count;
                //     Debug.Log($"index[{i}]  {boneWeights[i].weight0}    {boneWeights[i].weight1}    {boneWeights[i].weight2}    {boneWeights[i].weight3}");
                // }

                switch (bonePerVertex)
                {
                    case 3:
                        {
                            float scale = 1.0f / (weights[i].x + weights[i].y + weights[i].z);
                            weights[i].x = weights[i].x * scale;
                            weights[i].y = weights[i].y * scale;
                            weights[i].z = weights[i].z * scale;
                            weights[i].w = -0.1f;
                        }
                        break;
                    case 2:
                        {
                            float scale = 1.0f / (weights[i].x + weights[i].y);
                            weights[i].x = weights[i].x * scale;
                            weights[i].y = weights[i].y * scale;
                            weights[i].z = -0.1f;
                            weights[i].w = -0.1f;
                        }
                        break;
                    case 1:
                        {
                            weights[i].x = 1.0f;
                            weights[i].y = -0.1f;
                            weights[i].z = -0.1f;
                            weights[i].w = -0.1f;
                        }
                        break;
                }
            }

            return weights;
        }

        private int GetMaterialsHashCode(Mesh mesh, Material[] materials)
        {
            string combinedName = mesh.name;
            for(int i = 0; i < materials.Length; ++i)
            {
                combinedName += string.Format($"_{materials[i].name}");
            }
            return combinedName.GetHashCode();
        }
        
        private void Update()
        {
            UpdateInstancing();
            Render();
        }

        private void UpdateInstancing()
        {
            UnityEngine.Profiling.Profiler.BeginSample("AnimationInstancingManager::UpdateInstancing()");
            for(int i = 0; i < m_AnimInstancingList.Count; ++i)
            {
                AnimationInstancing inst = m_AnimInstancingList[i];
                inst.UpdateAnimation();

                // update boundingSphere
                UpdateBoundingSphere(inst, i);

                if(!inst.ShouldRender())
                    continue;

                // 不需渲染时可以不用更新lod
                inst.UpdateLod(targetCamera.transform.position);

                LODInfo lodInfo = inst.GetCurrentLODInfo();
                foreach(var rendererCache in lodInfo.rendererCacheList)
                {
                    MaterialBlock materialBlock = rendererCache.materialBlock;
                    Debug.Assert(materialBlock != null);

                    // // 因有资源异步加载，在轮询中检测
                    // SetupMaterialBlockPropertyIfNeed(materialBlock, rendererCache.vertexCache);
                    // if(!materialBlock.isInitMaterial)
                    //     continue;       // 资源仍未加载

                    ++materialBlock.instancingCount;

                    int packageIndex = (materialBlock.instancingCount - 1) / s_MaxInstanceCountPerRendering;
                    InstancingPackage package = null;
                    if(materialBlock.packageList.Count < packageIndex + 1)
                    {
                        package                     = new InstancingPackage();
                        package.count               = 0;
                        package.worldMatrix         = ArrayPool<Matrix4x4>.Get(s_MaxInstanceCountPerRendering);
                        package.frameIndex          = ArrayPool<float>.Get(s_MaxInstanceCountPerRendering);
                        package.preFrameIndex       = ArrayPool<float>.Get(s_MaxInstanceCountPerRendering);
                        package.transitionProgress  = ArrayPool<float>.Get(s_MaxInstanceCountPerRendering);
                        materialBlock.packageList.Add(package);
                    }
                    else
                    {
                        package = materialBlock.packageList[packageIndex];
                    }
                    ++package.count;
                    package.worldMatrix[package.count - 1]          = inst.worldTransform.localToWorldMatrix;
                    package.frameIndex[package.count - 1]           = inst.GetGlobalCurFrameIndex();
                    package.preFrameIndex[package.count - 1]        = inst.GetGlobalPreFrameIndex();
                    package.transitionProgress[package.count - 1]   = inst.transitionProgress;
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }

        private void Render()
        {
            UnityEngine.Profiling.Profiler.BeginSample("AnimationInstancingManager::Render()");
            foreach(var obj in m_VertexCachePool)
            {
                VertexCache vertexCache = obj.Value;
                foreach(var obj1 in vertexCache.matBlockList)
                {
                    MaterialBlock materialBlock = obj1.Value;

                    int instancingCount = materialBlock.instancingCount;
                    materialBlock.instancingCount = 0;      // 清空计数，UpdateInstancing重新统计
                    
                    if(instancingCount == 0)
                        continue;

                    if(useGPUInstancing)
                    {
                        for(int i = 0; i < materialBlock.packageList.Count; ++i)
                        {
                            InstancingPackage package = materialBlock.packageList[i];
                            for (int j = 0; j < materialBlock.subMeshCount; ++j)
                            {
                                materialBlock.propertyBlocks[j].Clear();
                                materialBlock.propertyBlocks[j].SetFloatArray("frameIndex", package.frameIndex);
                                materialBlock.propertyBlocks[j].SetFloatArray("preFrameIndex", package.preFrameIndex);
                                materialBlock.propertyBlocks[j].SetFloatArray("transitionProgress", package.transitionProgress);
                                materialBlock.ExecutePropertyBlock(j, materialBlock.propertyBlocks[j]);

                                Graphics.DrawMeshInstanced(vertexCache.mesh,
                                                           j,
                                                           materialBlock.materials[j],
                                                           package.worldMatrix,
                                                           package.count,
                                                           materialBlock.propertyBlocks[j],
                                                           vertexCache.shadowCastingMode,
                                                           vertexCache.receiveShadows,
                                                           vertexCache.layer,
#if UNITY_EDITOR
                                                           null             // 编辑模式下不设置camera，方便所有窗口可见
#else
                                                           targetCamera
#endif
                                                           );
                            }
                            package.count = 0;      // reset
                        }
                    }
                    else
                    {
                        for(int i = 0; i < materialBlock.packageList.Count; ++i)
                        {
                            InstancingPackage package = materialBlock.packageList[i];
                            for(int j = 0; j < package.count; ++j)
                            {
                                for (int k = 0; k < materialBlock.subMeshCount; ++k)
                                {
                                    materialBlock.propertyBlocks[k].SetFloat("frameIndex", package.frameIndex[j]);
                                    materialBlock.propertyBlocks[k].SetFloat("preFrameIndex", package.preFrameIndex[j]);
                                    materialBlock.propertyBlocks[k].SetFloat("transitionProgress", package.transitionProgress[j]);

                                    Graphics.DrawMesh(vertexCache.mesh,
                                                      package.worldMatrix[j],
                                                      materialBlock.materials[k],
                                                      vertexCache.layer,
#if UNITY_EDITOR                                                      
                                                      null,             // 编辑模式下不设置camera，方便所有窗口可见
#else
                                                      targetCamera,
#endif                                                      
                                                      k,
                                                      materialBlock.propertyBlocks[k],
                                                      vertexCache.shadowCastingMode,
                                                      vertexCache.receiveShadows);
                                }
                            }
                            package.count = 0;      // reset
                        }
                    }
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
}