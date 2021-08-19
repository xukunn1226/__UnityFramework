using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace AnimationInstancingModule.Runtime
{
    public class AnimationInstancingManager : SingletonMono<AnimationInstancingManager>
    {
        private const int                               kMaxInstanceCount           = 256;          // 最大允许创建的实例化数量
        static public int                               sMaxRenderingInstanceCount  = 128;          // 一次最多可渲染的实例化数量
        private Dictionary<int, VertexCache>            m_VertexCachePool           = new Dictionary<int, VertexCache>();
        private Dictionary<int, AnimationInstancing>    m_AnimInstancingList        = new Dictionary<int, AnimationInstancing>();

        public void AddInstance(AnimationInstancing inst)
        {
            if(!m_AnimInstancingList.ContainsKey(inst.GetInstanceID()))
            {
                m_AnimInstancingList.Add(inst.GetInstanceID(), inst);
            }
        }

        public void RemoveInstance(AnimationInstancing inst)
        {
            RemoveAllVertexCache(inst);
            m_AnimInstancingList.Remove(inst.GetInstanceID());
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
                            ArrayPool<Matrix4x4>.Release(materialBlock.worldMatrix);
                            ArrayPool<float>.Release(materialBlock.frameIndex);
                            ArrayPool<float>.Release(materialBlock.preFrameIndex);
                            ArrayPool<float>.Release(materialBlock.transitionProgress);
                            
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
                vertexCache.onGetAnimTexture += inst.animDataInst.GetAnimTexture;
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
                materialBlock.worldMatrix           = ArrayPool<Matrix4x4>.Get(kMaxInstanceCount);
                materialBlock.frameIndex            = ArrayPool<float>.Get(kMaxInstanceCount);
                materialBlock.preFrameIndex         = ArrayPool<float>.Get(kMaxInstanceCount);
                materialBlock.transitionProgress    = ArrayPool<float>.Get(kMaxInstanceCount);
                vertexCache.matBlockList.Add(materialsHashCode, materialBlock);
                
                InitMaterialBlock(materialBlock, vertexCache);
            }
            ++materialBlock.refCount;
            return materialBlock;
        }

        private void InitMaterialBlock(MaterialBlock block, VertexCache vertexCache)
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
            foreach(var obj in m_AnimInstancingList)
            {
                AnimationInstancing inst = obj.Value;

                inst.UpdateAnimation();

                if(!inst.visible || !inst.enabled)
                    continue;

                inst.UpdateLod();

                LODInfo lodInfo = inst.GetCurrentLODInfo();
                foreach(var rendererCache in lodInfo.rendererCacheList)
                {
                    MaterialBlock materialBlock = rendererCache.materialBlock;
                    ++materialBlock.instancingCount;

                    materialBlock.worldMatrix[materialBlock.instancingCount - 1] = inst.worldTransform.localToWorldMatrix;
                    materialBlock.frameIndex[materialBlock.instancingCount - 1] = inst.GetGlobalCurFrameIndex();
                    materialBlock.preFrameIndex[materialBlock.instancingCount - 1] = inst.GetGlobalPreFrameIndex();
                    materialBlock.transitionProgress[materialBlock.instancingCount - 1] = inst.transitionProgress;
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
                    InitMaterialBlock(materialBlock, vertexCache);
                    if(!materialBlock.isInitMaterial)
                        continue;       // 资源仍未加载

                    for(int i = 0; i < materialBlock.subMeshCount; ++i)
                    {
                        materialBlock.propertyBlocks[i].SetFloatArray("frameIndex", materialBlock.frameIndex);
                        materialBlock.propertyBlocks[i].SetFloatArray("preFrameIndex", materialBlock.preFrameIndex);
                        materialBlock.propertyBlocks[i].SetFloatArray("transitionProgress", materialBlock.transitionProgress);

                        Graphics.DrawMeshInstanced(vertexCache.mesh,
                                                   i,
                                                   materialBlock.materials[i],
                                                   materialBlock.worldMatrix,
                                                   instancingCount,
                                                   materialBlock.propertyBlocks[i],
                                                   vertexCache.shadowCastingMode,
                                                   vertexCache.receiveShadows,
                                                   vertexCache.layer);
                    }
                    materialBlock.instancingCount = 0;
                }
            }
        }
    }
}