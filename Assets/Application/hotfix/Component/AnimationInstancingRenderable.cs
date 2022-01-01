using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimationInstancingModule.Runtime;

namespace Application.HotFix
{
    public class AnimationInstancingRenderable : Renderable
    {
        private AnimationInstancing m_Inst;
        private string m_CachedName;
        private float m_CachedTransitionDuration;
        private float m_CachedPlaySpeed;

        public override bool enable
        {
            get { return m_Enable; }
            set
            {
                if(m_Enable != value)
                {
                    m_Enable = value;

                    if(m_Inst != null)
                    {
                        m_Inst.isShow = m_Enable;       // 不能SetActive(false)，因AnimationInstancing.OnDisable被用于销毁时使用
                    }
                }
            }
        }
        
        public AnimationInstancingRenderable(ZActor actor) : base(actor) {}

        public override void Load(string assetPath)
        {
            base.Load(assetPath);
            m_Inst = renderer.GetComponent<AnimationInstancing>();

            if(!string.IsNullOrEmpty(m_CachedName))
            {
                PlayAnimation(m_CachedName, m_CachedTransitionDuration, m_CachedPlaySpeed);
            }
            else
            {
                PlayAnimation("idle");
            }
        }

        public void PlayAnimation(string name, float transitionDuration = 0, float playSpeed = 1)
        {
            if(m_Inst != null)
            {
                m_Inst.PlayAnimation(name, transitionDuration);
                m_Inst.playSpeed = playSpeed;
            }
            else
            { // 异步加载机制可能导致尚未实例化，故缓存
                m_CachedName = name;
                m_CachedTransitionDuration = transitionDuration;
                m_CachedPlaySpeed = playSpeed;
            }
        }
    }
}