using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace AnimationInstancingModule.Runtime
{
    [DisallowMultipleComponent]
    public class AnimationInstancingGenerator : MonoBehaviour, ISerializationCallbackReceiver
    {
        class AnimationBakeInfo
        {
            public SkinnedMeshRenderer[]                            m_Smrs;
            public Animator                                         m_Animator;
            public int                                              m_WorkingFrame;
            public float                                            m_Length;
            public int                                              m_Layer;
            public AnimationInfo                                    m_AnimationInfo;
        }

        public int                          fps                         = 15;
        public bool                         exposeAttachments;

        [SerializeField]
        private List<string>                m_ExtraBoneNames            = new List<string>();                   // 所有绑点骨骼名称列表
        [SerializeField]
        private List<bool>                  m_ExtraBoneSelectables      = new List<bool>();                     // 绑点骨骼的选中状态
        public Dictionary<string, bool>     m_SelectExtraBone           = new Dictionary<string, bool>();
        [NonSerialized]
        private ExtraBoneInfo               m_ExtraBoneInfo             = new ExtraBoneInfo();

        public Dictionary<string, bool>     m_GenerateAnims             = new Dictionary<string, bool>();       // 所有解析出的动画
        [SerializeField]
        private List<string>                m_GenerateAnimNames         = new List<string>();
        [SerializeField]
        private List<bool>                  m_GenerateAnimSelectables   = new List<bool>();
        private List<Matrix4x4>             m_BindPose                  = new List<Matrix4x4>(150);
        private List<Transform>             m_BoneTransform             = new List<Transform>();
        private List<AnimationBakeInfo>     m_BakeInfo                  = new List<AnimationBakeInfo>();
        private Dictionary<AnimationClip, UnityEngine.AnimationEvent[]> m_CacheAnimationEvent;

        public void OnBeforeSerialize()
        {
            m_ExtraBoneNames.Clear();
            m_ExtraBoneSelectables.Clear();
            foreach (var kvp in m_SelectExtraBone)
            {
                m_ExtraBoneNames.Add(kvp.Key);
                m_ExtraBoneSelectables.Add(kvp.Value);
            }

            m_GenerateAnimNames.Clear();
            m_GenerateAnimSelectables.Clear();
            foreach (var kvp in m_GenerateAnims)
            {
                m_GenerateAnimNames.Add(kvp.Key);
                m_GenerateAnimSelectables.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            m_SelectExtraBone = new Dictionary<string, bool>();
            int count = Math.Min(m_ExtraBoneNames.Count, m_ExtraBoneSelectables.Count);
            for (int i = 0; i != count; ++i)
            {
                m_SelectExtraBone.Add(m_ExtraBoneNames[i], m_ExtraBoneSelectables[i]);
            }

            m_GenerateAnims = new Dictionary<string, bool>();
            count = Math.Min(m_GenerateAnimNames.Count, m_GenerateAnimSelectables.Count);
            for (int i = 0; i != count; ++i)
            {
                m_GenerateAnims.Add(m_GenerateAnimNames[i], m_GenerateAnimSelectables[i]);
            }
        }

#if UNITY_EDITOR
        public void Bake()
        {
            Animator animator = GetComponent<Animator>();

            GetFinalBonePose(ref m_BindPose, ref m_BoneTransform);

            // init ExtraBoneInfo base on extra bone transform
            List<Transform> listExtra = GetExtraBoneTransform();
            m_ExtraBoneInfo = new ExtraBoneInfo();
            if(listExtra.Count > 0)
            {
                m_ExtraBoneInfo.extraBone = new string[listExtra.Count];
                m_ExtraBoneInfo.extraBindPose = new Matrix4x4[listExtra.Count];
                for (int i = 0; i != listExtra.Count; ++i)
                {
                    m_ExtraBoneInfo.extraBone[i] = listExtra[i].name;
                    m_ExtraBoneInfo.extraBindPose[i] = m_BindPose[m_BindPose.Count - listExtra.Count + i];
                }
            }
        }

        // get the bindPose & boneTransform base on attached points
        public void GetFinalBonePose(ref List<Matrix4x4> bindPose, ref List<Transform> boneTransform)
        {
            SkinnedMeshRenderer[] meshRender = GetComponentsInChildren<SkinnedMeshRenderer>();
            AnimationInstancingModule.Runtime.AnimationUtility.MergeBone(meshRender, ref bindPose, ref boneTransform);
            if(exposeAttachments)
            { // 如果有挂点数据，则添加至bindPose和boneTransform
                List<Transform> listExtra = GetExtraBoneTransform();
                foreach(var tran in listExtra)
                {
                    bindPose.Add(tran.localToWorldMatrix);
                }

                Transform[] totalTransform = new Transform[boneTransform.Count + listExtra.Count];
                System.Array.Copy(boneTransform.ToArray(), totalTransform, boneTransform.Count);
                System.Array.Copy(listExtra.ToArray(), 0, totalTransform, boneTransform.Count, listExtra.Count);
                boneTransform = totalTransform.ToList();
            }
        }

        // extra bone transform
        public List<Transform> GetExtraBoneTransform()
        {
            List<Transform> listExtra = new List<Transform>();
            Transform[] trans = GetComponentsInChildren<Transform>();
            foreach (var obj in m_SelectExtraBone)
            {
                if (!obj.Value)
                    continue;

                for (int i = 0; i != trans.Length; ++i)
                {
                    Transform tran = trans[i] as Transform;
                    if (tran.name == obj.Key)
                    {
                        listExtra.Add(trans[i]);
                    }
                }
            }
            return listExtra;
        }

        void AnalyzeStateMachine(AnimatorStateMachine stateMachine,
            Animator animator,
            SkinnedMeshRenderer[] meshRender,
            int layer,
            int bakeFPS,
            int animationIndex)
        {
            for (int i = 0; i != stateMachine.states.Length; ++i)
            {
                ChildAnimatorState state = stateMachine.states[i];
                AnimationClip clip = state.state.motion as AnimationClip;
                bool needBake = false;
                if (clip == null)
                    continue;
                if (!m_GenerateAnims.TryGetValue(clip.name, out needBake))
                    continue;
                foreach (var obj in m_BakeInfo)
                {
                    if (obj.m_AnimationInfo.name == clip.name)
                    {
                        needBake = false;
                        break;
                    }
                }

                if (!needBake)
                    continue;

                AnimationBakeInfo bake = new AnimationBakeInfo();
                bake.m_Length = clip.averageDuration;
                bake.m_Animator = animator;
				bake.m_Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                bake.m_Smrs = meshRender;
                bake.m_Layer = layer;
                bake.m_AnimationInfo = new AnimationInfo();
                bake.m_AnimationInfo.name = clip.name;
                bake.m_AnimationInfo.nameHash = state.state.nameHash;
                bake.m_AnimationInfo.animationIndex = animationIndex;
                bake.m_AnimationInfo.totalFrame = (int)(bake.m_Length * bakeFPS + 0.5f) + 1;
                bake.m_AnimationInfo.totalFrame = Mathf.Clamp(bake.m_AnimationInfo.totalFrame, 1, bake.m_AnimationInfo.totalFrame);
                bake.m_AnimationInfo.fps = bakeFPS;
                bake.m_AnimationInfo.wrapMode = clip.isLooping? WrapMode.Loop: clip.wrapMode;
                
                m_BakeInfo.Add(bake);
                animationIndex += bake.m_AnimationInfo.totalFrame;

                bake.m_AnimationInfo.eventList = new List<AnimationEvent>();
                foreach (var evt in clip.events)
                {
                    AnimationEvent aniEvent = new AnimationEvent();
                    aniEvent.function = evt.functionName;
                    aniEvent.floatParameter = evt.floatParameter;
                    aniEvent.intParameter = evt.intParameter;
                    aniEvent.stringParameter = evt.stringParameter;
                    aniEvent.time = evt.time;
                    if (evt.objectReferenceParameter != null)
                        aniEvent.objectParameter = evt.objectReferenceParameter.name;
                    else
                        aniEvent.objectParameter = "";
                    bake.m_AnimationInfo.eventList.Add(aniEvent);
                }

                state.state.transitions = null;
                m_CacheAnimationEvent.Add(clip, clip.events);
                UnityEngine.AnimationEvent[] tempEvent = new UnityEngine.AnimationEvent[0];
                UnityEditor.AnimationUtility.SetAnimationEvents(clip, tempEvent);
            }
            for (int i = 0; i != stateMachine.stateMachines.Length; ++i)
            {
                AnalyzeStateMachine(stateMachine.stateMachines[i].stateMachine, animator, meshRender, layer, bakeFPS, animationIndex);
            }
        }
#endif        
    }
}