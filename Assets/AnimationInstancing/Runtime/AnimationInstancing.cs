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

        public AnimationData            prototype;                                      // WARNING: 资源，非实例化数据
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
        private BoundingSphere          m_BoundingSphere;
        [NonSerialized] public int      layer;
        [NonSerialized] public bool     visible             = true;
        public string                   defaultAnim;
        private int                     m_TriggerEventIndex = -1;                       // 已触发的动画事件
        private bool                    m_isAlreadyTriggerEndEvent;                     // 是否已触发动画结束回调
        private bool                    m_CachedPause;
        public bool                     isPause             { get; set; }
        public bool                     isPlaying           { get { return m_CurAnimationIndex >= 0 && !isPause; } }
        public bool                     isLoop              { get { return m_WrapMode == WrapMode.Loop; } }

        private void Awake()
        {
            if(prototype == null)
                throw new ArgumentNullException("prototype[AnimationData]");

            animDataInst = AnimationDataManager.Instance.Load(prototype);

            worldTransform = GetComponent<Transform>();
            m_BoundingSphere = new BoundingSphere(worldTransform.position, m_Radius);
            layer = gameObject.layer;

            m_PreAnimationIndex = -1;
            m_CurAnimationIndex = -1;
            m_PreFrameIndex = 0;
            m_CurFrameIndex = 0;

            // register
            AnimationInstancingManager.Instance.AddInstance(this);

            // lodLevel = 0;
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
            AnimationDataManager.Instance.Unload(prototype);
            
            // unregister
            AnimationInstancingManager.Instance.RemoveInstance(this);
        }

        private void OnEnable()
        {
            visible = true;

            isPause = m_CachedPause;
        }

        private void OnDisable()
        {
            visible = false;

            m_CachedPause = isPause;
            isPause = true;
        }

        public void PlayAnimation(string name, float transitionDuration = 0)
        {
            int hash = name.GetHashCode();
            PlayAnimation(animDataInst.FindAnimationInfoIndex(hash), transitionDuration);
        }

        public void PlayAnimation(int animationIndex, float transitionDuration = 0)
        {
            if(animationIndex < 0 || animDataInst.aniInfos == null || animationIndex >= animDataInst.aniInfos.Count)
                throw new ArgumentException($"animationIndex({animationIndex}) out of range");

            if(animationIndex == m_CurAnimationIndex && isPlaying)
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
                m_CurAnimationIndex = animationIndex;
                m_CurFrameIndex = 0;
            }
            else
            {
                m_isInTransition = false;
                transitionProgress = 1.0f;

                m_PreAnimationIndex = -1;
                m_PreFrameIndex = -1;
                m_CurAnimationIndex = animationIndex;
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
            m_WrapMode = animDataInst.aniInfos[animationIndex].wrapMode;
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

        public void UpdateLod()
        {
            lodLevel = 0;            
        }

        public void UpdateAnimation()
        {
            if(isPause)
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
        }

        private void UpdateAnimationEvent()
        {
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
        }
    }
}