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
        private int                     m_CurAnimationIndex = -1;                       // 当前播放动画的animationIndex（在整个AnimationTexture中的起始帧序号，同AnimationInfo.animationIndex）
        private int                     m_CurFrameIndex;                                // 当前播放动画的帧序号
        private int                     m_PreAnimationIndex = -1;                       // 上一个播放动画的animationIndex
        private int                     m_PreFrameIndex;                                // 上一个播放动画的帧序号
        private float                   m_BlendTime;                                    // 过渡时间
        private float                   m_Time;
        public ShadowCastingMode        shadowCastingMode   = ShadowCastingMode.On;
        public bool                     receiveShadows      = false;
        private BoundingSphere          m_BoundingSphere;
        [NonSerialized] public int      layer;
        [NonSerialized] public bool     visible             = true;

        private void Awake()
        {
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
        }

        private void OnDisable()
        {
            visible = false;
        }
    }
}