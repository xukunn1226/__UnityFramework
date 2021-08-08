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
        private float                   m_BlendTime;                                    // 过渡时间
        private float                   m_Time;
        private bool                    m_isInBlend;
        public float                    blendAlpha          { get; private set; }
        public ShadowCastingMode        shadowCastingMode   = ShadowCastingMode.On;
        public bool                     receiveShadows      = false;
        private BoundingSphere          m_BoundingSphere;
        [NonSerialized] public int      layer;
        [NonSerialized] public bool     visible             = true;
        [Range(1, 4)] public int        bonePerVertex       = 4;

        private void Awake()
        {
            if(prototype == null)
                throw new ArgumentNullException("prototype[AnimationData]");

            worldTransform = GetComponent<Transform>();
            m_BoundingSphere = new BoundingSphere(worldTransform.position, m_Radius);
            layer = gameObject.layer;

            // register
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

        public void PlayAnimation(string name, float speedScale = 1.0f)
        {
            int hash = name.GetHashCode();
            PlayAnimation(prototype.FindAnimationInfo(hash), speedScale);
        }

        public void PlayAnimation(int animationIndex, float speedScale = 1.0f)
        {
            if(animationIndex < 0 || animationIndex >= prototype.aniInfos.Count)
                throw new ArgumentException($"animationIndex({animationIndex}) out of range");

            if(animationIndex == m_CurAnimationIndex)
                return;

            m_BlendTime = 0;
            blendAlpha = 1.0f;
            m_isInBlend = false;

            m_PreAnimationIndex = m_CurAnimationIndex;
            m_CurAnimationIndex = animationIndex;
            m_PreFrameIndex = (int)(m_CurFrameIndex + 0.5f);
            m_CurFrameIndex = 0;
        }
    }
}