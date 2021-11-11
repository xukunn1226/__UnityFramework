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
        public int                  nameHash;                           // 非序列化数据，在烘焙时记录AnimationState.nameHash，运行时记录AnimationInfo.name.GetHashCode
        public int                  totalFrame;
        public int                  fps;
        public int                  startFrameIndex;                    // 在整个AnimationTexture中的起始帧序号
        public WrapMode             wrapMode;
        public List<AnimationEvent> eventList;
        public Dictionary<string, Matrix4x4[]>    extraBoneMatrix;      // length = count of extra bone; Matrix4x4's length == totalFrame
    }

    // 记录挂点骨骼在所有动画序列中的矩阵数据
    public class ExtraBoneInfo
    {
        public string               boneName;
        public List<Matrix4x4[]>    boneMatrix;                         // [animationInfo index][animation frame index]
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

    // 能被挂载到AnimationInstancing对象上的物体，需要继承此接口
    public interface IAttachmentToInstancing
    {
        string                  name            { get; set; }
        AnimationInstancing     owner           { get; set; }
        string                  extraBoneName   { get; set; }
        void SetParent(Transform parent);
        void SetPosition(Vector3 pos);
        void SetRotation(Quaternion rot);
        void Detach();
        void Attach(AnimationInstancing owner, string extraBoneName);
    }

    public class AttachmentInfo
    {
        static public int                   s_MaxCountAttachment    = 3;                                        // 性能考虑，一个挂点最多挂载一定数量的对象
        public string                       boneName;
        public IAttachmentToInstancing[]    attachments             = new IAttachmentToInstancing[s_MaxCountAttachment];
        public int                          count                   { get; private set; }                       // 有效挂载对象的数量
        public ExtraBoneInfo                extraBoneInfo;
        
        private int FindValidIndex(IAttachmentToInstancing attachment)
        {
#if UNITY_EDITOR
            for(int i = 0; i < attachments.Length; ++i)
            {
                if(attachments[i] != null && attachments[i] == attachment)
                {
                    Debug.LogError($"duplicated attachment: {attachment.name}");
                    return -1;
                }
            }
#endif
            for(int i = 0; i < s_MaxCountAttachment; ++i)
            {
                if(attachments[i] == null)
                    return i;
            }
            Debug.LogError($"too much more attachments, more than {s_MaxCountAttachment}");
            return -1;
        }

        private int FindIndex(IAttachmentToInstancing attachment)
        {
            for(int i = 0; i < s_MaxCountAttachment; ++i)
            {
                if(attachments[i] != null && attachments[i] == attachment)
                    return i;
            }
            return -1;
        }

        public int AddAttachment(IAttachmentToInstancing attachment)
        {
            int index = FindValidIndex(attachment);
            Debug.Assert(index != -1);
            if(index != -1)
            {
                attachments[index] = attachment;
                ++count;
                return index;
            }
            return -1;
        }

        public void RemoveAttachment(IAttachmentToInstancing attachment)
        {
            int index = FindIndex(attachment);
            if(index == -1)
            {
                Debug.LogError($"failed to remove attachment, because can't find {attachment.name}");
                return;
            }
            RemoveAttachment(index);
        }

        public void RemoveAttachment(int index)
        {
            Debug.Assert(index >= 0 && index < s_MaxCountAttachment && attachments[index] != null);
            attachments[index] = null;
            --count;
        }
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
        public Mesh                             mesh;                   // SkinnedMeshRenderer.mesh
        public Material[]                       materials;              // materials.Length == mesh.subMeshCount        
        public int                              bonePerVertex   = 4;    // 每顶点受多少骨骼影响
        [NonSerialized] public VertexCache      vertexCache;
        [NonSerialized] public MaterialBlock    materialBlock;
        [NonSerialized] public bool             isUsed;
    }

    public class VertexCache
    {
        public int                              nameHash;               // mesh name's hash
        public Mesh                             mesh;
        public Dictionary<int, MaterialBlock>   matBlockList            = new Dictionary<int, MaterialBlock>();      // 同一个mesh可能搭配不同材质使用  key:  hash code of mesh's name + materials' name
        public int                              blockWidth;
        public int                              blockHeight;
        public Texture2D                        animTexture;
        public ShadowCastingMode                shadowCastingMode;
        public bool                             receiveShadows;
        public int                              layer;
        public int                              refCount;               // 不同的实例(AnimationInstancing)可能共用VertexCache，故做引用计数管理
    }

    public class MaterialBlock
    {
        public int                              subMeshCount;
        public Material[]                       materials;
        public MaterialPropertyBlock[]          propertyBlocks;         // length == materials.length
        public int                              instancingCount;        // 总的实例化数量值
        public List<InstancingPackage>          packageList;
        public int                              refCount;               // 
        public bool                             isInitMaterial;         // 是否
        public delegate void                    propertyBlockHandler(int materialIndex, MaterialPropertyBlock block);
        public event propertyBlockHandler       onOverridePropertyBlock;
        public void                             ExecutePropertyBlock(int materialIndex, MaterialPropertyBlock block)
        {
            onOverridePropertyBlock?.Invoke(materialIndex, block);
        }
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