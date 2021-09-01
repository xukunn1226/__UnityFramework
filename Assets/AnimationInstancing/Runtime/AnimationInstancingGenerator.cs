using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using Framework.Core;
using Application.Runtime;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace AnimationInstancingModule.Runtime
{
    /// <summary>
    /// 目录结构：
    /// AnimationData：存储所有动画数据
    /// [CustomPrefab]:自定义文件夹
    /// WARNING: 要求“CustomPrefab1.prefab”全局唯一，不可重名
    /// <summary>
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class AnimationInstancingGenerator : MonoBehaviour, ISerializationCallbackReceiver
    {
        class AnimationBakeInfo
        {
            public Animator                     animator;
            public int                          workingFrame;
            public float                        length;
            public AnimationInfo                info;
            public List<Matrix4x4[]>            boneMatrix;                 // 记录动画所有帧数据, [frameIndex][boneIndex]
        }

        public bool                             enableReference;                                                    // 与其他模型公用一套动画时使用
        public bool                             forceRebuildReference;
        public AnimationInstancingGenerator     referenceTo;
        public int                              fps                         = 15;                                   // 采样帧率
        public Dictionary<string, bool>         m_SelectExtraBone           = new Dictionary<string, bool>();       // 可额外导入的绑点骨骼列表
        [SerializeField] private List<string>   m_ExtraBoneNames            = new List<string>();                   // 所有绑点骨骼名称列表
        [SerializeField] private List<bool>     m_ExtraBoneSelectables      = new List<bool>();                     // 绑点骨骼的选中状态
        public Dictionary<string, bool>         m_GenerateAnims             = new Dictionary<string, bool>();       // 从AnimatorController解析出的动画
        [SerializeField] private List<string>   m_GenerateAnimNames         = new List<string>();        
        [SerializeField] private List<bool>     m_GenerateAnimSelectables   = new List<bool>();

        private List<string>                    m_ExtraBone                 = new List<string>();
        private List<Matrix4x4>                 m_BindPose                  = new List<Matrix4x4>(150);
        public List<Transform>                  m_BoneTransform             = new List<Transform>();
        private List<Transform>                 m_ExtraTransform            = new List<Transform>();
        private List<AnimationBakeInfo>         m_BakeInfo                  = new List<AnimationBakeInfo>();        // 待烘焙动画数据
        private AnimationBakeInfo               m_WorkingBakeInfo;
        private int                             m_CurWorkingBakeInfoIndex;
        private Dictionary<AnimationClip, UnityEngine.AnimationEvent[]> m_CacheAnimationEvent = new Dictionary<AnimationClip, UnityEngine.AnimationEvent[]>();
        private List<AnimationInfo>             m_AnimationInfo             = new List<AnimationInfo>();            // 待序列化的动画数据
        private int                             m_TextureBlockWidth         = 4;                                    // 4个像素表示一个矩阵
        private int                             m_TextureBlockHeight;
        private Texture2D                       m_BakedBoneTexture;
        public bool                             isBaking;
        private bool                            m_ExportAnimationTexture    = false;                                // 是否导出AnimationTexture，否则在二进制数据中
        static public string                    s_AnimationInstancingRoot   = "Assets/AnimationInstancing/Art";
        static public string                    s_AnimationDataPath         = s_AnimationInstancingRoot + "/AnimationData";
        private List<Transform>                 m_BakedLODs;

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
        private void OnEnable()
        {
            if(!UnityEngine.Application.isPlaying)
            {
                EditorApplication.update += EditorApplication.QueuePlayerLoopUpdate;
            }
        }

        private void OnDisable()
        {
            if(!UnityEngine.Application.isPlaying)
            {
                EditorApplication.update -= EditorApplication.QueuePlayerLoopUpdate;
            }
        }

        public List<Transform> GetLODs()
        {
            if(transform.childCount == 0)
            {
                return new List<Transform>();
            }
            
            List<Transform> LODs = new List<Transform>();
            int index = 0;
            while(true)
            {
                string childName = "LOD" + index;
                Transform child = transform.Find(childName);
                if(child != null)
                {
                    LODs.Add(child);
                    ++index;
                }
                else
                    break;
            }
            return LODs;
        }

        public void Prepare()
        {
            List<Transform> LODs = GetLODs();
            if(LODs == null || LODs.Count == 0)
                throw new ArgumentException("LODs == null || LODs.Count == 0");

            m_BakedLODs = LODs;

            // 获取蒙皮骨骼信息和绑点矩阵
            GetSkinnedBoneInfo(m_BakedLODs[0], ref m_BindPose, ref m_BoneTransform);
            GetExtraBoneInfo(m_BakedLODs[0], ref m_ExtraTransform);
        }

        public void Bake()
        {
            // 还原至bindpose态，好获取bindpose矩阵数据
            // PrefabUtility.RevertPrefabInstance(gameObject, InteractionMode.AutomatedAction);

            Prepare();

            isBaking = true;

            if(enableReference)
            {
                // 烘焙引用动画贴图
                if(forceRebuildReference)
                {
                    referenceTo.Bake();
                }
                else
                {
                    referenceTo.Prepare();
                }
            }
            else
            {
                // collect m_BakeInfo
                m_CurWorkingBakeInfoIndex = 0;
                m_BakeInfo.Clear();
                m_CacheAnimationEvent.Clear();
                m_AnimationInfo.Clear();
                m_WorkingBakeInfo = null;
                AnimatorController controller = GetComponent<Animator>().runtimeAnimatorController as AnimatorController;
                AnalyzeStateMachine(controller.layers[0].stateMachine, GetComponent<Animator>(), fps, 0);

                // init ExtraBoneInfo base on extra bone transform
                m_ExtraBone.Clear();
                foreach(var bone in m_ExtraTransform)
                {
                    m_ExtraBone.Add(bone.name);
                }
            }
        }

        public void Update()
        {
            if(enableReference)
            {
                if(forceRebuildReference)
                {
                    if(!referenceTo.isBaking && isBaking)
                    { // 引用对象烘焙结束，仅引用引用对象的AnimationData，自身不生成AnimationData
                        ExportAnimInstancingPrefab(referenceTo);
                        isBaking = false;
                    }
                }
                else
                {
                    if(isBaking)
                    {
                        ExportAnimInstancingPrefab(referenceTo);
                        isBaking = false;
                    }
                }
            }
            else
            {
                // 仍有待烘焙数据，且当前无烘焙任务
                if (m_BakeInfo.Count > 0 && m_CurWorkingBakeInfoIndex < m_BakeInfo.Count && m_WorkingBakeInfo == null)
                {
                    m_WorkingBakeInfo = m_BakeInfo[m_CurWorkingBakeInfoIndex];

                    m_WorkingBakeInfo.boneMatrix = new List<Matrix4x4[]>();
                    m_WorkingBakeInfo.info.extraBoneMatrix = new Dictionary<string, Matrix4x4[]>();
                    foreach(var boneName in m_ExtraBone)
                    {
                        m_WorkingBakeInfo.info.extraBoneMatrix.Add(boneName, new Matrix4x4[m_WorkingBakeInfo.info.totalFrame]);                        
                    }

                    m_WorkingBakeInfo.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    m_WorkingBakeInfo.animator.gameObject.SetActive(true);
                    m_WorkingBakeInfo.animator.Update(0);       // 不然会报warning：Animator does not have an AnimatorController
                    m_WorkingBakeInfo.animator.Play(m_WorkingBakeInfo.info.nameHash);
                    m_WorkingBakeInfo.animator.Update(0);       // 第一帧
                    return;
                }

                if (m_WorkingBakeInfo == null)
                    return;

                // 计算每帧的蒙皮矩阵和绑点矩阵
                GenerateSkinnedBoneMatrix(m_WorkingBakeInfo, m_BoneTransform, m_BindPose);
                GenerateExtraBoneMatrix(m_WorkingBakeInfo, m_ExtraTransform);

                if (++m_WorkingBakeInfo.workingFrame >= m_WorkingBakeInfo.info.totalFrame)
                {
                    m_AnimationInfo.Add(m_WorkingBakeInfo.info);

                    m_WorkingBakeInfo = null;
                    ++m_CurWorkingBakeInfoIndex;

                    if (m_BakeInfo.Count == m_CurWorkingBakeInfoIndex)
                    { // 所有动画数据烘焙完毕
                        foreach (var obj in m_CacheAnimationEvent)
                        { // 动画数据烘焙之后还原动画事件数据
                            UnityEditor.AnimationUtility.SetAnimationEvents(obj.Key, obj.Value);
                        }
                        m_CacheAnimationEvent.Clear();

                        SaveAnimationInfo();
                        ExportAnimDataPrefab();
                        ExportAnimInstancingPrefab(null);
                        isBaking = false;
                    }

                    return;
                }

                float deltaTime = m_WorkingBakeInfo.length / (m_WorkingBakeInfo.info.totalFrame - 1);
                m_WorkingBakeInfo.animator.Update(deltaTime);
            }
        }

        // get the bindPose & boneTransform base on attached points
        public void GetSkinnedBoneInfo(Transform lod, ref List<Matrix4x4> bindPose, ref List<Transform> boneTransform)
        {
            SkinnedMeshRenderer[] meshRender = lod.GetComponentsInChildren<SkinnedMeshRenderer>();
            AnimationInstancingModule.Runtime.AnimationUtility.MergeBone(meshRender, ref bindPose, ref boneTransform);

            // 绑点信息不存储至animation texture，暂注释
            // if(exposeAttachments)
            // { // 如果有挂点数据，则添加至bindPose和boneTransform
            //     List<Transform> listExtra = GetExtraBoneTransform(lod);
            //     foreach(var tran in listExtra)
            //     {
            //         bindPose.Add(tran.localToWorldMatrix);
            //     }

            //     Transform[] totalTransform = new Transform[boneTransform.Count + listExtra.Count];
            //     System.Array.Copy(boneTransform.ToArray(), totalTransform, boneTransform.Count);
            //     System.Array.Copy(listExtra.ToArray(), 0, totalTransform, boneTransform.Count, listExtra.Count);
            //     boneTransform = totalTransform.ToList();
            // }
        }

        // extra bone transform
        private void GetExtraBoneInfo(Transform lod, ref List<Transform> boneTransform)
        {
            boneTransform.Clear();

            Transform[] trans = lod.GetComponentsInChildren<Transform>();
            foreach (var obj in m_SelectExtraBone)
            {
                if (!obj.Value)
                    continue;

                for (int i = 0; i != trans.Length; ++i)
                {
                    Transform tran = trans[i] as Transform;
                    if (tran.name == obj.Key)
                    {
                        boneTransform.Add(trans[i]);
                    }
                }
            }
        }

        private void AnalyzeStateMachine(AnimatorStateMachine stateMachine, Animator animator, int bakeFPS, int startFrameIndex)
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
                bake.workingFrame = 0;
                bake.info = new AnimationInfo();
                bake.info.name = clip.name;
                bake.info.nameHash = state.state.nameHash;
                bake.info.startFrameIndex = startFrameIndex;
                bake.info.totalFrame = CalculateTotalFrames(bake.length, bakeFPS);
                bake.info.totalFrame = Mathf.Clamp(bake.info.totalFrame, 1, bake.info.totalFrame);
                bake.info.fps = bakeFPS;
                bake.info.wrapMode = clip.isLooping? WrapMode.Loop: clip.wrapMode;
                
                m_BakeInfo.Add(bake);
                startFrameIndex += bake.info.totalFrame;        // 下一个动画的起始帧号

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

                m_CacheAnimationEvent.Add(clip, clip.events);       // 记录下动画事件，以免animator.Update会清除event
                UnityEngine.AnimationEvent[] tempEvent = new UnityEngine.AnimationEvent[0];
                UnityEditor.AnimationUtility.SetAnimationEvents(clip, tempEvent);
            }
            for (int i = 0; i != stateMachine.stateMachines.Length; ++i)
            {
                AnalyzeStateMachine(stateMachine.stateMachines[i].stateMachine, animator, bakeFPS, startFrameIndex);
            }
        }

        // 计算动画当前时间的蒙皮矩阵数据
        private void GenerateSkinnedBoneMatrix(AnimationBakeInfo bakeInfo, List<Transform> boneTransform, List<Matrix4x4> bindPose)
        {
            Transform root = boneTransform[0];
            while (root.parent != null)
            {
                root = root.parent;
            }

            bakeInfo.boneMatrix.Add(CalculateSkinMatrix(root, boneTransform, bindPose));
        }

        private Matrix4x4[] CalculateSkinMatrix(Transform root, List<Transform> boneTransform, List<Matrix4x4> bindPose)
        {
            // bindPose：模型空间到骨骼空间，The bind pose is the inverse of the transformation matrix of the bone, when the bone is in the bind pose
            // bindPoses[1] * Vmesh= bones[1].worldToLocalMatrix * transform.localToWorldMatrix * Vmesh;
            //      bones[1].worldToLocalMatrix: 世界坐标空间到骨骼的局部坐标空间
            //      transform.localToWorldMatrix: 模型（根节点）的局部坐标空间到世界坐标空间
            //      Vmesh：模型空间顶点
            //      见https://docs.unity3d.com/ScriptReference/Mesh-bindposes.html
            // bonePose.localToWorldMatrix：骨骼空间到模型空间
            Matrix4x4[] matrix = new Matrix4x4[boneTransform.Count];
            for (int i = 0; i != boneTransform.Count; ++i)
            {
                matrix[i] = root.worldToLocalMatrix * boneTransform[i].localToWorldMatrix * bindPose[i];
            }
            return matrix;
        }

        private void GenerateExtraBoneMatrix(AnimationBakeInfo bakeInfo, List<Transform> boneTransform)
        {
            if(m_ExtraBone.Count == 0)
                return;

            Transform root = boneTransform[0];
            while (root.parent != null)
            {
                root = root.parent;
            }

            foreach(var boneName in m_ExtraBone)
            {
                int index = boneTransform.FindIndex(item => item.name == boneName);
                Debug.Assert(index != -1);

                bakeInfo.info.extraBoneMatrix[boneName][bakeInfo.workingFrame] = CalculateExtraMatrix(root, boneTransform[index]);
            }
        }

        private Matrix4x4 CalculateExtraMatrix(Transform root, Transform boneTransform)
        {
            return root.worldToLocalMatrix * boneTransform.localToWorldMatrix;
        }

        private Color[] Convert2Color(Matrix4x4[] boneMatrix)
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

        public int CalculateTotalFrames(float length, int fps)
        {
            // return Mathf.CeilToInt(length * fps) + 1;
            return (int)(length * fps);
        }
        
        // 计算动画数据占用的贴图大小
        // 每根骨骼4个像素（一个像素记录4个值，4个像素一个矩阵），一个block记录一帧所有的骨骼数据
        // public void CalculateTextureSize(List<int> frames, List<Transform> boneTransform, out int textureWidth, out int textureHeight)
        // {
        //     m_TextureBlockWidth = 4;
        //     m_TextureBlockHeight = boneTransform.Count;

        //     int pixels = boneTransform.Count * frames.Sum() * m_TextureBlockWidth;             // 总像素数
        //     int side = Mathf.Max(Mathf.CeilToInt(Mathf.Sqrt(pixels)), m_TextureBlockHeight);

        //     int width = Mathf.NextPowerOfTwo(side);
        //     int xBlockNum = width / m_TextureBlockWidth;
        //     int yBlockNum = Mathf.CeilToInt(1.0f * frames.Sum() / xBlockNum);
        //     int height = Mathf.NextPowerOfTwo(yBlockNum * m_TextureBlockHeight);

        //     textureWidth = width;
        //     textureHeight = height;

        //     int width2 = Mathf.ClosestPowerOfTwo(side);
        //     if(width != width2)
        //     {
        //         xBlockNum = width2 / m_TextureBlockWidth;
        //         yBlockNum = Mathf.CeilToInt(1.0f * frames.Sum() / xBlockNum);
        //         int height2 = Mathf.NextPowerOfTwo(yBlockNum * m_TextureBlockHeight);
        //         if(width2 * height2 < width * height)
        //         {
        //             textureWidth = width2;
        //             textureHeight = height2;
        //         }
        //     }
        //     Debug.Assert(textureWidth * textureHeight >= pixels);
        // }

        public void CalculateTextureSize(List<int> frames, List<Transform> boneTransform, out int textureWidth, out int textureHeight)
        {
            m_TextureBlockWidth = 4;
            m_TextureBlockHeight = boneTransform.Count;

            int pixels = boneTransform.Count * frames.Sum() * m_TextureBlockWidth;             // 总像素数
            int side = Mathf.Max(Mathf.CeilToInt(Mathf.Sqrt(pixels)), m_TextureBlockHeight);

            int width = Mathf.ClosestPowerOfTwo(side);
            int xBlockNum = width / m_TextureBlockWidth;
            int yBlockNum = Mathf.CeilToInt(1.0f * frames.Sum() / xBlockNum);
            int height = MathUtility.AroundTo(yBlockNum * m_TextureBlockHeight, 4);

            textureWidth = width;
            textureHeight = height;

            int width2 = Mathf.NextPowerOfTwo(side);
            if(width != width2)
            {
                xBlockNum = width2 / m_TextureBlockWidth;
                yBlockNum = Mathf.CeilToInt(1.0f * frames.Sum() / xBlockNum);
                int height2 = MathUtility.AroundTo(yBlockNum * m_TextureBlockHeight, 4);
                if(width2 * height2 < width * height)
                {
                    textureWidth = width2;
                    textureHeight = height2;
                }
            }

            // textureHeight = MathUtility.AroundTo(side / boneTransform.Count * boneTransform.Count, 4);      // 取4的倍数
            // // textureWidth = MathUtility.AroundTo((int)(1.0f * pixels / (textureHeight == 0 ? 1 : textureHeight) + 0.5f), 4);

            // // 贴图height不变情况下拓宽width
            // int yBlockNum = textureHeight / boneTransform.Count;
            // int xBlockNum = Mathf.CeilToInt(1.0f * frames.Sum() / yBlockNum);
            // textureWidth = MathUtility.AroundTo(xBlockNum * m_TextureBlockWidth, 4);

            Debug.Assert(textureWidth * textureHeight >= pixels);
        }

        private void SetupAnimationTexture()
        {
            List<int> frames = new List<int>();
            foreach(var info in m_BakeInfo)
            {
                frames.Add(info.boneMatrix.Count);
            }

            int textureWidth, textureHeight;
            CalculateTextureSize(frames, m_BoneTransform, out textureWidth, out textureHeight);
            m_BakedBoneTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBAHalf, false);
            m_BakedBoneTexture.filterMode = FilterMode.Point;

            int pixelx = 0;
            int pixely = 0;
            for(int i = 0; i < m_BakeInfo.Count; ++i)
            {
                m_BakeInfo[i].info.startFrameIndex = pixelx / m_TextureBlockWidth + pixely / m_TextureBlockHeight * (m_BakedBoneTexture.width / m_TextureBlockWidth);
                int frameCount = m_BakeInfo[i].boneMatrix.Count;
                for(int j = 0; j < frameCount; ++j)
                {
                    // Debug.Log($"{j}  {pixelx}  {pixely}");
                    Color[] colors = Convert2Color(m_BakeInfo[i].boneMatrix[j]);        // 一块block数据（boneCount * 4），即一帧
                    m_BakedBoneTexture.SetPixels(pixelx, pixely, m_TextureBlockWidth, m_TextureBlockHeight, colors);

                    pixelx += m_TextureBlockWidth;
                    if(pixelx >= m_BakedBoneTexture.width)
                    {
                        pixelx = 0;
                        pixely += m_TextureBlockHeight;
                    }
                }
            }
        }

        private void SaveAnimationInfo()
        {
            SetupAnimationTexture();

            if(!Directory.Exists(s_AnimationDataPath))
                Directory.CreateDirectory(s_AnimationDataPath);

            string filename = GetManifestFilename();
            using(FileStream fs = File.Open(filename, FileMode.Create, FileAccess.Write))
            {
                BinaryWriter writer = new BinaryWriter(fs);
                writer.Write(m_BakeInfo.Count);
                foreach(var bakeInfo in m_BakeInfo)
                {
                    AnimationInfo info = bakeInfo.info;
                    writer.Write(info.name);
                    writer.Write(info.startFrameIndex);
                    writer.Write(info.totalFrame);
                    writer.Write(info.fps);
                    writer.Write((int)info.wrapMode);

                    writer.Write(info.eventList.Count);
                    foreach(var evt in info.eventList)
                    {
                        writer.Write(evt.function);
                        writer.Write(evt.floatParameter);
                        writer.Write(evt.intParameter);
                        writer.Write(evt.stringParameter);
                        writer.Write(evt.time);
                        writer.Write(evt.objectParameter);
                    }

                    writer.Write(info.extraBoneMatrix.Count);
                    foreach(var extra in info.extraBoneMatrix)
                    {
                        writer.Write(extra.Key);
                        writer.Write(extra.Value.Length);                       // 矩阵数量

                        Matrix4x4[] matrixs = extra.Value;
                        Debug.Assert(matrixs.Length == info.totalFrame);        // 矩阵数量应等于此动画的帧数                        
                        foreach(var matrix in matrixs)
                        {
                            writer.Write(matrix[0, 0]);
                            writer.Write(matrix[0, 1]);
                            writer.Write(matrix[0, 2]);
                            writer.Write(matrix[0, 3]);
                            writer.Write(matrix[1, 0]);
                            writer.Write(matrix[1, 1]);
                            writer.Write(matrix[1, 2]);
                            writer.Write(matrix[1, 3]);
                            writer.Write(matrix[2, 0]);
                            writer.Write(matrix[2, 1]);
                            writer.Write(matrix[2, 2]);
                            writer.Write(matrix[2, 3]);
                            writer.Write(matrix[3, 0]);
                            writer.Write(matrix[3, 1]);
                            writer.Write(matrix[3, 2]);
                            writer.Write(matrix[3, 3]);                          
                        }                        
                    }
                }

                // write boneTexture
                writer.Write(m_TextureBlockWidth);
                writer.Write(m_TextureBlockHeight);
            }

            string animTextureRawDataFilename = GetAnimationTextureRawDataFilename();
            using(FileStream fs = File.Open(animTextureRawDataFilename, FileMode.Create, FileAccess.Write))
            {
                BinaryWriter writer = new BinaryWriter(fs);
                byte[] bytes = m_BakedBoneTexture.GetRawTextureData();
                writer.Write(m_BakedBoneTexture.width);
                writer.Write(m_BakedBoneTexture.height);
                writer.Write(bytes.Length);
                writer.Write(bytes);
            }
            
            if(m_ExportAnimationTexture)
            {
                File.WriteAllBytes(GetAnimationTextureFilename(), m_BakedBoneTexture.EncodeToPNG());
            }

            AssetDatabase.Refresh();
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(filename);
            Debug.Log($"save animation data manifest: {filename}  {asset}", asset);
        }

        private void ExportAnimDataPrefab()
        {
            // 保存root/AnimationData/[Custom].prefab
            // new Prefab
            GameObject animDataPrefab = new GameObject(gameObject.name);

            // add AnimationData component
            AnimationData animData = animDataPrefab.AddComponent<AnimationData>();
            animData.manifest = AssetDatabase.LoadAssetAtPath<TextAsset>(GetManifestFilename());
            
            // add SoftObject component
            SoftObject texSoftObject = animDataPrefab.AddComponent<SoftObject>();
            texSoftObject.assetPath = GetAnimationTextureRawDataFilename().ToLower();
            animData.animTexSoftObject = texSoftObject;

            // save Prefab
            GameObject animDataAsset = PrefabUtility.SaveAsPrefabAsset(animDataPrefab, GetAnimationDataPrefabFilename());
            DestroyImmediate(animDataPrefab);
            
            Debug.Log($"export animation data prefab: {GetAnimationDataPrefabFilename()}");
        }

        private void ExportAnimInstancingPrefab(AnimationInstancingGenerator reference)
        {
            // 导出资源：mesh, material, texture
            int lodLevel = 0;
            foreach(var lod in m_BakedLODs)
            {
                SkinnedMeshRenderer[] smrs = lod.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var smr in smrs)
                {
                    ExtractInternalAssets(lod, smr, lodLevel);
                }
                ++lodLevel;
            }
            
            // 实例化对象，删除所有子节点和根节点上多余组件
            GameObject inst = Instantiate(gameObject);
            inst.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            int count = inst.transform.childCount;
            for(int i = count - 1; i >= 0; --i)
            {
                DestroyImmediate(inst.transform.GetChild(i).gameObject);
            }

            Component[] comps = inst.GetComponents<Component>();
            foreach(var comp in comps)
            {
                if(comp is Transform)
                    continue;
                DestroyImmediate(comp);
            }

            // 添加AnimationInstancing，记录RendererCache
            AnimationInstancing animInst = inst.AddComponent<AnimationInstancing>();

            // setup
            GameObject animDataAsset = AssetDatabase.LoadAssetAtPath<GameObject>(reference?.GetAnimationDataPrefabFilename() ?? GetAnimationDataPrefabFilename());
            Debug.Assert(animDataAsset != null);
            animInst.prototype = animDataAsset.GetComponent<AnimationData>();
            animInst.radius = CalcBoundingSphere();
            animInst.lodDistance[0] = 50;
            animInst.lodDistance[1] = 250;
            lodLevel = 0;
            foreach(var lod in m_BakedLODs)
            {
                LODInfo info = new LODInfo();                
                SkinnedMeshRenderer[] smrs = lod.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach(var smr in smrs)
                {
                    RendererCache cache = new RendererCache();
                    cache.mesh = AssetDatabase.LoadAssetAtPath<Mesh>(GetMeshFilename(gameObject, smr));
                    cache.materials = new Material[smr.sharedMaterials.Length];
                    for(int i = 0; i < smr.sharedMaterials.Length; ++i)
                    {
                        Material mat = smr.sharedMaterials[i];
                        if( mat == null)
                            continue;

                        string newMatFilename = GetMaterialFilename(mat.name).ToLower();
                        cache.materials[i] = AssetDatabase.LoadAssetAtPath<Material>(newMatFilename);
                    }

                    cache.bonePerVertex = lodLevel == 0 ? 4 : (lodLevel == 1 ? 2 : 1);
                    info.rendererCacheList.Add(cache);
                }
                info.lodLevel = lodLevel++;
                animInst.lodInfos.Add(info);
            }

            // step4. 保存新的prefab
            PrefabUtility.SaveAsPrefabAsset(inst, GetAnimationInstancingPrefabFilename());
            DestroyImmediate(inst);
            AssetDatabase.Refresh();

            // 把实例还原至original prefab
            // PrefabUtility.RevertPrefabInstance(gameObject, InteractionMode.AutomatedAction);


            Debug.Log($"export animation instancing prefab: {GetAnimationInstancingPrefabFilename()}");
        }

        private void ExtractInternalAssets(Transform lod, SkinnedMeshRenderer smr, int lodLevel)
        {
            // method 1. extract mesh
            Mesh meshInst = UnityEngine.Object.Instantiate(smr.sharedMesh);
            meshInst.colors = GetBoneWeights(meshInst, lodLevel == 0 ? 4 : (lodLevel == 1 ? 2 : 1));
            meshInst.SetUVs(2, GetBoneIndices(meshInst, smr, m_BoneTransform));
            meshInst.UploadMeshData(true);      // isReadable = false
            AssetDatabase.CreateAsset(meshInst, GetMeshFilename(gameObject, smr));

            // extract material and texture
            foreach(var mat in smr.sharedMaterials)
            {
                if(mat == null)
                    continue;

                // create new material
                string newMatFilename = GetMaterialFilename(mat.name).ToLower();
                Material newMat = UnityEngine.Object.Instantiate(mat) as Material;
                newMat.shader = Shader.Find("ZGame/URP/Standard-Instancing");
                newMat.enableInstancing = true;

                string[] names = mat.GetTexturePropertyNames();
                for(int i = 0; i < names.Length; ++i)
                {
                    Texture tex = mat.GetTexture(names[i]);
                    if(tex == null)
                        continue;

                    string newTexFilename = GetTextureFilename(tex).ToLower();
                    System.IO.File.Copy(AssetDatabase.GetAssetPath(tex), newTexFilename, true);
                    AssetDatabase.ImportAsset(newTexFilename);
                    
                    newMat.SetTexture(names[i], AssetDatabase.LoadAssetAtPath<Texture>(newTexFilename));
                }
                AssetDatabase.CreateAsset(newMat, newMatFilename);
            }
        }

        private Color[] GetBoneWeights(Mesh mesh, int bonePerVertex)
        {
            Color[] weights = new Color[mesh.vertexCount];
            BoneWeight[] boneWeights = mesh.boneWeights;
            for(int i = 0; i < mesh.vertexCount; ++i)
            {
                weights[i].r = boneWeights[i].weight0;
                weights[i].g = boneWeights[i].weight1;
                weights[i].b = boneWeights[i].weight2;
                weights[i].a = boneWeights[i].weight3;

                switch (bonePerVertex)
                {
                    case 3:
                        {
                            float scale = 1.0f / (weights[i].r + weights[i].g + weights[i].b);
                            weights[i].r = weights[i].r * scale;
                            weights[i].g = weights[i].g * scale;
                            weights[i].b = weights[i].b * scale;
                            weights[i].a = -0.1f;
                        }
                        break;
                    case 2:
                        {
                            float scale = 1.0f / (weights[i].r + weights[i].g);
                            weights[i].r = weights[i].r * scale;
                            weights[i].g = weights[i].g * scale;
                            weights[i].b = -0.1f;
                            weights[i].a = -0.1f;
                        }
                        break;
                    case 1:
                        {
                            weights[i].r = 1.0f;
                            weights[i].g = -0.1f;
                            weights[i].b = -0.1f;
                            weights[i].a = -0.1f;
                        }
                        break;
                }
            }

            return weights;
        }

        private Vector4[] GetBoneIndices(Mesh mesh, SkinnedMeshRenderer render, List<Transform> boneTransform)
        {
            int[] realBoneIndices = new int[render.bones.Length];       // 记录render.bones中每根骨骼在boneTransform（最终记录在AnimationTexture）中的索引值
            for(int i = 0; i < render.bones.Length; ++i)
            {
                int hashName = render.bones[i].name.GetHashCode();
                realBoneIndices[i] = boneTransform.FindIndex(item => ( item.name.GetHashCode() == hashName ));
                Debug.Assert(realBoneIndices[i] != -1);
            }

            Vector4[] boneIndices = new Vector4[mesh.vertexCount];
            BoneWeight[] boneWeights = mesh.boneWeights;
            for(int i = 0; i < mesh.vertexCount; ++i)
            {
                boneIndices[i].x = realBoneIndices[boneWeights[i].boneIndex0];
                boneIndices[i].y = realBoneIndices[boneWeights[i].boneIndex1];
                boneIndices[i].z = realBoneIndices[boneWeights[i].boneIndex2];
                boneIndices[i].w = realBoneIndices[boneWeights[i].boneIndex3];
            }

            return boneIndices;
        }

        private float CalcBoundingSphere()
        {
            Bounds bound = new Bounds(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
            Transform lod = m_BakedLODs[0];
            SkinnedMeshRenderer[] smrs = lod.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach(var smr in smrs)
            {
                bound.Encapsulate(smr.bounds);
            }
            float radius = bound.size.x > bound.size.y ? bound.size.x : bound.size.y;
            radius = radius > bound.size.z ? radius : bound.size.z;
            return (float)Math.Round(radius, 2);
        }
        
        // root/[CustomPrefab1]
        private string GetExportedPath()
        {
            string path = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject));
            path = path.Substring(0, path.LastIndexOf("/"));
            return path.Substring(0, path.LastIndexOf("/"));
        }
        
        // prefab name + lod level + mesh name
        private string GetMeshFilename(GameObject root, SkinnedMeshRenderer smr)
        {
            string prefix = string.Format($"{root.name}_{smr.transform.parent.gameObject.name}");

            return string.Format($"{GetExportedPath()}/{prefix.ToLower()}_{smr.sharedMesh.name.ToLower()}.asset");
        }

        private string GetMaterialFilename(string matName)
        {
            return string.Format($"{GetExportedPath()}/{matName}.mat");
        }

        private string GetTextureFilename(Texture tex)
        {
            string assetPath = AssetDatabase.GetAssetPath(tex);
            string filename = Path.GetFileName(assetPath);
            return string.Format($"{GetExportedPath()}/{filename}");
        }

        private string GetAnimationInstancingPrefabFilename()
        {
            return GetExportedPath() + "/" + gameObject.name.ToLower() + ".prefab";
        }

        private string GetManifestFilename()
        {
            return s_AnimationDataPath + "/" + gameObject.name.ToLower() + ".bytes";
        }

        private string GetAnimationTextureRawDataFilename()
        {
            return s_AnimationDataPath + "/" + gameObject.name.ToLower() + "_animtexture" + ".bytes";
        }

        private string GetAnimationTextureFilename()
        {
            return s_AnimationDataPath + "/" + gameObject.name.ToLower() + ".png";
        }

        public string GetAnimationDataPrefabFilename()
        {
            return s_AnimationDataPath + "/" + gameObject.name.ToLower() + ".prefab";
        }
#endif
    }
}