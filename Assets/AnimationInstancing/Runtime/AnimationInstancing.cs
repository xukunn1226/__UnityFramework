using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;

namespace AnimationInstancingModule.Runtime
{
    public class AnimationInstancing : MonoBehaviour
    {
        public AnimationData            prototype;
        public List<LODInfo>            lodInfos            = new List<LODInfo>();

        public Transform                worldTransform      { get; private set; }
        [SerializeField] private float  m_Speed             = 1;                        // 序列化数据
        public float                    speedScale          { get; set; } = 1;
        private float                   m_CacheSpeedScale;
        public float                    playSpeed           { get { return m_Speed * speedScale; } }
        [SerializeField] private float  m_Radius            = 1.0f;
        public float                    radius              { get { return m_Radius; } set { m_Radius = value; } }
        private int                     m_CurAnimationIndex = -1;                       // 当前播放动画的animationIndex, based on prototype.aniInfos[m_CurAnimationIndex]
        private int                     m_CurFrameIndex;                                // 当前播放动画的帧序号
        private int                     m_PreAnimationIndex = -1;                       // 上一个播放动画的animationIndex, based on prototype.aniInfos[m_CurAnimationIndex]
        private int                     m_PreFrameIndex;                                // 上一个播放动画的帧序号
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
        private int                     m_BonePerVertex     = 4;

        private void Awake()
        {
            if(prototype == null)
                throw new ArgumentNullException("prototype[AnimationData]");

            worldTransform = GetComponent<Transform>();
            m_BoundingSphere = new BoundingSphere(worldTransform.position, m_Radius);
            layer = gameObject.layer;

            switch(QualitySettings.skinWeights)
            {
                case SkinWeights.OneBone:
                    m_BonePerVertex = 1;
                    break;
                case SkinWeights.TwoBones:
                    m_BonePerVertex = 2;
                    break;
            }

            // register
            // 把当前lod注册到manager
        }

        private void OnDestroy()
        {
            // unregister
        }

        private void OnEnable()
        {
            visible = true;

            speedScale = m_CacheSpeedScale;
        }

        private void OnDisable()
        {
            visible = false;

            m_CacheSpeedScale = speedScale;
            speedScale = 0;
        }

        public void PlayAnimation(string name, float transitionDuration = 0)
        {
            int hash = name.GetHashCode();
            PlayAnimation(prototype.FindAnimationInfo(hash), transitionDuration);
        }

        public void PlayAnimation(int animationIndex, float transitionDuration = 0)
        {
            if(animationIndex < 0 || animationIndex >= prototype.aniInfos.Count)
                throw new ArgumentException($"animationIndex({animationIndex}) out of range");

            if(animationIndex == m_CurAnimationIndex)
                return;

            m_TransitionDuration = transitionDuration;
            m_TransitionTime = 0;
            if(m_TransitionDuration > 0)
            {
                m_isInTransition = true;
                transitionProgress = 0;
            }
            else
            {
                m_isInTransition = false;
                transitionProgress = 1.0f;
            }

            m_PreAnimationIndex = m_CurAnimationIndex;
            m_CurAnimationIndex = animationIndex;
            m_PreFrameIndex = (int)(m_CurFrameIndex + 0.5f);
            m_CurFrameIndex = 0;
            m_WrapMode = prototype.aniInfos[animationIndex].wrapMode;
        }

        public void Stop()
        {
            m_PreAnimationIndex = -1;
            m_CurAnimationIndex = -1;
            m_CurFrameIndex = 0;
        }

        public bool IsPlaying()
        {
            return m_CurAnimationIndex >= 0;
        }

        public bool IsPause()
        {
            return speedScale == 0;
        }

        public bool IsLoop()
        {
            return m_WrapMode == WrapMode.Loop;
        }

        public void Pause()
        {
            if(speedScale != 0)
            {
                m_CacheSpeedScale = speedScale;
                speedScale = 0;
            }
        }

        public void Resume()
        {
            speedScale = m_CacheSpeedScale;
        }

        public AnimationInfo GetCurrentAnimationInfo()
        {
            if(prototype.aniInfos != null && m_CurAnimationIndex >= 0 && m_CurAnimationIndex < prototype.aniInfos.Count)
            {
                return prototype.aniInfos[m_CurAnimationIndex];
            }
            return null;
        }

        public AnimationInfo GetPreAnimationInfo()
        {
            if(prototype.aniInfos != null && m_PreAnimationIndex >= 0 && m_PreAnimationIndex < prototype.aniInfos.Count)
            {
                return prototype.aniInfos[m_PreAnimationIndex];
            }
            return null;
        }

        public void UpdateAnimation()
        {
            if(IsPause())
            {
                return;
            }
        }
    }
}