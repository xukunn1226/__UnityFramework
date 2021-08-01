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
    [ExecuteInEditMode]
    public class AnimationInstancingGenerator : MonoBehaviour, ISerializationCallbackReceiver
    {
        class AnimationBakeInfo
        {
            public SkinnedMeshRenderer[]        smrs;
            public Animator                     animator;
            public int                          workingFrame;
            public float                        length;
            public AnimationInfo                info;
            public List<Matrix4x4[]>            boneMatrix;             // 记录动画所有帧数据, [frameIndex][boneIndex]
        }

        // class GenerateObjectInfo
        // {
        //     public int                          nameCode;
        //     public float                        animationTime;
        //     public int                          aniNameHash;
        //     public int                          frameIndex;
        //     public Matrix4x4[]                  boneMatrix;
        // }

        public int                              fps                         = 15;                                   // 采样帧率
        public bool                             exposeAttachments;                                                  // 是否导入绑点信息
        public Dictionary<string, bool>         m_SelectExtraBone           = new Dictionary<string, bool>();       // 可额外导入的绑点骨骼列表
        [SerializeField] private List<string>   m_ExtraBoneNames            = new List<string>();                   // 所有绑点骨骼名称列表
        [SerializeField] private List<bool>     m_ExtraBoneSelectables      = new List<bool>();                     // 绑点骨骼的选中状态
        public Dictionary<string, bool>         m_GenerateAnims             = new Dictionary<string, bool>();       // 从AnimatorController解析出的动画
        [SerializeField] private List<string>   m_GenerateAnimNames         = new List<string>();        
        [SerializeField] private List<bool>     m_GenerateAnimSelectables   = new List<bool>();


        private ExtraBoneInfo                   m_ExtraBoneInfo             = new ExtraBoneInfo();
        private List<Matrix4x4>                 m_BindPose                  = new List<Matrix4x4>(150);
        private List<Transform>                 m_BoneTransform             = new List<Transform>();
        private List<AnimationBakeInfo>         m_BakeInfo                  = new List<AnimationBakeInfo>();        // 待烘焙动画数据
        private AnimationBakeInfo               m_WorkingBakeInfo;
        private Dictionary<AnimationClip, UnityEngine.AnimationEvent[]> m_CacheAnimationEvent = new Dictionary<AnimationClip, UnityEngine.AnimationEvent[]>();
        // static private GenerateObjectInfo[]     m_GenerateObjectInfo;       // 每帧动画数据
        // private int                             m_CurrentDataIndex;
        // private Dictionary<int, List<GenerateObjectInfo>>  m_GenerateMatrixDataPool = new Dictionary<int, List<GenerateObjectInfo>>();   // key: aniNameHash; value: List<GenerateObjectInfo> 动画所有帧数据
        private List<AnimationInfo>             m_AnimationInfo             = new List<AnimationInfo>();        // 待序列化的动画数据

        // static private GenerateObjectInfo[]     generateObjectInfo
        // {
        //     get
        //     {
        //         if (m_GenerateObjectInfo == null)
        //         {
        //             m_GenerateObjectInfo = new GenerateObjectInfo[5000];
        //             for (int i = 0; i < 5000; ++i)
        //             {
        //                 m_GenerateObjectInfo[i] = new GenerateObjectInfo();
        //             }
        //         }
        //         return m_GenerateObjectInfo;
        //     }
        // }

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
            // 获取最终的骨骼信息和绑定姿态矩阵(算上了绑点)
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
            
            // collect m_BakeInfo
            m_BakeInfo.Clear();
            m_CacheAnimationEvent.Clear();
            AnimatorController controller = GetComponent<Animator>().runtimeAnimatorController as AnimatorController;
            AnalyzeStateMachine(controller.layers[0].stateMachine, GetComponent<Animator>(), GetComponentsInChildren<SkinnedMeshRenderer>(), fps, 0);

            // reset
            // m_GenerateMatrixDataPool.Clear();
            // m_CurrentDataIndex = 0;
            m_AnimationInfo.Clear();
        }

        private void Update()
        {
            if(m_BakeInfo.Count > 0 && m_WorkingBakeInfo == null)
            {
                m_WorkingBakeInfo = m_BakeInfo[0];
                m_BakeInfo.RemoveAt(0);

                m_WorkingBakeInfo.animator.gameObject.SetActive(true);
                m_WorkingBakeInfo.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                m_WorkingBakeInfo.boneMatrix = new List<Matrix4x4[]>();
                // m_WorkingBakeInfo.m_Animator.Update(0);
                m_WorkingBakeInfo.animator.Play(m_WorkingBakeInfo.info.nameHash);
                m_WorkingBakeInfo.animator.Update(0);
                return;
            }

            if(m_WorkingBakeInfo == null)
                return;

            GenerateBoneMatrix(m_WorkingBakeInfo, m_BoneTransform, m_BindPose);

            if(++m_WorkingBakeInfo.workingFrame >= m_WorkingBakeInfo.info.totalFrame)
            {
                m_AnimationInfo.Add(m_WorkingBakeInfo.info);

                if(m_BakeInfo.Count == 0)
                {
                    // save info
                }

                m_WorkingBakeInfo = null;
                return;
            }

            float deltaTime = m_WorkingBakeInfo.length / (m_WorkingBakeInfo.info.totalFrame - 1);
            m_WorkingBakeInfo.animator.Update(deltaTime);
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
        private List<Transform> GetExtraBoneTransform()
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

        private void AnalyzeStateMachine(AnimatorStateMachine stateMachine, Animator animator, SkinnedMeshRenderer[] meshRender, int bakeFPS, int animationIndex)
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
                    if (obj.info.name == clip.name)
                    {
                        needBake = false;
                        break;
                    }
                }

                if (!needBake)
                    continue;

                AnimationBakeInfo bake = new AnimationBakeInfo();
                bake.length = clip.averageDuration;
                bake.animator = animator;
                bake.smrs = meshRender;
                bake.workingFrame = 0;
                bake.info = new AnimationInfo();
                bake.info.name = clip.name;
                bake.info.nameHash = state.state.nameHash;
                bake.info.animationIndex = animationIndex;
                bake.info.totalFrame = (int)(bake.length * bakeFPS + 0.5f) + 1;
                bake.info.totalFrame = Mathf.Clamp(bake.info.totalFrame, 1, bake.info.totalFrame);
                bake.info.fps = bakeFPS;
                bake.info.wrapMode = clip.isLooping? WrapMode.Loop: clip.wrapMode;
                
                m_BakeInfo.Add(bake);
                animationIndex += bake.info.totalFrame;

                bake.info.eventList = new List<AnimationEvent>();
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
                    bake.info.eventList.Add(aniEvent);
                }

                m_CacheAnimationEvent.Add(clip, clip.events);
                UnityEngine.AnimationEvent[] tempEvent = new UnityEngine.AnimationEvent[0];
                UnityEditor.AnimationUtility.SetAnimationEvents(clip, tempEvent);
            }
            for (int i = 0; i != stateMachine.stateMachines.Length; ++i)
            {
                AnalyzeStateMachine(stateMachine.stateMachines[i].stateMachine, animator, meshRender, bakeFPS, animationIndex);
            }
        }

        // 计算动画当前时间的蒙皮矩阵数据
        private void GenerateBoneMatrix(AnimationBakeInfo bakeInfo, List<Transform> boneTransform, List<Matrix4x4> bindPose)
        {
            UnityEngine.Profiling.Profiler.BeginSample("CalculateSkinMatrix()");
            {
                // GenerateObjectInfo matrixData = generateObjectInfo[m_CurrentDataIndex++];
                // // matrixData.nameCode = nameCode;
                // matrixData.aniNameHash = aniNameHash;
                // matrixData.animationTime = animationTime;
                // matrixData.frameIndex = -1;

                // UnityEngine.Profiling.Profiler.BeginSample("AddBoneMatrix:update the matrix");
                // {
                //     if (!m_GenerateMatrixDataPool.ContainsKey(aniNameHash))
                //     {
                //         m_GenerateMatrixDataPool[aniNameHash] = new List<GenerateObjectInfo>();
                //     }
                //     matrixData.boneMatrix = CalculateSkinMatrix(boneTransform, bindPose);
                //     // GenerateObjectInfo data = new GenerateObjectInfo();
                //     // CopyMatrixData(data, matrixData);
                //     m_GenerateMatrixDataPool[aniNameHash].Add(matrixData);
                // }
                // UnityEngine.Profiling.Profiler.EndSample();

                bakeInfo.boneMatrix.Add(CalculateSkinMatrix(boneTransform, bindPose));
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }

        public static Matrix4x4[] CalculateSkinMatrix(List<Transform> bonePose, List<Matrix4x4> bindPose)
        {
            if (bonePose.Count == 0)
                return null;

            Transform root = bonePose[0];
            while (root.parent != null)
            {
                root = root.parent;
            }
            Matrix4x4 rootMat = root.worldToLocalMatrix;

            // bindPose：模型空间到骨骼空间，The bind pose is the inverse of the transformation matrix of the bone, when the bone is in the bind pose
            // bindPoses[1] * Vmesh= bones[1].worldToLocalMatrix * transform.localToWorldMatrix * Vmesh;
            //      bones[1].worldToLocalMatrix: 世界坐标空间到骨骼的局部坐标空间
            //      transform.localToWorldMatrix: 模型（根节点）的局部坐标空间到世界坐标空间
            //      Vmesh：模型空间顶点
            //      见https://docs.unity3d.com/ScriptReference/Mesh-bindposes.html
            // bonePose.localToWorldMatrix：骨骼空间到模型空间
            Matrix4x4[] matrix = new Matrix4x4[bonePose.Count];
            for (int i = 0; i != bonePose.Count; ++i)
            {
                matrix[i] = rootMat * bonePose[i].localToWorldMatrix * bindPose[i];
            }
            return matrix;
        }

        public static Color[] Convert2Color(Matrix4x4[] boneMatrix)
        {
            Color[] color = new Color[boneMatrix.Length * 4];
            int index = 0;
            foreach (var obj in boneMatrix)
            {
                color[index++] = obj.GetRow(0);
                color[index++] = obj.GetRow(1);
                color[index++] = obj.GetRow(2);
                color[index++] = obj.GetRow(3);
            }
            return color;
        }
#endif        
    }
}