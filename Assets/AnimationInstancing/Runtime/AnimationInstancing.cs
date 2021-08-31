using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;

namespace AnimationInstancingModule.Runtime
{
    public class AnimationInstancing : MonoBehaviour
    {
        public delegate void onAnimationHandler(string aniName);
        public event onAnimationHandler   OnAnimationBegin;
        public event onAnimationHandler   OnAnimationEnd;

        public delegate void onAnimationEvent(string aniName, string evtName, AnimationEvent evt);
        public event onAnimationEvent   OnAnimationEvent;

        [SerializeField]
        private AnimationData           m_Prototype;                                    // WARNING: 资源，非实例化数据
        public AnimationData            prototype           { private get { return m_Prototype; } set { m_Prototype = value; } }
        public AnimationData            animDataInst        { get; private set; }
        public List<LODInfo>            lodInfos            = new List<LODInfo>();

        public Transform                worldTransform      { get; private set; }
        [SerializeField] private float  m_Speed             = 1;                        // 预设的速度值，序列化数据
        public float                    speedScale          { get; set; } = 1;
        private float                   m_CacheSpeedScale;
        public float                    playSpeed           { get { return m_Speed * speedScale; } }        // 最终的实际速度值
        private float                   m_speedParameter    = 1.0f;                     // 某些特性时使用的临时变量，例如pingpong
        [SerializeField] private float  m_Radius            = 1.0f;
        public float                    radius              { get { return m_Radius; } set { m_Radius = value; } }
        private int                     m_CurAnimationIndex = -1;                       // 当前播放动画的animationIndex, based on prototype.aniInfos[m_CurAnimationIndex]
        private float                   m_CurFrameIndex;                                // 当前播放动画的帧序号
        private int                     m_PreAnimationIndex = -1;                       // 上一个播放动画的animationIndex, based on prototype.aniInfos[m_CurAnimationIndex]
        private float                   m_PreFrameIndex;                                // 上一个播放动画的帧序号
        private WrapMode                m_WrapMode;                                     // 当前正在播放动画的wrapmode
        private float                   m_TransitionDuration;                           // 过渡时间
        private float                   m_TransitionTime;
        private bool                    m_isInTransition;
        public float                    transitionProgress  { get; private set; }
        public ShadowCastingMode        shadowCastingMode   = ShadowCastingMode.On;
        public bool                     receiveShadows      = false;
        public BoundingSphere           boundingSphere;
        [NonSerialized] public int      layer;
        [NonSerialized] public bool     isCulled;                                       // 是否被裁剪
        public string                   defaultAnim;
        private int                     m_TriggerEventIndex = -1;                       // 已触发的动画事件
        private bool                    m_isAlreadyTriggerEndEvent;                     // 是否已触发动画结束回调
        public float                    lodFrequency        { private get; set; }       = 0.5f;
        private float                   m_LodFrequencyCount = float.MaxValue;
        public float[]                  lodDistance         = new float[2];
        private int                     m_FixedLodLevel     = -1;
        private bool                    m_CachedPause;
        public bool                     isPause             { get; set; }
        public bool                     isPlaying           { get { return m_CurAnimationIndex >= 0 && !isPause; } }
        public bool                     isLoop              { get { return m_WrapMode == WrapMode.Loop; } }
        private Dictionary<string, AttachmentInfo> m_AttachmentInfo = new Dictionary<string, AttachmentInfo>();

        private void Awake()
        {
            if(m_Prototype == null)
                throw new ArgumentNullException("prototype[AnimationData]");

            worldTransform = GetComponent<Transform>();
            boundingSphere = new BoundingSphere(worldTransform.position, m_Radius);
            layer = gameObject.layer;

            m_PreAnimationIndex = -1;
            m_CurAnimationIndex = -1;
            m_PreFrameIndex = 0;
            m_CurFrameIndex = 0;

            animDataInst = AnimationDataManager.Instance.Load(m_Prototype);

            AnimationInstancingManager.Instance.AddInstance(this);
        }

        private void Start()
        {
            if(!string.IsNullOrEmpty(defaultAnim))
            {
                PlayAnimation(defaultAnim);
            }
            else
            {
                PlayAnimation(0);
            }
        }

        private void OnDestroy()
        {
            if(!AnimationDataManager.IsDestroy())
            {
                AnimationDataManager.Instance.Unload(m_Prototype);
            }

            if(!AnimationInstancingManager.IsDestroy())
            {
                AnimationInstancingManager.Instance.RemoveInstance(this);
            }
        }

        private void OnEnable()
        {
            isPause = m_CachedPause;
        }

        private void OnDisable()
        {
            m_CachedPause = isPause;
            isPause = true;
        }

        public bool ShouldRender()
        {
            return isActiveAndEnabled && !isCulled;
        }

        public delegate void OverridePropertyBlockHandler(int materialIndex, MaterialPropertyBlock block);
        public event OverridePropertyBlockHandler onOverridePropertyBlock;

        public void ExecutePropertyBlock(int materialIndex, MaterialPropertyBlock block)
        {
            onOverridePropertyBlock?.Invoke(materialIndex, block);
        }

        public int NameToID(string name)
        {
            return animDataInst.FindAnimationInfoIndex(name.GetHashCode());
        }

        public void PlayAnimation(string name, float transitionDuration = 0)
        {
            int id = NameToID(name);
            Debug.Assert(id != -1);
            PlayAnimation(id, transitionDuration);
        }

        public void PlayAnimation(int id, float transitionDuration = 0)
        {
            if(id < 0 || animDataInst.aniInfos == null || id >= animDataInst.aniInfos.Count)
                throw new ArgumentException($"animationIndex({id}) out of range");

            if(id == m_CurAnimationIndex && isPlaying)
                return;

            // 触发当前动画的结束回调
            if(m_CurAnimationIndex >= 0)
            {
                if(!m_isAlreadyTriggerEndEvent)
                {
                    m_isAlreadyTriggerEndEvent = true;
                    OnAnimationEnd?.Invoke(animDataInst.aniInfos[m_CurAnimationIndex].name);
                }
            }
            
            // update transition
            m_TransitionDuration = transitionDuration;
            m_TransitionTime = 0;
            if(m_TransitionDuration > 0)
            {
                m_isInTransition = true;
                transitionProgress = 0;
                
                m_PreAnimationIndex = m_CurAnimationIndex;
                m_PreFrameIndex = Mathf.Round(m_CurFrameIndex);
                m_CurAnimationIndex = id;
                m_CurFrameIndex = 0;
            }
            else
            {
                m_isInTransition = false;
                transitionProgress = 1.0f;

                m_PreAnimationIndex = -1;
                m_PreFrameIndex = -1;
                m_CurAnimationIndex = id;
                m_CurFrameIndex = 0;
            }

            // 触发当前动画的开始回调
            if(m_CurAnimationIndex >= 0)
            {
                OnAnimationBegin?.Invoke(animDataInst.aniInfos[m_CurAnimationIndex].name);
            }

            // reset variants
            isPause = false;
            m_isAlreadyTriggerEndEvent = false;
            m_speedParameter = 1.0f;
            m_TriggerEventIndex = -1;
            m_WrapMode = animDataInst.aniInfos[id].wrapMode;
        }

        public AnimationInfo GetCurrentAnimationInfo()
        {
            if(animDataInst.aniInfos != null && m_CurAnimationIndex >= 0 && m_CurAnimationIndex < animDataInst.aniInfos.Count)
            {
                return animDataInst.aniInfos[m_CurAnimationIndex];
            }
            return null;
        }

        public AnimationInfo GetPreAnimationInfo()
        {
            if(animDataInst.aniInfos != null && m_PreAnimationIndex >= 0 && m_PreAnimationIndex < animDataInst.aniInfos.Count)
            {
                return animDataInst.aniInfos[m_PreAnimationIndex];
            }
            return null;
        }

        public LODInfo GetCurrentLODInfo()
        {
            return lodInfos[lodLevel];
        }

        // 当前动画帧在AnimTexture的帧号
        public float GetGlobalCurFrameIndex()
        {
             AnimationInfo info = GetCurrentAnimationInfo();
             return info != null ? (info.startFrameIndex + m_CurFrameIndex) : -1;
        }

        // 上一个动画在AnimTexture的帧号
        public float GetGlobalPreFrameIndex()
        {
            AnimationInfo info = GetPreAnimationInfo();
            return info != null ? (info.startFrameIndex + m_PreFrameIndex) : -1;
        }

        private int m_LodLevel = -1;
        public int lodLevel            
        { 
            get { return m_LodLevel; }
            private set
            {
                if(m_LodLevel != value)
                {
                    m_LodLevel = Mathf.Clamp(value, 0, lodInfos.Count);

                    // 把当前lod注册到manager
                    AnimationInstancingManager.Instance.AddVertexCache(this, lodInfos[m_LodLevel]);
                }
            }
        }

        // 强制使用某lod等级，设置lodLevel < 0表示返回常规lod流程
        public void ForceLOD(int lodLevel)
        {
            m_FixedLodLevel = lodLevel;
            if(lodLevel >= 0)
                m_FixedLodLevel = Mathf.Min(lodLevel, lodInfos.Count - 1);
        }

        public void UpdateLod(Vector3 cameraPosition)
        {
            if(m_FixedLodLevel > -1)
            {
                lodLevel = m_FixedLodLevel;
                return;
            }

            m_LodFrequencyCount += Time.deltaTime;
            if(m_LodFrequencyCount > lodFrequency)
            {
                m_LodFrequencyCount = 0;

                int level = 0;
                float distSqr = (cameraPosition - worldTransform.position).sqrMagnitude;
                if(distSqr < lodDistance[0] * lodDistance[0])
                    level = 0;
                else if(distSqr < lodDistance[1] * lodDistance[1])
                    level = 1;
                else
                    level = 2;
                lodLevel = Mathf.Clamp(level, 0, lodInfos.Count - 1);
            }
        }

        public void UpdateAnimation()
        {
            if(!isPlaying || !isActiveAndEnabled || isCulled)
            {
                return;
            }

            // 过渡时期上一个动画固定在最后一帧，节省vs开销
            if(m_isInTransition)
            {
                m_TransitionTime += Time.deltaTime;
                transitionProgress = Mathf.Min(m_TransitionTime / m_TransitionDuration, 1);
                if(transitionProgress >= 1.0f)
                {
                    m_isInTransition = false;
                    m_PreAnimationIndex = -1;
                    m_PreFrameIndex = -1;
                }
            }
            
            m_CurFrameIndex += playSpeed * m_speedParameter * Time.deltaTime * animDataInst.aniInfos[m_CurAnimationIndex].fps;
            int totalFrame = animDataInst.aniInfos[m_CurAnimationIndex].totalFrame;
            switch(m_WrapMode)
            {
                case WrapMode.Loop:
                    {
                        if(m_CurFrameIndex < 0)
                        {
                            m_CurFrameIndex += (m_CurFrameIndex / (totalFrame - 1) + 1) * (totalFrame - 1);
                            m_TriggerEventIndex = -1;
                        }
                        else if(m_CurFrameIndex > totalFrame - 1)
                        {
                            m_CurFrameIndex -= m_CurFrameIndex / (totalFrame - 1) * (totalFrame - 1);
                            m_TriggerEventIndex = -1;
                        }
                    }
                    break;
                case WrapMode.Default:
                case WrapMode.Clamp:
                    {
                        if(m_CurFrameIndex < 0 || m_CurFrameIndex > (totalFrame - 1))
                        {
                            isPause = true;

                            if(!m_isAlreadyTriggerEndEvent)
                            {
                                m_isAlreadyTriggerEndEvent = true;
                                OnAnimationEnd?.Invoke(animDataInst.aniInfos[m_CurAnimationIndex].name);
                            }
                        }
                    }
                    break;
                case WrapMode.PingPong:
                    {
                        if(m_CurFrameIndex < 0)
                        {
                            m_speedParameter = Mathf.Abs(m_speedParameter);
                            m_CurFrameIndex = Mathf.Abs(m_CurFrameIndex);
                            m_TriggerEventIndex = -1;
                        }
                        else if(m_CurFrameIndex > totalFrame - 1)
                        {
                            m_speedParameter = -Mathf.Abs(m_speedParameter);
                            m_CurFrameIndex = 2 * (totalFrame - 1) - m_CurFrameIndex;
                            m_TriggerEventIndex = -1;
                        }
                    }
                    break;
            }
            m_CurFrameIndex = Mathf.Clamp(m_CurFrameIndex, 0, totalFrame - 1);

            UpdateAnimationEvent();
            UpdateAttachment();
        }

        private void UpdateAnimationEvent()
        {
            UnityEngine.Profiling.Profiler.BeginSample("UpdateAnimationEvent()");
            AnimationInfo info = GetCurrentAnimationInfo();
            if(info == null || info.eventList.Count == 0)
                return;

            float time = m_CurFrameIndex / info.fps;
            int eventCount = info.eventList.Count;
            int firstEventIndex = m_TriggerEventIndex + 1;
            int lastEventIndex = -1;
            for(int i = firstEventIndex; i < eventCount; ++i)
            {
                if(info.eventList[i].time > time)
                {
                    break;
                }
                lastEventIndex = i;
            }

            // no fire
            if(lastEventIndex == -1)
                return;

            // fire event
            for(int i = firstEventIndex; i <= lastEventIndex; ++i)
            {
                OnAnimationEvent?.Invoke(info.name, info.eventList[i].function, info.eventList[i]);
            }
            m_TriggerEventIndex = lastEventIndex;
            UnityEngine.Profiling.Profiler.EndSample();
        }        

        // 返回index，可用于detach，对性能更友好
        public int Attach(string boneName, IAttachmentToInstancing attachment)
        {
            AttachmentInfo info;
            if(!m_AttachmentInfo.TryGetValue(boneName, out info))
            {
                info = new AttachmentInfo();
                info.boneName = boneName;
                info.extraBoneInfo = animDataInst.GetExtraBoneInfo(boneName);
                Debug.Assert(info.extraBoneInfo != null);

                m_AttachmentInfo.Add(boneName, info);
            }
            
            int index = info.AddAttachment(attachment);
            if(index != -1)
            {
                attachment.SetParent(transform);
                UpdateAttachment();                 // update immediately
            }
            return index;
        }

        public void Detach(string boneName, IAttachmentToInstancing attachment)
        {
            AttachmentInfo info;
            if(!m_AttachmentInfo.TryGetValue(boneName, out info))
            {
                Debug.LogWarning($"{boneName} does not have any attachments");
                return;
            }
            info.RemoveAttachment(attachment);
            if(info.count == 0)
            {
                m_AttachmentInfo.Remove(boneName);
            }
        }

        public void Detach(string boneName, int index)
        {
            AttachmentInfo info;
            if(!m_AttachmentInfo.TryGetValue(boneName, out info))
            {
                Debug.LogWarning($"{boneName} does not have any attachments");
                return;
            }
            info.RemoveAttachment(index);
            if(info.count == 0)
            {
                m_AttachmentInfo.Remove(boneName);
            }
        }

        public Vector3 GetExtraBonePosition(string boneName)
        {
            AnimationInfo info = GetCurrentAnimationInfo();
            if(info == null)
            {
                Debug.LogWarning($"GetExtraBonePosition() failed, because of not playing    ({boneName})");
                return Vector3.negativeInfinity;
            }

            Matrix4x4[] matrixs;
            if(!info.extraBoneMatrix.TryGetValue(boneName, out matrixs))
            {
                Debug.LogWarning($"GetExtraBonePosition() failed, ({boneName}) does not export as extra bone, plz check it");
                return Vector3.negativeInfinity;
            }

            return GetFramePosition(matrixs, m_CurFrameIndex);
        }

        public Quaternion GetExtraBoneRotation(string boneName)
        {
            AnimationInfo info = GetCurrentAnimationInfo();
            if(info == null)
            {
                Debug.LogWarning($"GetExtraBoneRotation() failed, because of not playing    ({boneName})");
                return Quaternion.identity;
            }

            Matrix4x4[] matrixs;
            if(!info.extraBoneMatrix.TryGetValue(boneName, out matrixs))
            {
                Debug.LogWarning($"GetExtraBoneRotation() failed, ({boneName}) does not export as extra bone, plz check it");
                return Quaternion.identity;
            }
            return GetFrameRotation(matrixs, m_CurFrameIndex);
        }

        public void GetExtraBonePositionAndRotation(string boneName, ref Vector3 position, ref Quaternion rotation)
        {
            AnimationInfo info = GetCurrentAnimationInfo();
            if(info == null)
            {
                Debug.LogWarning($"GetExtraBoneRotation() failed, because of not playing    ({boneName})");
                return;
            }

            Matrix4x4[] matrixs;
            if(!info.extraBoneMatrix.TryGetValue(boneName, out matrixs))
            {
                Debug.LogWarning($"GetExtraBoneRotation() failed, ({boneName}) does not export as extra bone, plz check it");
                return;
            }

            position = GetFramePosition(matrixs, m_CurFrameIndex);
            rotation = GetFrameRotation(matrixs, m_CurFrameIndex);
        }

        private void UpdateAttachment()
        {
            UnityEngine.Profiling.Profiler.BeginSample("UpdateAttachment()");
            foreach(var item in m_AttachmentInfo)
            {
                string boneName = item.Key;
                AttachmentInfo info = item.Value;
                for(int i = 0; i < AttachmentInfo.s_MaxCountAttachment; ++i)
                {
                    if(info.attachments[i] == null)
                        continue;

                    Matrix4x4 worldMatrix = transform.localToWorldMatrix * GetFrameMatrix(info.extraBoneInfo.boneMatrix[m_CurAnimationIndex],
                                                                                         m_CurFrameIndex);
                    info.attachments[i].SetPosition(worldMatrix.MultiplyPoint3x4(Vector3.zero));
                    info.attachments[i].SetRotation(worldMatrix.rotation);
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }

        private Matrix4x4 GetFrameMatrix(Matrix4x4[] matrixs, float frameIndex)
        {
            Quaternion rot = GetFrameRotation(matrixs, frameIndex);
            Vector3 pos = GetFramePosition(matrixs, frameIndex);
            return Matrix4x4.TRS(pos, rot, Vector3.one);
        }

        private Vector3 GetFramePosition(Matrix4x4[] matrixs, float frameIndex)
        {
            int curFrame = (int)frameIndex;
            int nextFrame = Mathf.Clamp(curFrame + 1, 0, matrixs.Length);

            Vector3 curPos = matrixs[curFrame].GetColumn(3);
            Vector3 nextPos = matrixs[nextFrame].GetColumn(3);
            return Vector3.Lerp(curPos, nextPos, frameIndex - curFrame);
        }

        private Quaternion GetFrameRotation(Matrix4x4[] matrixs, float frameIndex)
        {
            int curFrame = (int)frameIndex;
            int nextFrame = Mathf.Clamp(curFrame + 1, 0, matrixs.Length);
            
            Quaternion curRot = matrixs[curFrame].rotation;
            Quaternion nextRot = matrixs[nextFrame].rotation;
            return Quaternion.Slerp(curRot, nextRot, frameIndex - curFrame);
        }
    }
}