using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationInstancingModule.Runtime
{
    public class AnimationInfo
    {
        public string               name;
        public int                  nameHash;
        public int                  totalFrame;
        public int                  fps;
        public int                  animationIndex;
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
}