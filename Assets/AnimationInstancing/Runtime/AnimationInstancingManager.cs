using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace AnimationInstancingModule.Runtime
{
    public class AnimationInstancingManager : SingletonMono<AnimationInstancingManager>
    {
        static public int                               s_MaxInstanceCountPerRendering  = 2;            // 一次最多可渲染的实例化数量
        private Dictionary<int, VertexCache>            m_VertexCachePool               = new Dictionary<int, VertexCache>();
        private List<AnimationInstancing>               m_AnimInstancingList            = new List<AnimationInstancing>();

        private BoundingSphere[]                        m_BoundingSphere;
        private int                                     m_UsedBoundingSphereCount;
        private CullingGroup                            m_CullingGroup;
        public bool                                     useGPUInstancing                { get; set; } = false;

        protected override void Awake()
        {
            base.Awake();
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
            m_BoundingSphere = new BoundingSphere[5000];
            m_CullingGroup = new CullingGroup();
            m_CullingGroup.targetCamera = Camera.main;
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
                    m_AnimInstancingList[evt.index].visible = true;
                }
            }
            if (evt.hasBecomeInvisible)
            {
                Debug.Assert(evt.index < m_AnimInstancingList.Count);
                m_AnimInstancingList[evt.index].visible = false;
            }
        }

        private void AddBoundingSphere(AnimationInstancing inst)
        {
            m_BoundingSphere[m_UsedBoundingSphereCount++] = inst.boundingSphere;
            m_CullingGroup.SetBoundingSphereCount(m_UsedBoundingSphereCount);
            inst.visible = m_CullingGroup.IsVisible(m_UsedBoundingSphereCount - 1);
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


        public void AddInstance(AnimationInstancing inst)
        {
#if UNITY_EDITOR
            if(m_AnimInstancingList.Contains(inst))
                Debug.LogError($"{inst.gameObject.name} has already exist.");
#endif            

            m_AnimInstancingList.Add(inst);
            AddBoundingSphere(inst);
        }

        public void RemoveInstance(AnimationInstancing inst)
        {
            RemoveAllVertexCache(inst);
            if(m_AnimInstancingList.Remove(inst))
            {
                RemoveBoundingSphere();                
            }
        }

        public void AddVertexCache(AnimationInstancing inst, LODInfo lodInfo)
        {
            foreach(var rendererCache in lodInfo.rendererCacheList)
            {
                if(rendererCache.isUsed)
                    continue;

                VertexCache vertexCache = GetOrCreateVertexCache(inst, rendererCache);
                rendererCache.vertexCache = vertexCache;
                rendererCache.materialBlock = GetOrCreateMaterialBlock(vertexCache, rendererCache);
                rendererCache.isUsed = true;
            }
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
                        vertexCache.onGetAnimTexture -= inst.animDataInst.GetAnimTexture;
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
            int nameHash = rendererCache.mesh.name.GetHashCode();
            VertexCache vertexCache;
            if(!m_VertexCachePool.TryGetValue(nameHash, out vertexCache))
            {
                // construct VertexCache
                vertexCache = new VertexCache();
                vertexCache.nameHash = nameHash;
                vertexCache.mesh = rendererCache.mesh;
                vertexCache.weights = GetBoneWeights(rendererCache.mesh, rendererCache.bonePerVertex);
                vertexCache.boneIndices = rendererCache.boneIndices;
                vertexCache.blockWidth = inst.animDataInst.textureBlockWidth;
                vertexCache.blockHeight = inst.animDataInst.textureBlockHeight;
                vertexCache.shadowCastingMode = inst.shadowCastingMode;
                vertexCache.receiveShadows = inst.receiveShadows;
                vertexCache.layer = inst.layer;
                m_VertexCachePool.Add(nameHash, vertexCache);

                // upload mesh data to gpu
                // todo: 能否把weights，boneIndices等数据序列化至mesh，这样mesh就无需打开read/write enable
                UploadMeshData(vertexCache);
            }
            vertexCache.onGetAnimTexture += inst.animDataInst.GetAnimTexture;
            ++vertexCache.refCount;
            return vertexCache;
        }

        private MaterialBlock GetOrCreateMaterialBlock(VertexCache vertexCache, RendererCache rendererCache)
        {
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
            ++materialBlock.refCount;
            return materialBlock;
        }

        private void SetupMaterialBlockPropertyIfNeed(MaterialBlock block, VertexCache vertexCache)
        {
            if(block.isInitMaterial || vertexCache.GetAnimTexture() == null)
                return;

            block.isInitMaterial = true;
            for(int i = 0; i < block.subMeshCount; ++i)
            {
                block.materials[i].SetTexture("_boneTexture", vertexCache.GetAnimTexture());
                block.materials[i].SetInt("_boneTextureWidth", vertexCache.GetAnimTexture().width);
                block.materials[i].SetInt("_boneTextureHeight", vertexCache.GetAnimTexture().height);
                block.materials[i].SetInt("_boneTextureBlockWidth", vertexCache.blockWidth);
                block.materials[i].SetInt("_boneTextureBlockHeight", vertexCache.blockHeight);
            }
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

        private void UploadMeshData(VertexCache vertexCache)
        {
            Color[] colors = new Color[vertexCache.weights.Length];            
            for (int i = 0; i != colors.Length; ++i)
            {
                colors[i].r = vertexCache.weights[i].x;
                colors[i].g = vertexCache.weights[i].y;
                colors[i].b = vertexCache.weights[i].z;
                colors[i].a = vertexCache.weights[i].w;
            }
            vertexCache.mesh.colors = colors;
            vertexCache.mesh.SetUVs(2, vertexCache.boneIndices);
            vertexCache.mesh.UploadMeshData(false);
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
            for(int i = 0; i < m_AnimInstancingList.Count; ++i)
            {
                AnimationInstancing inst = m_AnimInstancingList[i];
                inst.UpdateAnimation();

                // update boundingSphere
                UpdateBoundingSphere(inst, i);

                if(!inst.ShouldRender())
                    continue;

                inst.UpdateLod();

                LODInfo lodInfo = inst.GetCurrentLODInfo();
                foreach(var rendererCache in lodInfo.rendererCacheList)
                {
                    MaterialBlock materialBlock = rendererCache.materialBlock;
                    Debug.Assert(materialBlock != null);

                    ++materialBlock.instancingCount;

                    int packageIndex = (materialBlock.instancingCount - 1) / s_MaxInstanceCountPerRendering;
                    InstancingPackage package = null;
                    if(materialBlock.packageList.Count < packageIndex + 1)
                    {
                        package = new InstancingPackage();
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
        }

        private void Render()
        {
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

                    // 因有资源异步加载，在轮询中检测
                    SetupMaterialBlockPropertyIfNeed(materialBlock, vertexCache);
                    if(!materialBlock.isInitMaterial)
                        continue;       // 资源仍未加载

                    if(useGPUInstancing)
                    {
                        for(int i = 0; i < materialBlock.packageList.Count; ++i)
                        {
                            InstancingPackage package = materialBlock.packageList[i];
                            for (int j = 0; j < materialBlock.subMeshCount; ++j)
                            {
                                materialBlock.propertyBlocks[j].SetFloatArray("frameIndex", package.frameIndex);
                                materialBlock.propertyBlocks[j].SetFloatArray("preFrameIndex", package.preFrameIndex);
                                materialBlock.propertyBlocks[j].SetFloatArray("transitionProgress", package.transitionProgress);
#if UNITY_EDITOR
                                // 编辑模式下不设置camera，方便所有窗口可见
                                Graphics.DrawMeshInstanced(vertexCache.mesh,
                                                           j,
                                                           materialBlock.materials[j],
                                                           package.worldMatrix,
                                                           package.count,
                                                           materialBlock.propertyBlocks[j],
                                                           vertexCache.shadowCastingMode,
                                                           vertexCache.receiveShadows,
                                                           vertexCache.layer,
                                                           null);
#else
                                Graphics.DrawMeshInstanced(vertexCache.mesh,
                                                           j,
                                                           materialBlock.materials[j],
                                                           package.worldMatrix,
                                                           package.count,
                                                           materialBlock.propertyBlocks[j],
                                                           vertexCache.shadowCastingMode,
                                                           vertexCache.receiveShadows,
                                                           vertexCache.layer,
                                                           Camera.main);
#endif
                            }
                            package.count = 0;      // reset
                        }
                    }
                    else
                    {
                        for(int i = 0; i < materialBlock.packageList.Count; ++i)
                        {
                            InstancingPackage package = materialBlock.packageList[i];
                            for (int j = 0; j < materialBlock.subMeshCount; ++j)
                            {
                                materialBlock.propertyBlocks[j].SetFloat("frameIndex", package.frameIndex[i]);
                                materialBlock.propertyBlocks[j].SetFloat("preFrameIndex", package.preFrameIndex[i]);
                                materialBlock.propertyBlocks[j].SetFloat("transitionProgress", package.transitionProgress[i]);
#if UNITY_EDITOR
                                // 编辑模式下不设置camera，方便所有窗口可见
                                Graphics.DrawMesh(vertexCache.mesh,
                                                  package.worldMatrix[i],
                                                  materialBlock.materials[j],
                                                  vertexCache.layer,
                                                  null,
                                                  j,
                                                  materialBlock.propertyBlocks[j],
                                                  vertexCache.shadowCastingMode,
                                                  vertexCache.receiveShadows);
#else
                                Graphics.DrawMesh(vertexCache.mesh,
                                                  package.worldMatrix[i],
                                                  materialBlock.materials[j],
                                                  vertexCache.layer,
                                                  Camera.main,
                                                  j,
                                                  materialBlock.propertyBlocks[j],
                                                  vertexCache.shadowCastingMode,
                                                  vertexCache.receiveShadows);
#endif
                            }
                            package.count = 0;      // reset
                        }
                    }
                }
            }
        }
    }
}