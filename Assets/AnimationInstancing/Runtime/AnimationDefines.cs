using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

namespace AnimationInstancingModule.Runtime
{
    public class AnimationInfo
    {
        public string               name;
        public int                  nameHash;               // 非序列化数据，在烘焙时记录AnimationState.nameHash，运行时记录AnimationInfo.name.GetHashCode
        public int                  totalFrame;
        public int                  fps;
        public int                  startFrameIndex;        // 在整个AnimationTexture中的起始帧序号
        public WrapMode             wrapMode;
        public List<AnimationEvent> eventList;
    }

    public class AnimationEvent
    {
        public string               function;
        public int                  intParameter;
        public float                floatParameter;
        public string               stringParameter;
        public string               objectParameter;
        public float                time;
    }

    public class ExtraBoneInfo
    {
        public string[]             extraBone;
        public Matrix4x4[]          extraBindPose;
    }

    public class ComparerHash : IComparer<AnimationInfo>
    {
        public int Compare(AnimationInfo x, AnimationInfo y)
        {
            return x.nameHash.CompareTo(y.nameHash);
        }
    }

    [System.Serializable]
    public class LODInfo
    {
        public int                              lodLevel;
        public List<RendererCache>              rendererCacheList = new List<RendererCache>();
    }

    [System.Serializable]
    public class RendererCache
    {
        public Mesh                             mesh;
        public Material[]                       materials;              // materials.Length == mesh.subMeshCount        
        public int                              bonePerVertex   = 4;    // 每顶点受多少骨骼影响
        public Vector4[]                        boneIndices;            // 顶点受哪些骨骼影响，length == mesh.vertexCount 【需要用到render.bones，所以必须预计算序列化】
        // [NonSerialized] public Vector4[]        weights;                // 顶点受4根骨骼影响的权重因子，length == mesh.vertexCount 【数据量较大，运行时计算优于序列化】
        [NonSerialized] public VertexCache      vertexCache;
        [NonSerialized] public MaterialBlock    materialBlock;
        [NonSerialized] public bool             isUsed;
    }

    public class VertexCache
    {
        public int                              nameHash;               // mesh name's hash
        public Mesh                             mesh;
        public Dictionary<int, MaterialBlock>   matBlockList            = new Dictionary<int, MaterialBlock>();      // 同一个mesh可能搭配不同材质使用  key: materials' hash code
        public Vector4[]                        weights;                // weight.Length == mesh.vertexCount
        public Vector4[]                        boneIndices;            // boneIndex.Length == mesh.vertexCount
        public int                              blockWidth;
        public int                              blockHeight;
        public ShadowCastingMode                shadowCastingMode;
        public bool                             receiveShadows;
        public int                              layer;
        public int                              refCount;               // 
        public delegate Texture2D animTextureHandler();
        public event animTextureHandler onGetAnimTexture;               // mesh对应的动画数据
        public Texture2D GetAnimTexture()
        {
            return onGetAnimTexture?.Invoke();
        }
    }

    public class MaterialBlock
    {
        public int                              subMeshCount;
        public Material[]                       materials;
        public MaterialPropertyBlock[]          propertyBlocks;         // length == materials.length
        public int                              instancingCount;
        public List<InstancingPackage>          packageList;
        public int                              refCount;               // 
        public bool                             isInitMaterial;         // 是否
    }

    public class InstancingPackage
    {
        public int                              count;
        public Matrix4x4[]                      worldMatrix;            // 所有实例的世界坐标矩阵
        public float[]                          frameIndex;             // 所有实例播放的当前帧
        public float[]                          preFrameIndex;          // 所有实例播放的上一帧
        public float[]                          transitionProgress;     // 所有实例的过渡参数
    }
}