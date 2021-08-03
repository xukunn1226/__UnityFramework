using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationInstancingModule.Runtime
{
    public class AnimationInfo
    {
        public string               name;
        public int                  nameHash;               // 非序列化数据，在烘焙时记录AnimationState.nameHash，运行时记录AnimationInfo.name.GetHashCode
        public int                  totalFrame;
        public int                  fps;
        public int                  animationIndex;         // 在整个AnimationTexture中的起始帧序号
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

    [System.Serializable]
    public class RendererCache
    {
        public Mesh         mesh;
        public Material[]   materials;      // materials.Length == mesh.subMeshCount
    }
}